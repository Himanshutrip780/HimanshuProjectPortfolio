import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const subdomainGuard = async () => {
  const router = inject(Router);
  const authService = inject(AuthService);

  const hostname = window.location.hostname;
  const parts = hostname.split('.');

  // Basic check: if there is a subdomain (e.g. acme.workflow.io or acme.localhost)
  // We assume parts.length > 2 for standard domains, or parts.length > 1 for localhost
  let subdomain = '';
  if (hostname.endsWith('.azurecontainerapps.io') || (hostname.endsWith('.onrender.com') && parts.length <= 3)) {
    subdomain = '';
  } else if (hostname.includes('localhost') && parts.length > 1) {
    subdomain = parts[0];
  } else if (parts.length > 2) {
    subdomain = parts[0];
  }

  const reserved = ['www', 'app', 'api', 'admin', 'frontend', 'web', 'gateway', 'gatewayapi'];

  if (subdomain && !reserved.includes(subdomain)) {
    // Attempt to resolve the organization by subdomain
    try {
      const resolvedOrg = await authService.resolveOrganizationBySubdomain(subdomain);
      if (resolvedOrg) {
        authService.setActiveOrganization(resolvedOrg.id);
        return true;
      } else {
        // Redirect to a 'workspace-not-found' page
        return router.createUrlTree(['/workspace-not-found']);
      }
    } catch (err) {
      return router.createUrlTree(['/workspace-not-found']);
    }
  }

  return true;
};
