import { HttpParams } from '@angular/common/http';

import { ApiResponse } from '../models/api-response.model';

export function unwrapApiResponse<T>(response: ApiResponse<T> | null): T {
  if (response == null) {
    return undefined as T;
  }

  if (!response.success || response.data == null) {
    throw new Error(response.message || 'Request failed');
  }

  return response.data;
}

export function unwrapApiResponseOptional<T>(
  response: ApiResponse<T> | null,
): T | null {
  if (response == null) {
    return null;
  }

  if (!response.success) {
    throw new Error(response.message || 'Request failed');
  }

  return response.data;
}

/** Returns empty array on 204 No Content; otherwise unwraps ApiResponse. */
export function unwrapApiResponseOrEmpty<T>(
  response: ApiResponse<T> | null | undefined,
  statusCode?: number,
): T {
  if (statusCode === 204 || response == null) {
    return [] as T;
  }

  return unwrapApiResponse(response);
}

export function toHttpParams(
  query: Record<string, string | number | boolean | null | undefined>,
): HttpParams {
  let params = new HttpParams();

  for (const [key, value] of Object.entries(query)) {
    if (value === null || value === undefined || value === '') {
      continue;
    }

    params = params.set(key, String(value));
  }

  return params;
}
