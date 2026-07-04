import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    const user = authService.currentUser();
    if (user && user.role === 'Candidate') {
      router.navigate(['/portal']);
      return false;
    }
    return true;
  }

  router.navigate(['/auth/login']);
  return false;
};
