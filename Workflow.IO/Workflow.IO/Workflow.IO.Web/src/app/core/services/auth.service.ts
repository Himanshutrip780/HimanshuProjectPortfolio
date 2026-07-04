import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  catchError,
  finalize,
  map,
  Observable,
  of,
  switchMap,
  tap,
  throwError,
} from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import {
  AuthenticationRequest,
  AuthenticationResponse,
  AuthSessionUser,
  RegisterUserRequest,
  UserDto,
  UserProfile,
} from '../models/user.models';
import { apiUrl } from '../utils/api-url.util';
import { toAuthSessionUser } from '../utils/jwt.util';
import { ApiErrorService } from './api-error.service';
import { RealtimeService } from './realtime.service';
import { TokenStorageService } from './token-storage.service';
import { UserService } from './user.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);
  private readonly apiErrors = inject(ApiErrorService);
  private readonly userService = inject(UserService);
  private readonly router = inject(Router);
  private readonly realtime = inject(RealtimeService);

  private readonly usersBase = apiUrl(environment.apiGatewayUrl, '/users');

  private readonly _currentUser = signal<AuthSessionUser | null>(null);
  private readonly _isAuthenticated = signal(false);
  private readonly _isLoading = signal(false);
  private readonly _userAvatar = signal<string | null>(null);

  readonly currentUser = this._currentUser.asReadonly();
  readonly isAuthenticated = this._isAuthenticated.asReadonly();
  readonly isLoading = this._isLoading.asReadonly();
  readonly userAvatar = this._userAvatar.asReadonly();
  readonly lastError = this.apiErrors.lastError;
  readonly fieldErrors = this.apiErrors.fieldErrors;

  readonly displayName = computed(() => {
    const user = this._currentUser();
    if (!user) {
      return '';
    }

    if (user.firstName || user.lastName) {
      return `${user.firstName ?? ''} ${user.lastName ?? ''}`.trim();
    }

    if (user.email) {
      const parts = user.email.split('@')[0].split(/[._-]/);
      return parts.map(p => p.charAt(0).toUpperCase() + p.slice(1)).join(' ');
    }

    return user.email;
  });

  constructor() {
    this.restoreSessionFromStorage();
  }

  login(request: AuthenticationRequest): Observable<AuthSessionUser> {
    this.apiErrors.clear();
    this._isLoading.set(true);

    return this.http
      .post<ApiResponse<AuthenticationResponse>>(
        `${this.usersBase}/authenticate`,
        request,
      )
      .pipe(
        map((response) => this.unwrapAuthResponse(response)),
        tap((auth) => this.persistTokens(auth)),
        switchMap((auth) => this.loadProfileFromApi(auth)),
        catchError((error) => {
          this.apiErrors.capture(error, 'Login failed');
          return throwError(() => error);
        }),
        finalize(() => this._isLoading.set(false)),
      );
  }

  register(request: RegisterUserRequest): Observable<UserDto> {
    this.apiErrors.clear();
    this._isLoading.set(true);

    return this.http
      .post<ApiResponse<UserDto>>(`${this.usersBase}/register`, request)
      .pipe(
        map((response) => this.unwrapData(response)),
        catchError((error) => {
          this.apiErrors.capture(error, 'Registration failed');
          return throwError(() => error);
        }),
        finalize(() => this._isLoading.set(false)),
      );
  }

  sendOtp(request: RegisterUserRequest): Observable<any> {
    this.apiErrors.clear();
    this._isLoading.set(true);

    return this.http
      .post<ApiResponse<any>>(`${this.usersBase}/send-otp`, request)
      .pipe(
        map((response) => this.unwrapData(response)),
        catchError((error) => {
          this.apiErrors.capture(error, 'Failed to send verification code');
          return throwError(() => error);
        }),
        finalize(() => this._isLoading.set(false)),
      );
  }

  verifyOtp(email: string, code: string): Observable<UserDto> {
    this.apiErrors.clear();
    this._isLoading.set(true);

    return this.http
      .post<ApiResponse<UserDto>>(`${this.usersBase}/verify-otp`, { email, code })
      .pipe(
        map((response) => this.unwrapData(response)),
        catchError((error) => {
          this.apiErrors.capture(error, 'Verification failed');
          return throwError(() => error);
        }),
        finalize(() => this._isLoading.set(false)),
      );
  }

  resendOtp(email: string): Observable<any> {
    this.apiErrors.clear();
    this._isLoading.set(true);

    return this.http
      .post<ApiResponse<any>>(`${this.usersBase}/resend-otp`, { email })
      .pipe(
        map((response) => this.unwrapData(response)),
        catchError((error) => {
          this.apiErrors.capture(error, 'Failed to resend verification code');
          return throwError(() => error);
        }),
        finalize(() => this._isLoading.set(false)),
      );
  }

  checkStatus(email: string): Observable<string> {
    return this.http
      .get<ApiResponse<string>>(`${this.usersBase}/check-status`, { params: { email } })
      .pipe(
        map((response) => this.unwrapData(response)),
        catchError((error) => {
          return throwError(() => error);
        })
      );
  }

  logout(): void {
    this.tokenStorage.clear();
    this._currentUser.set(null);
    this._isAuthenticated.set(false);
    this._userAvatar.set(null);
    this._activeOrganizationId.set(null);
    this.apiErrors.clear();
    
    // Disconnect realtime connection on logout
    void this.realtime.disconnect();
    
    // Redirect to login page
    setTimeout(() => {
      this.router.navigate(['/auth/login']);
    });
  }

  private readonly _activeOrganizationId = signal<string | null>(
    this.tokenStorage.getActiveOrganizationId()
  );
  
  private readonly _myOrganizations = signal<import('../models/user.models').OrganizationDto[]>([]);
  readonly myOrganizations = this._myOrganizations.asReadonly();

  readonly activeOrganizationId = this._activeOrganizationId.asReadonly();

  readonly activeOrganization = computed(() => {
    const orgId = this._activeOrganizationId();
    if (!orgId) return null;
    return this._myOrganizations().find(o => o.organizationId === orgId) || null;
  });

  setActiveOrganization(orgId: string | null): void {
    this.tokenStorage.setActiveOrganizationId(orgId);
    this._activeOrganizationId.set(orgId);
  }

  loadOrganizations(): void {
    if (!this._isAuthenticated()) return;
    
    this.http.get<ApiResponse<import('../models/user.models').OrganizationDto>>(`${this.usersBase}/me/organization`)
      .pipe(
        map(res => this.unwrapData(res)),
        catchError(() => of(null))
      )
      .subscribe({
        next: (org) => {
          if (org) {
            this._myOrganizations.set([org]);
            if (!this._activeOrganizationId() || this._activeOrganizationId() !== org.organizationId) {
              this.setActiveOrganization(org.organizationId);
            }
          } else {
            this._myOrganizations.set([]);
          }
        }
      });
  }

  resolveOrganizationBySubdomain(subdomain: string): Promise<{ id: string } | null> {
    const url = `${this.usersBase}/organizations/by-subdomain/${subdomain}`;
    return new Promise((resolve) => {
      this.http.get<ApiResponse<any>>(url)
        .pipe(
          map(res => {
            if (res.success && res.data) {
              return { id: res.data.organizationId };
            }
            return null;
          }),
          catchError(() => of(null))
        )
        .subscribe(result => resolve(result));
    });
  }

  refreshToken(): Observable<AuthenticationResponse | null> {
    const refreshToken = this.tokenStorage.getRefreshToken();
    if (!refreshToken) {
      return of(null);
    }

    return this.http
      .post<ApiResponse<AuthenticationResponse>>(
        `${this.usersBase}/refresh`,
        { refreshToken },
      )
      .pipe(
        switchMap((response) => {
          const auth = this.unwrapAuthResponse(response);
          this.persistTokens(auth);
          return this.loadProfileFromApi(auth).pipe(map(() => auth));
        }),
        catchError(() => {
          this.logout();
          return of(null);
        }),
      );
  }

  syncProfile(): Observable<AuthSessionUser | null> {
    if (!this._isAuthenticated()) {
      return of(null);
    }

    return this.userService.getMe().pipe(
      tap((profile) => this.applyProfile(profile)),
      map(() => this._currentUser()),
      catchError(() => {
        this.logout();
        return of(null);
      }),
    );
  }

  private restoreSessionFromStorage(): void {
    const accessToken = this.tokenStorage.getAccessToken();
    if (!accessToken) {
      return;
    }

    const sessionUser = toAuthSessionUser(accessToken);
    if (!sessionUser) {
      this.logout();
      return;
    }

    this._currentUser.set(sessionUser);
    this._isAuthenticated.set(true);

    this.userService.getMe().subscribe({
      next: (profile) => this.applyProfile(profile),
      error: (err) => {
        if (err && (err.status === 401 || err.status === 403)) {
          this.logout();
        }
      },
    });
  }

  private loadProfileFromApi(
    auth: AuthenticationResponse,
  ): Observable<AuthSessionUser> {
    return this.userService.getMe().pipe(
      map((profile) => {
        this.applyProfile(profile, auth);
        return this._currentUser()!;
      }),
      catchError(() => {
        const sessionUser = auth.jwtToken
          ? toAuthSessionUser(auth.jwtToken)
          : null;

        if (!sessionUser) {
          this.logout();
          return throwError(() => new Error('Unable to load profile'));
        }

        this._currentUser.set({
          userId: sessionUser.userId,
          email: auth.email ?? sessionUser.email,
          role: auth.role ?? sessionUser.role,
        });
        this._isAuthenticated.set(true);
        return of(this._currentUser()!);
      }),
    );
  }

  updateUserAvatar(avatarDataUrl: string | null): void {
    const user = this._currentUser();
    if (user) {
      if (avatarDataUrl) {
        localStorage.setItem('workflow.io_avatar_' + user.userId, avatarDataUrl);
        this._userAvatar.set(avatarDataUrl);
      } else {
        localStorage.removeItem('workflow.io_avatar_' + user.userId);
        this._userAvatar.set(null);
      }
    }
  }

  private applyProfile(
    profile: UserProfile,
    auth?: AuthenticationResponse,
  ): void {
    const tokenUser = auth?.jwtToken
      ? toAuthSessionUser(auth.jwtToken)
      : null;

    this._currentUser.set({
      userId: profile.userId,
      email: auth?.email ?? tokenUser?.email ?? '',
      role: auth?.role ?? tokenUser?.role ?? 'User',
      firstName: profile.firstName,
      lastName: profile.lastName,
    });
    this._isAuthenticated.set(true);

    const avatar = profile.avatarUrl || localStorage.getItem('workflow.io_avatar_' + profile.userId);
    if (profile.avatarUrl) {
      localStorage.setItem('workflow.io_avatar_' + profile.userId, profile.avatarUrl);
    }
    this._userAvatar.set(avatar);

    // Fetch user organizations now that session is active
    this.loadOrganizations();
  }

  private persistTokens(auth: AuthenticationResponse): void {
    this.tokenStorage.persistTokens(auth);
  }

  private unwrapAuthResponse(
    response: ApiResponse<AuthenticationResponse>,
  ): AuthenticationResponse {
    if (!response.success || !response.data) {
      throw new Error(response.message || 'Authentication failed');
    }

    return response.data;
  }

  private unwrapData<T>(response: ApiResponse<T>): T {
    if (!response.success || response.data == null) {
      throw new Error(response.message || 'Request failed');
    }

    return response.data;
  }
}
