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
        Esta sección queda preparada para publicar WhatsApp, dirección y horarios cuando el cliente confirme
        la información. Por ahora no se muestra ningún dato como definitivo.
      </p>
      <div class="contact-grid">
        <article>
          <strong>WhatsApp</strong>
          <span>No confirmado.</span>
        </article>
        <article>
          <strong>Dirección</strong>
          <span>No confirmada.</span>
        </article>
        <article>
          <strong>Horarios</strong>
          <span>No confirmados.</span>
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
        color: #0f766e;
        font-size: 0.78rem;
        font-weight: 800;
        margin: 0;
        text-transform: uppercase;
      }

      h1 {
        color: #163235;
        line-height: 1.08;
      }

      p,
      span {
        color: #4b6366;
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
        border: 1px solid #dce9e7;
        border-radius: 8px;
        display: grid;
        gap: 8px;
        min-width: 0;
        padding: 18px;
      }

      strong {
        color: #163235;
      }

      .detail-action {
        align-items: center;
        background: #ffffff;
        border: 1px solid #91b9b5;
        border-radius: 7px;
        color: #0f766e;
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
        box-shadow: 0 0 0 3px rgba(15, 118, 110, 0.24);
        outline: 2px solid transparent;
      }

      @media (min-width: 768px) {
        .public-detail {
          padding-bottom: 64px;
          padding-top: 64px;
        }

        .contact-grid {
          grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .detail-action {
          justify-self: start;
        }
      }
    `
  ]
})
export class ContactPageComponent {}
