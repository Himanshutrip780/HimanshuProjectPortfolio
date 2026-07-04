import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CandidateDto {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  phone: string;
  skills: string[];
  education?: string;
  createdDate: string;
  parsedDataJson?: string;
  parsingConfidenceScore?: number;
  currentTitle?: string;
  yearsOfExperience?: string;
  latestApplicationJobTitle?: string;
  latestApplicationStage?: string;
  latestApplicationMatchScore?: number;
  linkedInUrl?: string;
  gitHubUrl?: string;
  portfolioUrl?: string;
  resumePath?: string;
  source?: string;
}

@Injectable({
  providedIn: 'root'
})
export class CandidateService {
  private apiUrl = 'http://localhost:5000/api/candidates';

  constructor(private http: HttpClient) {}

  public getCandidates(params?: {
    searchTerm?: string;
    pageIndex?: number;
    pageSize?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
      if (params.pageIndex) httpParams = httpParams.set('pageIndex', params.pageIndex.toString());
      if (params.pageSize) httpParams = httpParams.set('pageSize', params.pageSize.toString());
    }

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getCandidate(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public createCandidate(command: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, command);
  }

  public uploadResume(file: File, jobId?: string): Observable<any> {
    const formData = new FormData();
    formData.append('file', file);
    if (jobId) {
      formData.append('jobId', jobId);
    }
    return this.http.post<any>(`${this.apiUrl}/upload-resume`, formData);
  }

  public downloadResume(id: string): Observable<Blob> {
    return this.http.get(`${this.apiUrl}/${id}/resume`, { responseType: 'blob' });
  }

  public getDuplicates(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/duplicates`);
  }

  public assignTalentPool(id: string, poolName: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/talent-pool`, { poolName });
  }
}
