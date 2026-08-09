import { Component, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

@Component({
  selector: 'app-public-layout',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <header class="public-header">
      <div class="header-inner">
        <a routerLink="/" class="brand" aria-label="Ir al inicio" (click)="closeMobileMenu()">
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

        <nav class="desktop-nav" aria-label="Navegación pública">
          <a routerLink="/" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Inicio</a>
          <a routerLink="/servicios" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Servicios</a>
          <a routerLink="/catalogo" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Catálogo</a>
          <a routerLink="/contacto" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }">Contacto</a>
          <a routerLink="/login" class="system-access">Acceso al sistema</a>
        </nav>

        <button
          type="button"
          class="mobile-menu-toggle"
          aria-controls="public-mobile-nav"
          [attr.aria-expanded]="mobileMenuOpen()"
          aria-label="Abrir o cerrar navegación"
          (click)="toggleMobileMenu()"
        >
          <span aria-hidden="true"></span>
          <span aria-hidden="true"></span>
          <span aria-hidden="true"></span>
          <span class="mobile-menu-label">Menú</span>
        </button>
      </div>

      <nav
        id="public-mobile-nav"
        class="mobile-nav"
        [class.is-open]="mobileMenuOpen()"
        [attr.aria-hidden]="!mobileMenuOpen()"
        aria-label="Navegación móvil"
      >
        <a routerLink="/" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }" (click)="closeMobileMenu()">Inicio</a>
        <a routerLink="/servicios" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }" (click)="closeMobileMenu()">Servicios</a>
        <a routerLink="/catalogo" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }" (click)="closeMobileMenu()">Catálogo</a>
        <a routerLink="/contacto" routerLinkActive="is-active" [routerLinkActiveOptions]="{ exact: true }" (click)="closeMobileMenu()">Contacto</a>
        <a routerLink="/login" class="system-access" (click)="closeMobileMenu()">Acceso al sistema</a>
      </nav>
    </header>

    <main id="contenido-principal" class="public-main">
      <router-outlet />
    </main>

    <footer class="public-footer">
      <div class="footer-inner">
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
            <span>Prótesis, restauraciones y soluciones dentales.</span>
          </div>
        </div>

        <nav class="footer-nav" aria-label="Navegación de pie de página">
          <strong>Explora</strong>
          <a routerLink="/servicios">Servicios</a>
          <a routerLink="/catalogo">Catálogo</a>
          <a routerLink="/contacto">Contacto</a>
        </nav>

        <div class="footer-contact" aria-label="Datos de contacto">
          <strong>Contacto</strong>
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="tel:+525521612311">55 2161 2311</a>
          <a href="tel:+525598029816">55 9802 9816</a>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        </div>
      </div>

      <div class="footer-bottom">
        <span>Precisión • Estética • Confianza</span>
        <a routerLink="/login">Acceso al sistema</a>
      </div>
    </footer>
  `,
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent {
  readonly mobileMenuOpen = signal(false);

  toggleMobileMenu() {
    this.mobileMenuOpen.update((isOpen) => !isOpen);
  }

  closeMobileMenu() {
    this.mobileMenuOpen.set(false);
  }
}
