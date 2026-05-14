import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { catchError, map, of } from 'rxjs';

import { AuthService } from '../auth/auth.service';

export const permissionGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const requiredPermission = route.data?.['permission'];
  const loginRedirect = router.createUrlTree(['/login'], {
    queryParams: { returnUrl: state.url }
  });

  if (typeof requiredPermission !== 'string') {
    return true;
  }

  return authService.ensureSession().pipe(
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return loginRedirect;
      }

      return authService.hasPermission(requiredPermission)
        ? true
        : router.createUrlTree(['/app/access-denied']);
    }),
    catchError(() => of(loginRedirect))
  );
};
