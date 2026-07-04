import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface JobDto {
  id: string;
  title: string;
  description: string;
  responsibilities: string;
  qualifications: string;
  departmentId: string;
  departmentName: string;
  hiringManagerId: string;
  hiringManagerName: string;
  recruiterId: string;
  recruiterName: string;
  location: string;
  type: number; // JobType Enum
  status: number; // JobStatus Enum
  salaryMin: number;
  salaryMax: number;
  currency: string;
  createdDate: string;
  applicantCount?: number;
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private apiUrl = 'http://localhost:5000/api/jobs';

  constructor(private http: HttpClient) {}

  public getJobs(params?: {
    departmentId?: string;
    status?: number;
    searchTerm?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.departmentId) httpParams = httpParams.set('departmentId', params.departmentId);
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status);
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.pageIndex) httpParams = httpParams.set('pageIndex', params.pageIndex.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getJob(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public createJob(command: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, command);
  }

  public updateJob(id: string, command: any): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}`, command);
  }

  public deleteJob(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }

  public updateJobStatus(id: string, status: number): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/status`, { status });
  }

  public getDepartments(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/departments`);
  }

  public getCurrencies(): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/currencies`);
  }

  public getUsers(role?: string): Observable<any> {
    let httpParams = new HttpParams();
    if (role) httpParams = httpParams.set('role', role);
    return this.http.get<any>(`${this.apiUrl}/users`, { params: httpParams });
  }
}
