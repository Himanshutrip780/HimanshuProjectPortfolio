/** Mirrors JwtAuthenticationManager.Model.AuthenticationRequest */
export interface AuthenticationRequest {
  email: string;
  password: string;
}

/** Mirrors JwtAuthenticationManager.Model.AuthenticationResponse */
export interface AuthenticationResponse {
  email: string | null;
  jwtToken: string | null;
  refreshToken: string | null;
  expiresIn: number;
  role: string | null;
}

/** Mirrors JwtAuthenticationManager.Model.RefreshTokenRequest */
export interface RefreshTokenRequest {
  refreshToken: string;
}

/** Mirrors UserApi.Model.Dto.RegisterUserRequestDTO */
export interface RegisterUserRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
  organizationName?: string;
  inviteCode?: string;
}

/** Mirrors UserApi.Model.Dto.ChangePasswordRequestDto */
export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

/** Mirrors UserApi.Model.Domian.Common.UserStatus (serialized as int). */
export enum UserStatus {
  Active = 0,
  Inactive = 1,
  Suspended = 2,
}

/** Mirrors UserApi.Model.Dto.UserDto */
export interface UserDto {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl: string | null;
  role: string;
  status: UserStatus;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
}

/** JWT claims surfaced for the authenticated session. */
export interface AuthSessionUser {
  userId: string;
  email: string;
  role: string;
  firstName?: string;
  lastName?: string;
}

/** Mirrors UserApi.Model.Dto.UserLookupDto */
export interface UserLookup {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
}

/** Mirrors UserApi.Model.Dto.UpdateProfileRequestDto */
export interface UpdateProfileRequest {
  firstName: string;
  lastName: string;
  avatarUrl?: string | null;
}

/** Mirrors UserApi.Model.Dto.UserProfileDto (GET/PUT /users/me) */
export interface UserProfile {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  avatarUrl: string | null;
  role: string;
  status: UserStatus;
  isDeleted: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface OrganizationDto {
  organizationId: string;
  name: string;
  subdomain: string | null;
  subscriptionTier: string;
  inviteCode: string | null;
  createdAt: string;
}
