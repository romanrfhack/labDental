import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-access-denied-page',
  imports: [RouterLink],
  template: `
    <section>
      <h1>Acceso denegado</h1>
      <p>No tienes permiso para ver esta seccion.</p>
      <a [routerLink]="authService.getDefaultPrivateRoute()">Ir a mi inicio</a>
    </section>
  `
})
export class AccessDeniedPageComponent {
  constructor(readonly authService: AuthService) {}
}
