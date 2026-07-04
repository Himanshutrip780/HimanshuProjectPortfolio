import { Injectable, computed, signal } from '@angular/core';

import {
  extractFieldErrors,
  getApiErrorMessage,
} from '../utils/api-error.util';

@Injectable({ providedIn: 'root' })
export class ApiErrorService {
  private readonly _lastError = signal<string | null>(null);
  private readonly _fieldErrors = signal<Record<string, string[]>>({});

  readonly lastError = this._lastError.asReadonly();
  readonly fieldErrors = this._fieldErrors.asReadonly();
  readonly hasFieldErrors = computed(
    () => Object.keys(this._fieldErrors()).length > 0,
  );

  capture(error: unknown, fallbackMessage = 'Request failed'): void {
    const fieldErrors = extractFieldErrors(error);
    if (Object.keys(fieldErrors).length > 0) {
      this._fieldErrors.set(fieldErrors);
      this._lastError.set(getApiErrorMessage(error, fallbackMessage));
      return;
    }

    this._fieldErrors.set({});
    this._lastError.set(getApiErrorMessage(error, fallbackMessage));
  }

  clear(): void {
    this._lastError.set(null);
    this._fieldErrors.set({});
  }
}
