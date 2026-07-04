import { Injectable, inject } from '@angular/core';
import { tap } from 'rxjs';

import {
  ApiResponse,
  AuthenticationResponse,
  LoginRequest,
  RegisterUserRequest,
} from '../models/api.models';
import { ApiClientService } from './api-client.service';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly api = inject(ApiClientService);
  private readonly tokenStorage = inject(TokenStorageService);

  register(request: RegisterUserRequest) {
    return this.api.post<unknown>('/users/register', request);
  }

  login(request: LoginRequest) {
    return this.api
      .post<AuthenticationResponse>('/users/authenticate', request)
      .pipe(
        tap((response: ApiResponse<AuthenticationResponse>) => {
          this.tokenStorage.saveTokens(
            response.data.jwtToken,
            response.data.refreshToken,
          );
        }),
      );
  }

  logout(): void {
    this.tokenStorage.clear();
  }
}
