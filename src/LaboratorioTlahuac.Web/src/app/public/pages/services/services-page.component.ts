import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-services-page',
  imports: [RouterLink],
  template: `
    <section class="public-detail">
      <p class="eyebrow">Servicios</p>
      <h1>Servicios del laboratorio</h1>
      <p>
        Consulta el catálogo público para revisar secciones, productos, precios e imágenes disponibles. Esta
        página mantiene una vista resumida para orientar a doctores, consultorios y clínicas con la identidad
        visual del laboratorio.
      </p>
      <div class="detail-grid">
        <article>
          <strong>Catálogo público</strong>
          <span>Productos y precios organizados por secciones.</span>
        </article>
        <article>
          <strong>Seguimiento interno</strong>
          <span>Acceso privado para operación administrativa del laboratorio.</span>
        </article>
        <article>
          <strong>Comunicación con clínicas</strong>
          <span>Prótesis, restauraciones y soluciones dentales con contacto por teléfono y correo.</span>
        </article>
      </div>
      <div class="detail-actions">
        <a class="detail-action primary" routerLink="/catalogo">Ver catálogo</a>
        <a class="detail-action" routerLink="/contacto">Ver contacto</a>
      </div>
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

      .detail-grid {
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

      .detail-actions {
        display: grid;
        gap: 10px;
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

      .detail-action.primary {
        background: var(--ldt-blue-dark, #0781e6);
        border-color: var(--ldt-blue-dark, #0781e6);
        color: var(--ldt-white, #ffffff);
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

        .detail-grid {
          grid-template-columns: repeat(3, minmax(0, 1fr));
        }

        .detail-actions {
          display: flex;
          flex-wrap: wrap;
        }

        .detail-action {
          justify-self: start;
        }
      }
    `
  ]
})
export class ServicesPageComponent {}
