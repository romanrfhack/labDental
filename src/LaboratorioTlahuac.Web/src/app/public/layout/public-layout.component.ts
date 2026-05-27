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
      <nav class="public-nav" aria-label="Navegación pública">
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
      <div class="footer-brand">
        <img
          class="footer-logo"
          src="/assets/brand/logo-ldt.webp"
          alt=""
          width="809"
          height="545"
          aria-hidden="true"
        />
        <div>
          <strong>Laboratorio Dental Tláhuac</strong>
          <span>Precisión • Estética • Confianza</span>
        </div>
      </div>
      <div class="footer-contact" aria-label="Datos de contacto">
        <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        <a href="tel:+525533319445">55 3331 9445</a>
        <a href="tel:+525521612311">55 2161 2311</a>
        <a href="tel:+525598029816">55 9802 9816</a>
      </div>
      <span class="footer-note">Dirección, horarios y WhatsApp se publicarán solo con datos confirmados.</span>
    </footer>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent {}
