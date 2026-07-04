import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';
import { catchError, switchMap, throwError } from 'rxjs';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  let requestUrl = req.url;
  if (requestUrl.startsWith('http://localhost:5000')) {
    if (!window.location.hostname.includes('localhost')) {
      requestUrl = requestUrl.replace('http://localhost:5000', window.location.origin);
    }
  }

  const rewrittenReq = req.clone({ url: requestUrl });

  let authReq = rewrittenReq;
  if (token) {
    authReq = rewrittenReq.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((err) => {
      if (err.status === 401 && !rewrittenReq.url.includes('/refresh-token')) {
        return authService.refreshToken().pipe(
          switchMap((res) => {
            if (res && res.isSuccess && res.data) {
              const newToken = res.data.token;
              const retryReq = rewrittenReq.clone({
                setHeaders: {
                  Authorization: `Bearer ${newToken}`
                }
              });
              return next(retryReq);
            }
            authService.logout();
            return throwError(() => err);
          }),
          catchError((refreshErr) => {
            authService.logout();
            return throwError(() => refreshErr);
          })
        );
      } else if (err.status === 401) {
        authService.logout();
      }
      return throwError(() => err);
    })
  );
};
