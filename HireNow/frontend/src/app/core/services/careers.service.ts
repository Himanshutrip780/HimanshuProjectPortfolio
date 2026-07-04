import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CareersService {
  private apiUrl = 'http://localhost:5000/api/jobs/public';

  constructor(private http: HttpClient) {}

  public getPublicJobs(params?: { departmentId?: string; searchTerm?: string; companyId?: string }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId);
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.companyId) httpParams = httpParams.set('companyId', params.companyId);
    }
    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getPublicJob(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public getPublicBranding(companyId: string): Observable<any> {
    return this.http.get<any>(`http://localhost:5000/api/companies/public/${companyId}`);
  }

  public apply(jobId: string, formData: FormData): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${jobId}/apply`, formData);
  }
}
