import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-contact-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <section class="public-detail public-animation-scope" appPublicScrollAnimations>
      <div class="detail-hero" data-animate="fade-up">
        <p class="eyebrow">Contacto</p>
        <h1>Datos de contacto</h1>
        <p>
          Teléfonos y correo tomados del cartel/catálogo. Se muestran como canales de contacto generales; WhatsApp,
          dirección y horarios siguen pendientes de confirmación.
        </p>
      </div>
      <div class="contact-grid">
        <article class="confirmed-card" data-animate="stagger-card">
          <span class="status-label">Confirmado</span>
          <strong>Teléfonos</strong>
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="tel:+525521612311">55 2161 2311</a>
          <a href="tel:+525598029816">55 9802 9816</a>
        </article>
        <article class="confirmed-card" data-animate="stagger-card">
          <span class="status-label">Confirmado</span>
          <strong>Correo</strong>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        </article>
        <article class="pending-card" data-animate="stagger-card">
          <span class="status-label">Pendiente</span>
          <strong>Dirección</strong>
          <span>No confirmada.</span>
        </article>
        <article class="pending-card" data-animate="stagger-card">
          <span class="status-label">Pendiente</span>
          <strong>Horarios</strong>
          <span>No confirmados.</span>
        </article>
        <article class="pending-card" data-animate="stagger-card">
          <span class="status-label">Pendiente</span>
          <strong>WhatsApp</strong>
          <span>Pendiente de confirmar; no se publica como canal real todavía.</span>
        </article>
      </div>
      <a class="detail-action" routerLink="/login" data-animate="fade-up">Iniciar sesión</a>
    </section>
  `,
  styles: [
    `
      .public-detail {
        background:
          linear-gradient(180deg, rgba(166, 223, 247, 0.22), rgba(255, 255, 255, 0.92) 36%),
          repeating-linear-gradient(115deg, rgba(84, 177, 232, 0.09) 0 1px, transparent 1px 32px);
        display: grid;
        gap: 22px;
        padding: 44px max(16px, calc((100vw - 960px) / 2));
      }

      .detail-hero {
        max-width: 760px;
      }

      .eyebrow {
        color: var(--ldt-blue-dark, #0781e6);
        font-size: 0.78rem;
        font-weight: 800;
        margin: 0;
        text-transform: uppercase;
      }

      h1 {
        color: var(--ldt-navy, #032155);
        font-size: 2.15rem;
        line-height: 1.08;
        margin: 0 0 12px;
      }

      p,
      span {
        color: var(--ldt-gray, #65738e);
        line-height: 1.6;
      }

      .detail-hero p {
        max-width: 720px;
      }

      .contact-grid {
        display: grid;
        gap: 12px;
      }

      article {
        background:
          linear-gradient(180deg, #ffffff, #f7fcff);
        border: 1px solid rgba(84, 177, 232, 0.35);
        border-radius: 8px;
        box-shadow: 0 14px 30px rgba(3, 33, 85, 0.06);
        display: grid;
        gap: 8px;
        min-width: 0;
        padding: 18px;
        transition:
          border-color 160ms ease,
          box-shadow 160ms ease,
          transform 160ms ease;
      }

      .confirmed-card {
        border-top: 4px solid var(--ldt-blue-dark, #0781e6);
      }

      .pending-card {
        border-top: 4px solid rgba(101, 115, 142, 0.42);
      }

      .status-label {
        background: rgba(166, 223, 247, 0.34);
        border: 1px solid rgba(84, 177, 232, 0.34);
        border-radius: 999px;
        color: var(--ldt-navy-soft, #0b3069);
        display: inline-flex;
        font-size: 0.74rem;
        font-weight: 900;
        justify-self: start;
        line-height: 1;
        padding: 6px 8px;
        text-transform: uppercase;
      }

      strong {
        color: var(--ldt-navy, #032155);
      }

      article a {
        color: var(--ldt-blue-dark, #0781e6);
        font-weight: 800;
        line-height: 1.45;
        overflow-wrap: anywhere;
        text-decoration-color: var(--ldt-sky, #54b1e8);
        text-underline-offset: 3px;
      }

      .detail-action {
        align-items: center;
        background: linear-gradient(135deg, var(--ldt-blue-dark, #0781e6), var(--ldt-blue, #039ef7));
        border: 1px solid var(--ldt-blue-dark, #0781e6);
        border-radius: 7px;
        box-shadow: 0 12px 28px rgba(3, 158, 247, 0.2);
        color: var(--ldt-white, #ffffff);
        display: inline-flex;
        font-weight: 800;
        justify-content: center;
        min-height: 48px;
        overflow-wrap: anywhere;
        padding: 12px 16px;
        text-align: center;
        text-decoration: none;
        transition:
          box-shadow 160ms ease,
          transform 160ms ease;
      }

      .detail-action:focus-visible,
      .detail-action:hover {
        box-shadow: 0 0 0 3px var(--ldt-focus, rgba(3, 158, 247, 0.24));
        outline: 2px solid transparent;
      }

      .detail-action:hover {
        transform: translateY(-1px);
      }

      @media (hover: hover) {
        article:hover {
          border-color: rgba(3, 158, 247, 0.42);
          box-shadow: 0 18px 42px rgba(3, 33, 85, 0.1);
          transform: translateY(-3px);
        }
      }

      @media (min-width: 768px) {
        .public-detail {
          padding-bottom: 64px;
          padding-top: 64px;
        }

        h1 {
          font-size: 3rem;
        }

        .contact-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }

        .detail-action {
          justify-self: start;
        }
      }

      @media (prefers-reduced-motion: reduce) {
        article,
        .detail-action {
          transition: none;
        }

        article:hover,
        .detail-action:hover {
          transform: none;
        }
      }
    `
  ]
})
export class ContactPageComponent {}
