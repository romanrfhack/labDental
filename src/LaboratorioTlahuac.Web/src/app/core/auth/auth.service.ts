import { DOCUMENT } from '@angular/common';
import { HttpClient, HttpErrorResponse, HttpHeaders } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { BehaviorSubject, catchError, map, Observable, of, switchMap, tap, throwError } from 'rxjs';

import { ApiClient } from '../http/api-client';
import { AuthUser, LoginRequest } from './auth.models';

type AuthState = AuthUser | null | undefined;

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly apiClient = inject(ApiClient);
  private readonly document = inject(DOCUMENT);
  private readonly currentUserSubject = new BehaviorSubject<AuthState>(undefined);

  readonly currentUser$ = this.currentUserSubject.asObservable();

  login(email: string, password: string): Observable<AuthUser> {
    const request: LoginRequest = { email, password };

    return this.getCsrfToken().pipe(
      switchMap((xsrfToken) =>
        this.http.post<AuthUser>(this.apiClient.getUrl('/api/auth/login'), request, {
          headers: this.createXsrfHeaders(xsrfToken),
          withCredentials: true
        })
      ),
      switchMap((user) => this.csrf().pipe(map(() => user))),
      tap((user) => this.currentUserSubject.next(user))
    );
  }

  logout(): Observable<void> {
    return this.getCsrfToken()
      .pipe(
        switchMap((xsrfToken) =>
          this.http.post<void>(this.apiClient.getUrl('/api/auth/logout'), null, {
            headers: this.createXsrfHeaders(xsrfToken),
            withCredentials: true
          })
        )
      )
      .pipe(
        tap(() => this.currentUserSubject.next(null)),
        map(() => undefined),
        catchError((error: HttpErrorResponse) => {
          this.currentUserSubject.next(null);

          if (error.status === 401) {
            return of(undefined);
          }

          return throwError(() => error);
        })
      );
  }

  csrf(): Observable<void> {
    return this.http
      .get<void>(this.apiClient.getUrl('/api/auth/csrf'), {
        withCredentials: true
      })
      .pipe(map(() => undefined));
  }

  me(): Observable<AuthUser | null> {
    return this.http
      .get<AuthUser>(this.apiClient.getUrl('/api/auth/me'), {
        withCredentials: true
      })
      .pipe(
        tap((user) => this.currentUserSubject.next(user)),
        catchError((error: HttpErrorResponse) => {
          if (error.status === 401) {
            this.currentUserSubject.next(null);
            return of(null);
          }

          return throwError(() => error);
        })
      );
  }

  ensureSession(): Observable<boolean> {
    const currentUser = this.currentUserSubject.value;

    if (currentUser !== undefined) {
      return of(currentUser !== null);
    }

    return this.me().pipe(map((user) => user !== null));
  }

  isAuthenticated(): boolean {
    return this.currentUserSubject.value !== null && this.currentUserSubject.value !== undefined;
  }

  hasPermission(permission: string): boolean {
    return this.currentUserSubject.value?.permissions.includes(permission) ?? false;
  }

  private getCsrfToken(): Observable<string> {
    return this.csrf().pipe(
      map(() => {
        const token = this.readCookie('XSRF-TOKEN');

        if (!token) {
          throw new Error('XSRF-TOKEN cookie was not issued by the API.');
        }

        return token;
      })
    );
  }

  private createXsrfHeaders(xsrfToken: string) {
    return new HttpHeaders({
      'X-XSRF-TOKEN': xsrfToken
    });
  }

  private readCookie(name: string) {
    const cookiePrefix = `${name}=`;
    const cookie = this.document.cookie
      .split(';')
      .map((item) => item.trim())
      .find((item) => item.startsWith(cookiePrefix));

    return cookie ? decodeURIComponent(cookie.substring(cookiePrefix.length)) : null;
  }
}
