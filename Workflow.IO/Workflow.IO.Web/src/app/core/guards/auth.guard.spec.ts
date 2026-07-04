import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';

import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

describe('authGuard', () => {
  let auth: { isAuthenticated: jasmine.Spy<() => boolean> };
  let router: Router;

  beforeEach(() => {
    auth = {
      isAuthenticated: jasmine.createSpy('isAuthenticated'),
    };

    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        { provide: AuthService, useValue: auth },
      ],
    });

    router = TestBed.inject(Router);
  });

  it('allows navigation when authenticated', () => {
    auth.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as never, { url: '/projects' } as never),
    );

    expect(result).toBeTrue();
  });

  it('redirects to login when not authenticated', () => {
    auth.isAuthenticated.and.returnValue(false);

    const result = TestBed.runInInjectionContext(() =>
      authGuard({} as never, { url: '/projects' } as never),
    );

    expect(result).toEqual(
      router.createUrlTree(['/auth/login'], {
        queryParams: { returnUrl: '/projects' },
      }),
    );
  });
});
