import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ApplicationDto {
  id: string;
  jobId: string;
  jobTitle: string;
  candidateId: string;
  candidateName: string;
  candidateEmail: string;
  stage: string;
  status: string;
  appliedDate: string;
  aimatchScore?: number;
  airecommendation?: string;
  aisummary?: string;
  skillsMatch?: string[];
  missingSkills?: string[];
  educationFit?: string;
  experienceFit?: string;
  strengths?: string[];
  weaknesses?: string[];
  aiQuestions?: string[];
  candidate?: any;
  timeInStageDays?: number;
}

export interface ActivityLogDto {
  id: string;
  applicationId: string;
  activityType: string;
  description: string;
  performedBy: string;
  timestamp: string;
}

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {
  private apiUrl = 'http://localhost:5000/api/applications';

  constructor(private http: HttpClient) {}

  public getApplications(params?: {
    jobId?: string;
    candidateId?: string;
    stage?: string;
    status?: string;
    searchTerm?: string;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.jobId) httpParams = httpParams.set('jobId', params.jobId);
      if (params.candidateId) httpParams = httpParams.set('candidateId', params.candidateId);
      if (params.stage) httpParams = httpParams.set('stage', params.stage);
      if (params.status) httpParams = httpParams.set('status', params.status);
      if (params.searchTerm) httpParams = httpParams.set('searchTerm', params.searchTerm);
    }

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getApplication(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public createApplication(command: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, command);
  }

  public updateStage(id: string, newStage: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/stage`, { newStage });
  }

  public getTimeline(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}/timeline`);
  }
}
