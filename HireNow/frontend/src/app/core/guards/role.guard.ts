import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const expectedRoles = route.data['expectedRoles'] as Array<string>;
  const currentUser = authService.currentUser();

  if (authService.isAuthenticated() && currentUser) {
    if (!expectedRoles || expectedRoles.length === 0 || expectedRoles.includes(currentUser.role)) {
      return true;
    }
  }

  // Redirect to dashboard if unauthorized
  router.navigate(['/dashboard']);
  return false;
};
