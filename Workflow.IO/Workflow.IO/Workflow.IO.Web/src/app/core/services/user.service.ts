import { Injectable, inject, signal } from '@angular/core';
import { Observable } from 'rxjs';

import {
  ChangePasswordRequest,
  RegisterUserRequest,
  UpdateProfileRequest,
  UserDto,
  UserLookup,
  UserProfile,
} from '../models/user.models';
import { ApiHttpService } from './api-http.service';

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly api = inject(ApiHttpService);

  readonly usersMap = signal<Record<string, UserDto>>({});
  private readonly loadingIds = new Set<string>();

  getMe(): Observable<UserProfile> {
    return this.api.get<UserProfile>('/users/me');
  }

  updateMe(request: UpdateProfileRequest): Observable<UserProfile> {
    return this.api.put<UserProfile>('/users/me', request);
  }

  changePassword(request: ChangePasswordRequest): Observable<void> {
    return this.api.postCommand('/users/me/change-password', request);
  }

  lookupUsers(email: string): Observable<UserLookup[]> {
    return this.api.getList<UserLookup>('/users/lookup', { email });
  }

  getAllUsers(): Observable<UserDto[]> {
    return this.api.getList<UserDto>('/users');
  }

  getUserById(userId: string): Observable<UserDto> {
    return this.api.get<UserDto>(`/users/${userId}`);
  }

  updateUser(
    userId: string,
    request: RegisterUserRequest,
  ): Observable<UserDto> {
    return this.api.put<UserDto>(`/users/${userId}`, request);
  }

  deleteUser(userId: string): Observable<void> {
    return this.api.delete(`/users/${userId}`);
  }

  loadAllUsers(): void {
    this.getAllUsers().subscribe({
      next: (users) => {
        const map: Record<string, UserDto> = {};
        for (const u of users) {
          map[u.userId.toLowerCase()] = u;
        }
        this.usersMap.update((existing) => ({ ...existing, ...map }));
      }
    });
  }

  getUserDisplayName(userId: string | null): string {
    if (!userId) return 'Unassigned';
    const lowerId = userId.toLowerCase();
    const user = this.usersMap()[lowerId];
    if (!user) {
      this.lazyLoadUser(lowerId);
      return `User ${userId.slice(0, 8)}`;
    }
    return `${user.firstName} ${user.lastName}`;
  }

  getUserInitials(userId: string | null): string {
    if (!userId) return '?';
    const lowerId = userId.toLowerCase();
    const user = this.usersMap()[lowerId];
    if (!user) {
      this.lazyLoadUser(lowerId);
      return userId.slice(0, 2).toUpperCase();
    }
    const first = user.firstName ? user.firstName.charAt(0).toUpperCase() : '';
    const last = user.lastName ? user.lastName.charAt(0).toUpperCase() : '';
    return first + last || '?';
  }

  getUserAvatarUrl(userId: string | null): string | null {
    if (!userId) return null;
    const lowerId = userId.toLowerCase();
    const user = this.usersMap()[lowerId];
    if (!user) {
      this.lazyLoadUser(lowerId);
      return null;
    }
    return user.avatarUrl || null;
  }

  private lazyLoadUser(userId: string): void {
    if (this.loadingIds.has(userId)) return;
    this.loadingIds.add(userId);

    this.getUserById(userId).subscribe({
      next: (user) => {
        this.usersMap.update((map) => ({
          ...map,
          [userId]: user,
        }));
        this.loadingIds.delete(userId);
      },
      error: () => {
        // Cache placeholder so we don't try loading again
        this.usersMap.update((map) => ({
          ...map,
          [userId]: {
            userId: userId,
            firstName: 'User',
            lastName: userId.slice(0, 8),
            email: '',
            role: '',
            avatarUrl: null
          } as UserDto,
        }));
        this.loadingIds.delete(userId);
      }
    });
  }
}
