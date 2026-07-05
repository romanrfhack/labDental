import { Component } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { AuthUser } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';

export function getSafePrivateReturnUrl(returnUrl: string | null) {
  if (!returnUrl) {
    return null;
  }

  const value = returnUrl.trim();

  if (
    value !== returnUrl ||
    value.includes('\\') ||
    /^[a-z][a-z0-9+.-]*:/i.test(value) ||
    value.startsWith('//')
  ) {
    return null;
  }

  const isPrivateRoute =
    value === '/app' ||
    value.startsWith('/app/') ||
    value.startsWith('/app?') ||
    value.startsWith('/app#');

  return isPrivateRoute ? value : null;
}

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, RouterLink],
  template: `
    <main class="login-page">
      <section class="login-panel">
        <div class="login-logo-wrap">
          <img
            class="login-logo"
            src="/assets/brand/logo-ldt.webp"
            alt="Laboratorio Dental Tláhuac"
            width="809"
            height="545"
          />
        </div>
        <div class="login-copy">
          <p class="login-brand">Precisión • Estética • Confianza</p>
          <h1>Acceso privado</h1>
          <p>Ingresa para administrar clientes, órdenes, pagos y operación interna del laboratorio.</p>
        </div>
        <form [formGroup]="form" (ngSubmit)="submit()">
          <label>
            Email
            <input type="email" formControlName="email" autocomplete="username" placeholder="tu@correo.com" />
          </label>
          <label>
            Contrasena
            <input type="password" formControlName="password" autocomplete="current-password" placeholder="••••••••" />
          </label>
          <div class="login-actions">
            @if (errorMessage) {
              <p class="error" role="alert">{{ errorMessage }}</p>
            }
            <button type="submit" [disabled]="form.invalid || isSubmitting">
              {{ isSubmitting ? 'Entrando...' : 'Entrar' }}
            </button>
          </div>
        </form>
        <a class="login-footer-link" routerLink="/">Volver al sitio publico</a>
      </section>
    </main>
  `,
  styleUrl: './login-page.component.scss'
})
export class LoginPageComponent {
  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email]
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required]
    })
  });

  errorMessage = '';
  isSubmitting = false;

  constructor(
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  submit() {
    if (this.form.invalid || this.isSubmitting) {
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;

    this.authService
      .login(this.form.controls.email.value, this.form.controls.password.value)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (user) => this.router.navigateByUrl(this.getReturnUrl(user)),
        error: (error: HttpErrorResponse) => {
          this.errorMessage =
            error.status === 423
              ? 'Usuario inactivo o bloqueado.'
              : 'Email o contrasena invalidos.';
        }
      });
  }

  private getReturnUrl(user: AuthUser) {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');

    return getSafePrivateReturnUrl(returnUrl) ?? this.authService.getDefaultPrivateRoute(user);
  }
}
