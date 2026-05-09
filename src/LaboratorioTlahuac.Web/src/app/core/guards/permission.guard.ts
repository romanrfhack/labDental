import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map } from 'rxjs';

import { AuthService } from '../auth/auth.service';

export const permissionGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);
  const requiredPermission = route.data?.['permission'];

  if (typeof requiredPermission !== 'string') {
    return true;
  }

  return authService.ensureSession().pipe(
    map((isAuthenticated) => {
      if (!isAuthenticated) {
        return router.createUrlTree(['/login'], {
          queryParams: { returnUrl: state.url }
        });
      }

      return authService.hasPermission(requiredPermission)
        ? true
        : router.createUrlTree(['/app/access-denied']);
    })
  );
};
