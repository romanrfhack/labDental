import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <div class="home-page public-animation-scope" appPublicScrollAnimations>
    <section class="hero-section">
      <div class="hero-copy" data-animate="fade-up">
        <img
          class="hero-logo"
          src="/assets/brand/logo-ldt.webp"
          alt="Laboratorio Dental Tláhuac"
          width="809"
          height="545"
          data-parallax="0.08"
        />
        <p class="eyebrow">Precisión • Estética • Confianza</p>
        <h1>Prótesis, restauraciones y soluciones dentales</h1>
        <p class="hero-text">
          Catálogo público del laboratorio y entrada al sistema administrativo para seguimiento operativo interno.
        </p>
        <div class="hero-actions" aria-label="Acciones principales">
          <a class="primary-action" routerLink="/catalogo">Ver catálogo</a>
          <a class="secondary-action" routerLink="/contacto">Contactar</a>
        </div>
      </div>
      <div class="hero-panel" aria-label="Información resumida" data-animate="fade-in">
        <span class="panel-label">Catálogo de precios 2026</span>
        <strong>Doctores, consultorios y clínicas dentales</strong>
        <p>Precios de referencia sujetos a confirmación del cliente antes de publicación formal.</p>
        <a href="tel:+525533319445">55 3331 9445</a>
      </div>
    </section>
    <section class="section-block" id="servicios">
      <div class="section-heading" data-animate="fade-up">
        <p class="eyebrow">Capacidades</p>
        <h2>Catálogo público con precios</h2>
        <p>
          La nueva ruta /catalogo muestra productos organizados por categoría, precios e imágenes locales
          cuando están disponibles.
        </p>
      </div>
      <div class="card-grid">
        <article data-animate="stagger-card">
          <span aria-hidden="true">01</span>
          <h3>Catálogo de servicios</h3>
          <p>Productos y precios organizados por secciones para revisión pública del catálogo 2026.</p>
        </article>
        <article data-animate="stagger-card">
          <span aria-hidden="true">02</span>
          <h3>Seguimiento interno</h3>
          <p>Entrada al sistema administrativo para revisar clientes, órdenes, pagos y estado operativo.</p>
        </article>
        <article data-animate="stagger-card">
          <span aria-hidden="true">03</span>
          <h3>Comunicación profesional</h3>
          <p>Contacto visible por teléfono y correo, sin publicar dirección, horarios ni WhatsApp no confirmados.</p>
        </article>
      </div>
    </section>
    <section class="section-block process-section" id="proceso">
      <div class="section-heading" data-animate="fade-up">
        <p class="eyebrow">Proceso</p>
        <h2>Una ruta simple para trabajar con el laboratorio</h2>
      </div>
      <ol class="process-list">
        <li data-animate="stagger-card">
          <strong>Contacto</strong>
          <span>El doctor, consultorio o clínica puede llamar o escribir al correo publicado.</span>
        </li>
        <li data-animate="stagger-card">
          <strong>Coordinación</strong>
          <span>Se revisan indicaciones, tiempos y datos necesarios para cada trabajo.</span>
        </li>
        <li data-animate="stagger-card">
          <strong>Seguimiento</strong>
          <span>El sistema privado concentra órdenes, estados, pagos y saldos para operación interna.</span>
        </li>
      </ol>
    </section>
    <section class="section-block benefits-section">
      <div class="section-heading" data-animate="fade-up">
        <p class="eyebrow">Beneficios</p>
        <h2>Diseñado para una operación dental más clara</h2>
      </div>
      <div class="benefit-list">
        <div data-animate="stagger-card">
          <strong>Presencia digital formal</strong>
          <span>Información institucional disponible en el dominio del laboratorio.</span>
        </div>
        <div data-animate="stagger-card">
          <strong>Acceso ordenado</strong>
          <span>Entrada visible al sistema administrativo mediante /login.</span>
        </div>
        <div data-animate="stagger-card">
          <strong>Separación segura</strong>
          <span>El sitio público no expone la aplicación privada bajo /app.</span>
        </div>
      </div>
    </section>
    <section class="contact-band" id="contacto">
      <div data-animate="fade-up">
        <p class="eyebrow">Contacto</p>
        <h2>Contacto directo del laboratorio</h2>
        <p>
          Teléfonos y correo tomados del cartel/catálogo. Dirección, horarios y WhatsApp siguen pendientes de
          confirmación.
        </p>
        <div class="contact-links" aria-label="Datos de contacto">
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="tel:+525521612311">55 2161 2311</a>
          <a href="tel:+525598029816">55 9802 9816</a>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        </div>
      </div>
      <div class="contact-actions" data-animate="fade-in">
        <a class="primary-action" routerLink="/contacto">Ver contacto</a>
        <a class="secondary-action" routerLink="/login">Iniciar sesión</a>
      </div>
    </section>
    </div>
  `,
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {}
