import { HttpClient, HttpResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, throwError } from 'rxjs';

import { environment } from '../../../environments/environment';
import { ApiResponse } from '../models/api-response.model';
import { ApiErrorService } from './api-error.service';
import { apiUrl } from '../utils/api-url.util';
import {
  toHttpParams,
  unwrapApiResponse,
  unwrapApiResponseOptional,
  unwrapApiResponseOrEmpty,
} from '../utils/api.util';

@Injectable({ providedIn: 'root' })
export class ApiHttpService {
  private readonly http = inject(HttpClient);
  private readonly apiErrors = inject(ApiErrorService);

  get<T>(
    path: string,
    query?: Record<string, string | number | boolean | null | undefined>,
  ): Observable<T> {
    const params = query ? toHttpParams(query) : undefined;
    return this.http
      .get<ApiResponse<T>>(apiUrl(environment.apiGatewayUrl, path), {
        params,
        observe: 'response',
      })
      .pipe(
        map((response) => this.unwrapGetResponse(response)),
        catchError((error) => this.handleError(error)),
      );
  }

  /** List GET that returns an empty array on 204 No Content. */
  getList<T>(
    path: string,
    query?: Record<string, string | number | boolean | null | undefined>,
  ): Observable<T[]> {
    return this.get<T[]>(path, query).pipe(
      map((data) => (Array.isArray(data) ? data : [])),
    );
  }

  getOptional<T>(
    path: string,
    query?: Record<string, string | number | boolean | null | undefined>,
    fallback: T | null = null,
  ): Observable<T> {
    const params = query ? toHttpParams(query) : undefined;
    return this.http
      .get<ApiResponse<T> | null>(apiUrl(environment.apiGatewayUrl, path), {
        params,
      })
      .pipe(
        map((response): T => (unwrapApiResponseOptional(response) ?? fallback) as T),
        catchError((error) => this.handleError(error)),
      );
  }

  post<T>(path: string, body?: unknown): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(apiUrl(environment.apiGatewayUrl, path), body ?? null)
      .pipe(
        map(unwrapApiResponse),
        catchError((error) => this.handleError(error)),
      );
  }

  /** POST that only requires success=true (data may be null). */
  postCommand(path: string, body?: unknown): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(apiUrl(environment.apiGatewayUrl, path), body ?? null)
      .pipe(
        map((response) => {
          if (!response?.success) {
            throw new Error(response?.message || 'Request failed');
          }
        }),
        catchError((error) => this.handleError(error)),
      );
  }

  postOptional<T>(path: string, body?: unknown): Observable<T | null> {
    return this.http
      .post<ApiResponse<T> | null>(
        apiUrl(environment.apiGatewayUrl, path),
        body ?? null,
      )
      .pipe(
        map(unwrapApiResponseOptional),
        catchError((error) => this.handleError(error)),
      );
  }

  put<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .put<ApiResponse<T>>(apiUrl(environment.apiGatewayUrl, path), body)
      .pipe(
        map(unwrapApiResponse),
        catchError((error) => this.handleError(error)),
      );
  }

  patch<T>(path: string, body: unknown): Observable<T> {
    return this.http
      .patch<ApiResponse<T>>(apiUrl(environment.apiGatewayUrl, path), body)
      .pipe(
        map(unwrapApiResponse),
        catchError((error) => this.handleError(error)),
      );
  }

  /** PATCH that only requires success=true (data may be null). */
  patchCommand(path: string, body: unknown): Observable<void> {
    return this.http
      .patch<ApiResponse<unknown>>(apiUrl(environment.apiGatewayUrl, path), body)
      .pipe(
        map((response) => {
          if (!response?.success) {
            throw new Error(response?.message || 'Request failed');
          }
        }),
        catchError((error) => this.handleError(error)),
      );
  }

  patchOptional<T>(path: string, body: unknown): Observable<T | null> {
    return this.http
      .patch<ApiResponse<T> | null>(apiUrl(environment.apiGatewayUrl, path), body)
      .pipe(
        map(unwrapApiResponseOptional),
        catchError((error) => this.handleError(error)),
      );
  }

  delete(path: string): Observable<void> {
    return this.http
      .delete<void>(apiUrl(environment.apiGatewayUrl, path))
      .pipe(catchError((error) => this.handleError(error)));
  }

  /** Binary GET with centralized error handling. */
  getBlob(path: string): Observable<Blob> {
    return this.http
      .get(apiUrl(environment.apiGatewayUrl, path), {
        responseType: 'blob',
        observe: 'response',
      })
      .pipe(
        map((response) => {
          const blob = response.body;
          if (!blob) {
            throw new Error('Empty file response');
          }

          if (blob.type === 'application/json') {
            throw new Error('Download failed');
          }

          return blob;
        }),
        catchError((error) => this.handleError(error)),
      );
  }

  /** Multipart upload (attachments). */
  postForm<T>(path: string, formData: FormData): Observable<T> {
    return this.http
      .post<ApiResponse<T>>(apiUrl(environment.apiGatewayUrl, path), formData)
      .pipe(
        map(unwrapApiResponse),
        catchError((error) => this.handleError(error)),
      );
  }

  private unwrapGetResponse<T>(response: HttpResponse<ApiResponse<T>>): T {
    if (response.status === 204) {
      return unwrapApiResponseOrEmpty<T>(response.body, 204);
    }

    if (!response.body) {
      return unwrapApiResponseOrEmpty<T>(null, 204);
    }

    return unwrapApiResponse(response.body);
  }

  private handleError(error: unknown): Observable<never> {
    this.apiErrors.capture(error);
    return throwError(() => error);
  }
}
