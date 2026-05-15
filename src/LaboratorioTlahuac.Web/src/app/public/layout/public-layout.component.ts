import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <header class="public-header">
      <a routerLink="/" class="brand" aria-label="Ir al inicio">
        <img
          class="brand-logo"
          src="/assets/brand/logo-ldt.webp"
          alt=""
          width="809"
          height="545"
          aria-hidden="true"
        />
        <span class="brand-copy">
          <span class="brand-name">Laboratorio Dental Tláhuac</span>
          <span class="brand-tagline">Precisión • Estética • Confianza</span>
        </span>
      </a>
      <nav aria-label="Navegación pública">
        <a routerLink="/catalogo" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Catálogo</a>
        <a routerLink="/servicios" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Servicios</a>
        <a routerLink="/contacto" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Contacto</a>
        <a routerLink="/login" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }" class="login-link">Iniciar sesión</a>
      </nav>
    </header>
    <main class="public-main">
      <router-outlet />
    </main>
    <footer class="public-footer">
      <div>
        <strong>Laboratorio Dental Tláhuac</strong>
        <span>Precisión • Estética • Confianza</span>
      </div>
      <div>
        <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        <a href="tel:+525533319445">55 3331 9445</a>
      </div>
      <span>Dirección y horarios se publicarán solo con datos confirmados.</span>
    </footer>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent {}
