import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';

import { AuthService } from '../services/auth.service';

/** Restricts route to users whose JWT role matches one of `route.data['roles']`. */
export const roleGuard: CanActivateFn = (route) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  const allowedRoles = (route.data['roles'] as string[] | undefined) ?? [];

  const user = auth.currentUser();
  if (user && allowedRoles.some((r) => r.toLowerCase() === user.role.toLowerCase())) {
    return true;
  }

  return router.createUrlTree(['/projects']);
};
