import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterOutlet],
  template: `
    <header class="public-header">
      <a routerLink="/" class="brand">Laboratorio Dental Tlahuac</a>
      <nav>
        <a routerLink="/servicios">Servicios</a>
        <a routerLink="/contacto">Contacto</a>
        <a routerLink="/login">Login</a>
      </nav>
    </header>
    <main class="public-main">
      <router-outlet />
    </main>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent {}
