import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CandidatePortalService {
  private authUrl = 'http://localhost:5000/api/auth/candidate';
  private portalUrl = 'http://localhost:5000/api/candidatesportal';

  constructor(private http: HttpClient) {}

  public requestLoginCode(email: string): Observable<any> {
    return this.http.post<any>(`${this.authUrl}/login-request`, { email });
  }

  public verifyLoginCode(email: string, code: string): Observable<any> {
    return this.http.post<any>(`${this.authUrl}/login-verify`, { email, code }).pipe(
      tap(res => {
        if (res && res.isSuccess && res.data) {
          localStorage.setItem('token', res.data.token);
          localStorage.setItem('refreshToken', res.data.refreshToken);
          localStorage.setItem('user', JSON.stringify({
            email: res.data.email,
            firstName: res.data.firstName,
            lastName: res.data.lastName,
            role: res.data.role,
            companyId: res.data.companyId
          }));
        }
      })
    );
  }

  private getAuthHeaders(): { headers: HttpHeaders } {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  public getApplications(): Observable<any> {
    return this.http.get<any>(`${this.portalUrl}/applications`, this.getAuthHeaders());
  }

  public getInterviews(): Observable<any> {
    return this.http.get<any>(`${this.portalUrl}/interviews`, this.getAuthHeaders());
  }

  public getOffers(): Observable<any> {
    return this.http.get<any>(`${this.portalUrl}/offers`, this.getAuthHeaders());
  }

  public acceptOffer(offerId: string, signatureName: string): Observable<any> {
    return this.http.post<any>(`${this.portalUrl}/offers/${offerId}/accept`, { signatureName }, this.getAuthHeaders());
  }

  public getDocuments(): Observable<any> {
    return this.http.get<any>(`${this.portalUrl}/documents`, this.getAuthHeaders());
  }
}
