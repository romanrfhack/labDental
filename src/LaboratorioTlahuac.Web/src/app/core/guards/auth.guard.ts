import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const loginRedirect = router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });

  return authService.ensureSession().pipe(
    map((isAuthenticated) =>
      isAuthenticated
        ? true
        : loginRedirect
    ),
    catchError(() => of(loginRedirect))
  );
};
