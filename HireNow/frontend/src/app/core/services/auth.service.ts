import { Injectable, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap, catchError, throwError, of } from 'rxjs';

export interface AuthResponse {
  token: string;
  refreshToken: string;
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId: string;
  id: string;
}

export interface UserInfo {
  email: string;
  firstName: string;
  lastName: string;
  role: string;
  companyId: string;
  id: string;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = 'http://localhost:5000/api/auth';
  
  // Signals for state management
  private userState = signal<UserInfo | null>(null);
  
  public currentUser = computed(() => this.userState());
  public isAuthenticated = computed(() => this.userState() !== null);
  public userRole = computed(() => this.userState()?.role ?? '');

  constructor(
    private http: HttpClient,
    private router: Router
  ) {
    this.loadUserFromStorage();
  }

  private loadUserFromStorage() {
    const token = localStorage.getItem('token');
    const userStr = localStorage.getItem('user');
    if (token && userStr) {
      try {
        const user = JSON.parse(userStr) as UserInfo;
        this.userState.set(user);
      } catch {
        this.logout();
      }
    }
  }

  public getCompanies(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/companies`);
  }

  public register(request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/register`, request).pipe(
      tap(res => {
        if (res && res.isSuccess && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  public sendOtp(request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/send-otp`, request);
  }

  public verifyOtp(email: string, code: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/verify-otp`, { email, code }).pipe(
      tap(res => {
        if (res && res.isSuccess && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  public resendOtp(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/resend-otp`, { email });
  }

  public checkStatus(email: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/check-status`, { params: { email } });
  }

  public login(request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/login`, request).pipe(
      tap(res => {
        if (res && res.isSuccess && res.data) {
          this.setSession(res.data);
        }
      })
    );
  }

  public refreshToken(): Observable<any> {
    const token = localStorage.getItem('token');
    const refreshToken = localStorage.getItem('refreshToken');
    if (!token || !refreshToken) {
      this.logout();
      return throwError(() => new Error('No tokens found'));
    }

    return this.http.post<any>(`${this.apiUrl}/refresh-token`, { token, refreshToken }).pipe(
      tap(res => {
        if (res && res.isSuccess && res.data) {
          this.setSession(res.data);
        } else {
          this.logout();
        }
      }),
      catchError(err => {
        this.logout();
        return throwError(() => err);
      })
    );
  }

  public forgotPassword(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/forgot-password`, { email });
  }

  public resetPassword(request: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/reset-password`, request);
  }

  public logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
    this.userState.set(null);
    void this.router.navigate(['/auth/login']);
  }

  private setSession(authResult: AuthResponse) {
    localStorage.setItem('token', authResult.token);
    localStorage.setItem('refreshToken', authResult.refreshToken);
    
    const userInfo: UserInfo = {
      email: authResult.email,
      firstName: authResult.firstName,
      lastName: authResult.lastName,
      role: authResult.role,
      companyId: authResult.companyId,
      id: authResult.id
    };
    
    localStorage.setItem('user', JSON.stringify(userInfo));
    this.userState.set(userInfo);
  }

  public resolveSso(email: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/sso/resolve`, { email });
  }

  public setSessionFromCallback(token: string, refreshToken: string) {
    try {
      const payloadBase64 = token.split('.')[1];
      const payloadJson = atob(payloadBase64);
      const payload = JSON.parse(payloadJson);
      
      const userInfo: UserInfo = {
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || payload['email'] || '',
        firstName: payload['FirstName'] || '',
        lastName: payload['LastName'] || '',
        role: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || payload['role'] || '',
        companyId: payload['CompanyId'] || '',
        id: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || payload['sub'] || ''
      };
      
      localStorage.setItem('token', token);
      localStorage.setItem('refreshToken', refreshToken);
      localStorage.setItem('user', JSON.stringify(userInfo));
      this.userState.set(userInfo);
    } catch (e) {
      console.error('Failed to parse token callback', e);
    }
  }

  public getToken(): string | null {
    return localStorage.getItem('token');
  }
}
