import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CompanyService {
  private apiUrl = 'http://localhost:5000/api/companies/current';

  constructor(private http: HttpClient) {}

  private getAuthHeaders(): { headers: HttpHeaders } {
    const token = localStorage.getItem('token');
    return {
      headers: new HttpHeaders({
        'Authorization': `Bearer ${token}`
      })
    };
  }

  public getCompany(): Observable<any> {
    return this.http.get<any>(this.apiUrl, this.getAuthHeaders());
  }

  public updateCompany(companyData: {
    name: string;
    domain: string;
    logoUrl?: string | null;
    primaryColor?: string | null;
    fontFamily?: string | null;
    customCss?: string | null;
    ssoEnabled?: boolean;
    ssoProvider?: string | null;
    ssoIssuer?: string | null;
    ssoClientId?: string | null;
    ssoRedirectUrl?: string | null;
  }): Observable<any> {
    return this.http.put<any>(this.apiUrl, companyData, this.getAuthHeaders());
  }

  public getAuditLogs(pageIndex: number = 1, pageSize: number = 10): Observable<any> {
    const url = 'http://localhost:5000/api/auditlogs';
    return this.http.get<any>(`${url}?pageIndex=${pageIndex}&pageSize=${pageSize}`, this.getAuthHeaders());
  }
}
