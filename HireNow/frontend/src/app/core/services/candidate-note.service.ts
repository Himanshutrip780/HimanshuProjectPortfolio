import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface CandidateNoteDto {
  id: string;
  candidateId: string;
  applicationId?: string;
  text: string;
  authorName: string;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class CandidateNoteService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5000/api/candidateNotes';

  public getNotes(candidateId: string, applicationId?: string): Observable<any> {
    let params = new HttpParams();
    if (applicationId) {
      params = params.set('applicationId', applicationId);
    }
    return this.http.get<any>(`${this.apiUrl}/candidate/${candidateId}`, { params });
  }

  public createNote(note: { candidateId: string; applicationId?: string; text: string }): Observable<any> {
    return this.http.post<any>(this.apiUrl, note);
  }

  public deleteNote(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/${id}`);
  }
}
