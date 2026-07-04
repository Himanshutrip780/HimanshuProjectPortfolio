import { AuthSessionUser } from '../models/user.models';

interface JwtPayload {
  sub?: string;
  email?: string;
  role?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
}

export function decodeJwtPayload(token: string): JwtPayload | null {
  try {
    const [, payloadSegment] = token.split('.');
    if (!payloadSegment) {
      return null;
    }

    const normalized = payloadSegment.replace(/-/g, '+').replace(/_/g, '/');
    const json = atob(normalized);
    return JSON.parse(json) as JwtPayload;
  } catch {
    return null;
  }
}

export function toAuthSessionUser(token: string): AuthSessionUser | null {
  const payload = decodeJwtPayload(token) as any;
  if (!payload) {
    return null;
  }

  const userId =
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] ??
    payload.sub ??
    payload.nameid ??
    payload.id;

  const email =
    payload.email ??
    payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
    '';

  const role =
    payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ??
    payload.role ??
    'User';

  if (!userId) {
    return null;
  }

  return { userId, email, role };
}

