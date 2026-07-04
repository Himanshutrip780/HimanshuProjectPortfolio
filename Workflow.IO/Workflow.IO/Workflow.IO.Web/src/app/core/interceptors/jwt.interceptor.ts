import {
  HttpErrorResponse,
  HttpHandlerFn,
  HttpInterceptorFn,
  HttpRequest,
} from '@angular/common/http';
import { inject, Injector } from '@angular/core';
import { catchError, finalize, switchMap, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  ANONYMOUS_GATEWAY_PATHS,
  GATEWAY_PATH_PREFIXES,
} from '../constants/gateway-paths.constants';
import { AuthenticationResponse } from '../models/user.models';
import { AuthService } from '../services/auth.service';
import { TokenStorageService } from '../services/token-storage.service';

let refreshInFlight: ReturnType<AuthService['refreshToken']> | null = null;

function resolvePathname(url: string): string {
  try {
    return new URL(url, window.location.origin).pathname;
  } catch {
    return url.split('?')[0] ?? url;
  }
}

function isGatewayRequest(url: string): boolean {
  const pathname = resolvePathname(url);

  let requestOrigin: string;
  try {
    requestOrigin = new URL(url).origin;
  } catch {
    requestOrigin = window.location.origin;
  }

  const isSameOrigin = requestOrigin === window.location.origin;
  
  let isGatewayOrigin = false;
  if (environment.apiGatewayUrl) {
    try {
      isGatewayOrigin = requestOrigin === new URL(environment.apiGatewayUrl).origin;
    } catch {
      isGatewayOrigin = url.startsWith(environment.apiGatewayUrl);
    }
  }

  if (!isSameOrigin && !isGatewayOrigin) {
    return false;
  }

  return GATEWAY_PATH_PREFIXES.some((prefix) => pathname.startsWith(prefix));
}

function isAnonymousGatewayRequest(url: string): boolean {
  const pathname = resolvePathname(url);
  return [...ANONYMOUS_GATEWAY_PATHS].some((segment) =>
    pathname.endsWith(segment),
  );
}

function attachBearerToken(
  request: HttpRequest<unknown>,
  token: string,
  orgId: string | null,
): HttpRequest<unknown> {
  const headers = request.headers.set('Authorization', `Bearer ${token}`);
  
  if (orgId) {
    return request.clone({ headers: headers.set('X-Organization-ID', orgId) });
  }
  
  return request.clone({ headers });
}

function queueRefresh(
  auth: AuthService,
): ReturnType<AuthService['refreshToken']> {
  if (!refreshInFlight) {
    refreshInFlight = auth.refreshToken().pipe(
      finalize(() => {
        refreshInFlight = null;
      }),
    );
  }

  return refreshInFlight;
}

export const jwtInterceptor: HttpInterceptorFn = (
  request,
  next: HttpHandlerFn,
) => {
  const tokenStorage = inject(TokenStorageService);
  const injector = inject(Injector);

  if (!isGatewayRequest(request.url)) {
    return next(request);
  }

  const accessToken = tokenStorage.getAccessToken();
  const orgId = tokenStorage.getActiveOrganizationId();
  const authedRequest =
    accessToken && !isAnonymousGatewayRequest(request.url)
      ? attachBearerToken(request, accessToken, orgId)
      : request;

  return next(authedRequest).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 403) {
        const isForbiddenWorkspace =
          error.error === 'Access to this workspace is forbidden.' ||
          (typeof error.error === 'string' && error.error.includes('workspace is forbidden')) ||
          error.message?.includes('Access to this workspace is forbidden');

        if (isForbiddenWorkspace) {
          const auth = injector.get(AuthService);
          auth.logout();
          return throwError(() => error);
        }
      }

      if (
        !(error instanceof HttpErrorResponse) ||
        error.status !== 401 ||
        isAnonymousGatewayRequest(request.url)
      ) {
        return throwError(() => error);
      }

      const auth = injector.get(AuthService);
      return queueRefresh(auth).pipe(
        switchMap((authResponse: AuthenticationResponse | null) => {
          const refreshedToken = tokenStorage.getAccessToken();
          if (!authResponse || !refreshedToken) {
            auth.logout();
            return throwError(() => error);
          }

          return next(attachBearerToken(request, refreshedToken, orgId));
        }),
        catchError((refreshError) => {
          auth.logout();
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
