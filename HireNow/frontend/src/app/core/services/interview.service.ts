import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface InterviewFeedbackDto {
  id: string;
  interviewerId: string;
  interviewerName: string;
  communicationScore: number;
  problemSolvingScore: number;
  codingScore: number;
  systemDesignScore: number;
  cultureFitScore: number;
  feedbackText: string;
  recommendation: number; // Recommendation Enum
  submittedDate: string;
}

export interface InterviewDto {
  id: string;
  applicationId: string;
  candidateName: string;
  jobTitle: string;
  interviewerId: string;
  interviewerName: string;
  title: string;
  scheduledTime: string;
  durationMinutes: number;
  meetingLink: string;
  notes: string;
  status: number; // InterviewStatus Enum
  feedbacks: InterviewFeedbackDto[];
}

@Injectable({
  providedIn: 'root'
})
export class InterviewService {
  private apiUrl = 'http://localhost:5000/api/interviews';

  constructor(private http: HttpClient) {}

  public getInterviews(params?: {
    applicationId?: string;
    interviewerId?: string;
    status?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.applicationId) httpParams = httpParams.set('applicationId', params.applicationId);
      if (params.interviewerId) httpParams = httpParams.set('interviewerId', params.interviewerId);
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status);
    }

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getInterview(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public scheduleInterview(command: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, command);
  }

  public submitFeedback(interviewId: string, command: any): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${interviewId}/feedback`, { ...command, interviewId });
  }

  public cancelInterview(id: string): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/cancel`, {});
  }

  public rescheduleInterview(id: string, command: { scheduledTime: string; durationMinutes: number }): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/${id}/reschedule`, command);
  }
}
