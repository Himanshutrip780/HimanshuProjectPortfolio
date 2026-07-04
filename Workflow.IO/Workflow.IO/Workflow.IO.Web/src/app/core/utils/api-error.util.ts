import { HttpErrorResponse } from '@angular/common/http';

import { ErrorResponse } from '../models/api-response.model';

const STATUS_MESSAGES: Record<number, string> = {
  401: 'You are not signed in or your session has expired.',
  403: 'You do not have permission to perform this action.',
  404: 'The requested resource was not found.',
  429: 'Too many requests. Please try again later.',
};

export function getApiErrorMessage(
  error: unknown,
  fallbackMessage = 'Request failed',
): string {
  if (error instanceof HttpErrorResponse) {
    if (error.status === 401) {
      if (error.url && (error.url.includes('/users/authenticate') || error.url.includes('/authenticate'))) {
        return 'Invalid email or password. Please try again.';
      }
    }
    const statusMessage = STATUS_MESSAGES[error.status];
    if (statusMessage) {
      return statusMessage;
    }

    if (typeof error.error === 'object' && error.error !== null) {
      const body = error.error as ErrorResponse & {
        title?: string;
        detail?: string;
      };

      if (body.message) {
        return body.message;
      }

      if (body.title) {
        return body.title;
      }

      if (body.errors) {
        const first = Object.values(body.errors).flat()[0];
        if (first) {
          return first;
        }
      }
    }

    if (typeof error.error === 'string' && error.error.trim().length > 0) {
      return error.error;
    }

    return error.message || fallbackMessage;
  }

  if (error instanceof Error && error.message) {
    return error.message;
  }

  return fallbackMessage;
}

export function extractFieldErrors(error: unknown): Record<string, string[]> {
  if (!(error instanceof HttpErrorResponse)) {
    return {};
  }

  if (error.status !== 400 || typeof error.error !== 'object' || error.error === null) {
    return {};
  }

  const body = error.error as ErrorResponse;
  return body.errors ?? {};
}
