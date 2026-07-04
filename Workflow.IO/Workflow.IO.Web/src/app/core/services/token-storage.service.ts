import { Injectable } from '@angular/core';

import { AUTH_STORAGE_KEYS } from '../constants/auth-storage.constants';
import { AuthenticationResponse } from '../models/user.models';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getAccessToken(): string | null {
    return localStorage.getItem(AUTH_STORAGE_KEYS.accessToken);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(AUTH_STORAGE_KEYS.refreshToken);
  }

  isAccessTokenExpired(): boolean {
    const expiresAt = localStorage.getItem(AUTH_STORAGE_KEYS.expiresAt);
    if (!expiresAt) {
      return true;
    }

    return Date.now() >= Number(expiresAt);
  }

  persistTokens(auth: AuthenticationResponse): void {
    if (auth.jwtToken) {
      localStorage.setItem(AUTH_STORAGE_KEYS.accessToken, auth.jwtToken);
    }

    if (auth.refreshToken) {
      localStorage.setItem(AUTH_STORAGE_KEYS.refreshToken, auth.refreshToken);
    }

    if (auth.expiresIn > 0) {
      const expiresAt = Date.now() + auth.expiresIn * 1000;
      localStorage.setItem(
        AUTH_STORAGE_KEYS.expiresAt,
        String(expiresAt),
      );
    }
  }

  getActiveOrganizationId(): string | null {
    return localStorage.getItem('workflow.io_active_org_id');
  }

  setActiveOrganizationId(orgId: string | null): void {
    if (orgId) {
      localStorage.setItem('workflow.io_active_org_id', orgId);
    } else {
      localStorage.removeItem('workflow.io_active_org_id');
    }
  }

  clear(): void {
    localStorage.removeItem(AUTH_STORAGE_KEYS.accessToken);
    localStorage.removeItem(AUTH_STORAGE_KEYS.refreshToken);
    localStorage.removeItem(AUTH_STORAGE_KEYS.expiresAt);
    localStorage.removeItem('workflow.io_active_org_id');
  }
}
