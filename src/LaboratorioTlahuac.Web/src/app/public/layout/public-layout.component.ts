import { Component } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterOutlet],
  template: `
    <header class="public-header">
      <a routerLink="/" class="brand" aria-label="Ir al inicio">
        <span class="brand-mark" aria-hidden="true">LDT</span>
        <span>Laboratorio Dental Tláhuac</span>
      </a>
      <nav aria-label="Navegación pública">
        <a routerLink="/catalogo">Catálogo</a>
        <a routerLink="/servicios">Servicios</a>
        <a routerLink="/contacto">Contacto</a>
        <a routerLink="/login" class="login-link">Iniciar sesión</a>
      </nav>
    </header>
    <main class="public-main">
      <router-outlet />
    </main>
    <footer class="public-footer">
      <strong>Laboratorio Dental Tláhuac</strong>
      <span>laboratoriodentaltlahuac.com</span>
      <span>Contacto, ubicación y horarios se publicarán solo con datos confirmados.</span>
    </footer>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent {}
