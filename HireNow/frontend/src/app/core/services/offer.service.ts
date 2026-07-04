import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface OfferDto {
  id: string;
  applicationId: string;
  candidateName: string;
  jobTitle: string;
  salary: number;
  startDate: string;
  status: number; // OfferStatus Enum
  offerLetterPath: string;
  offerLetterContent: string;
  eSignatureDetails: string;
  createdDate: string;
}

@Injectable({
  providedIn: 'root'
})
export class OfferService {
  private apiUrl = 'http://localhost:5000/api/offers';

  constructor(private http: HttpClient) {}

  public getOffers(params?: {
    applicationId?: string;
    status?: number;
  }): Observable<any> {
    let httpParams = new HttpParams();
    if (params) {
      if (params.applicationId) httpParams = httpParams.set('applicationId', params.applicationId);
      if (params.status !== undefined) httpParams = httpParams.set('status', params.status);
    }

    return this.http.get<any>(this.apiUrl, { params: httpParams });
  }

  public getOffer(id: string): Observable<any> {
    return this.http.get<any>(`${this.apiUrl}/${id}`);
  }

  public createOffer(command: any): Observable<any> {
    return this.http.post<any>(this.apiUrl, command);
  }

  public updateOfferStatus(id: string, status: number, eSignatureDetails: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/status`, { status, eSignatureDetails });
  }

  public updateOfferLetter(id: string, offerLetterContent: string): Observable<any> {
    return this.http.put<any>(`${this.apiUrl}/${id}/letter`, { offerLetterContent });
  }
}
