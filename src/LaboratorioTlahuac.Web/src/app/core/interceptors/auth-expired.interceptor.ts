import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

let isRedirectingToLogin = false;

export const authExpiredInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === 401) {
        redirectToLogin(router);
      }

      return throwError(() => error);
    })
  );
};

function redirectToLogin(router: Router): void {
  if (isRedirectingToLogin || isLoginRoute(router.url)) {
    return;
  }

  isRedirectingToLogin = true;

  void router.navigate(['/login'], { replaceUrl: true }).then(
    () => {
      isRedirectingToLogin = false;
    },
    () => {
      isRedirectingToLogin = false;
    }
  );
}

function isLoginRoute(url: string): boolean {
  return url === '/login' || url.startsWith('/login?') || url.startsWith('/login#');
}
