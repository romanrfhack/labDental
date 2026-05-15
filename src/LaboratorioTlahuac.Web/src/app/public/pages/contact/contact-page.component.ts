import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-contact-page',
  imports: [RouterLink],
  template: `
    <section class="public-detail">
      <p class="eyebrow">Contacto</p>
      <h1>Datos de contacto</h1>
      <p>
        Teléfonos y correo tomados del cartel/catálogo. Se muestran como canales de contacto generales; WhatsApp,
        dirección y horarios siguen pendientes de confirmación.
      </p>
      <div class="contact-grid">
        <article>
          <strong>Teléfonos</strong>
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="tel:+525521612311">55 2161 2311</a>
          <a href="tel:+525598029816">55 9802 9816</a>
        </article>
        <article>
          <strong>Correo</strong>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        </article>
        <article>
          <strong>Dirección</strong>
          <span>No confirmada.</span>
        </article>
        <article>
          <strong>Horarios</strong>
          <span>No confirmados.</span>
        </article>
        <article>
          <strong>WhatsApp</strong>
          <span>Pendiente de confirmar; no se publica como canal real todavía.</span>
        </article>
      </div>
      <a class="detail-action" routerLink="/login">Iniciar sesión</a>
    </section>
  `,
  styles: [
    `
      .public-detail {
        display: grid;
        gap: 18px;
        padding: 44px max(16px, calc((100vw - 960px) / 2));
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
        line-height: 1.08;
      }

      p,
      span {
        color: var(--ldt-gray, #65738e);
        line-height: 1.6;
      }

      .public-detail > p {
        max-width: 720px;
      }

      .contact-grid {
        display: grid;
        gap: 12px;
      }

      article {
        background: #ffffff;
        border: 1px solid rgba(84, 177, 232, 0.35);
        border-radius: 8px;
        display: grid;
        gap: 8px;
        min-width: 0;
        padding: 18px;
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
        background: #ffffff;
        border: 1px solid var(--ldt-sky, #54b1e8);
        border-radius: 7px;
        color: var(--ldt-navy-soft, #0b3069);
        display: inline-flex;
        font-weight: 800;
        justify-content: center;
        min-height: 48px;
        overflow-wrap: anywhere;
        padding: 12px 16px;
        text-align: center;
        text-decoration: none;
      }

      .detail-action:focus-visible,
      .detail-action:hover {
        box-shadow: 0 0 0 3px var(--ldt-focus, rgba(3, 158, 247, 0.24));
        outline: 2px solid transparent;
      }

      @media (min-width: 768px) {
        .public-detail {
          padding-bottom: 64px;
          padding-top: 64px;
        }

        .contact-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }

        .detail-action {
          justify-self: start;
        }
      }
    `
  ]
})
export class ContactPageComponent {}
