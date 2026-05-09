import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-access-denied-page',
  imports: [RouterLink],
  template: `
    <section>
      <h1>Acceso denegado</h1>
      <p>No tienes permiso para ver esta seccion.</p>
      <a routerLink="/app/dashboard">Ir al dashboard</a>
    </section>
  `
})
export class AccessDeniedPageComponent {}
