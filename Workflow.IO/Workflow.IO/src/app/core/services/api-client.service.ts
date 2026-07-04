import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api.models';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class ApiClientService {
  private readonly http = inject(HttpClient);
  private readonly tokenStorage = inject(TokenStorageService);

  private correlationId = crypto.randomUUID();

  get<T>(path: string, params?: Record<string, string>): Observable<ApiResponse<T>> {
    return this.http.get<ApiResponse<T>>(
      `${environment.apiGatewayUrl}${path}`,
      {
        headers: this.buildHeaders(),
        params: new HttpParams({ fromObject: params ?? {} }),
      },
    );
  }

  post<T>(path: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.post<ApiResponse<T>>(
      `${environment.apiGatewayUrl}${path}`,
      body,
      { headers: this.buildHeaders() },
    );
  }

  put<T>(path: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.put<ApiResponse<T>>(
      `${environment.apiGatewayUrl}${path}`,
      body,
      { headers: this.buildHeaders() },
    );
  }

  patch<T>(path: string, body: unknown): Observable<ApiResponse<T>> {
    return this.http.patch<ApiResponse<T>>(
      `${environment.apiGatewayUrl}${path}`,
      body,
      { headers: this.buildHeaders() },
    );
  }

  delete<T>(path: string): Observable<ApiResponse<T>> {
    return this.http.delete<ApiResponse<T>>(
      `${environment.apiGatewayUrl}${path}`,
      { headers: this.buildHeaders() },
    );
  }

  private buildHeaders(): HttpHeaders {
    let headers = new HttpHeaders({
      'X-Correlation-Id': this.correlationId,
    });

    const token = this.tokenStorage.getAccessToken();
    if (token) {
      headers = headers.set('Authorization', `Bearer ${token}`);
    }

    return headers;
  }
}
