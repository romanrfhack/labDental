import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-services-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <section class="public-detail public-animation-scope" appPublicScrollAnimations>
      <div class="detail-hero" data-animate="fade-up">
        <p class="eyebrow">Servicios</p>
        <h1>Servicios del laboratorio</h1>
        <p>
          Consulta el catálogo público para revisar secciones, productos, precios e imágenes disponibles. Esta
          página mantiene una vista resumida para orientar a doctores, consultorios y clínicas con la identidad
          visual del laboratorio.
        </p>
      </div>
      <div class="detail-grid">
        <article data-animate="stagger-card">
          <span class="card-index" aria-hidden="true">01</span>
          <strong>Catálogo público</strong>
          <span>Productos y precios organizados por secciones.</span>
        </article>
        <article data-animate="stagger-card">
          <span class="card-index" aria-hidden="true">02</span>
          <strong>Seguimiento interno</strong>
          <span>Acceso privado para operación administrativa del laboratorio.</span>
        </article>
        <article data-animate="stagger-card">
          <span class="card-index" aria-hidden="true">03</span>
          <strong>Comunicación con clínicas</strong>
          <span>Prótesis, restauraciones y soluciones dentales con contacto por teléfono y correo.</span>
        </article>
      </div>
      <div class="service-band" data-animate="fade-up">
        <strong>Ruta recomendada</strong>
        <span>Revisar el catálogo, confirmar precio vigente y coordinar por teléfono o correo.</span>
      </div>
      <div class="detail-actions" data-animate="fade-up">
        <a class="detail-action primary" routerLink="/catalogo">Ver catálogo</a>
        <a class="detail-action" routerLink="/contacto">Ver contacto</a>
      </div>
    </section>
  `,
  styles: [
    `
      .public-detail {
        background:
          linear-gradient(180deg, rgba(166, 223, 247, 0.24), rgba(255, 255, 255, 0.88) 34%),
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

      .detail-grid {
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

      .card-index {
        color: var(--ldt-blue-dark, #0781e6);
        font-size: 0.78rem;
        font-weight: 900;
      }

      strong {
        color: var(--ldt-navy, #032155);
      }

      .service-band {
        background: var(--ldt-navy, #032155);
        border: 1px solid rgba(166, 223, 247, 0.28);
        border-radius: 8px;
        box-shadow: 0 18px 44px rgba(3, 33, 85, 0.12);
        display: grid;
        gap: 6px;
        padding: 18px;
      }

      .service-band strong {
        color: var(--ldt-white, #ffffff);
      }

      .service-band span {
        color: #d7ecff;
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
        transition:
          box-shadow 160ms ease,
          transform 160ms ease;
      }

      .detail-action.primary {
        background: linear-gradient(135deg, var(--ldt-blue-dark, #0781e6), var(--ldt-blue, #039ef7));
        border-color: var(--ldt-blue-dark, #0781e6);
        box-shadow: 0 12px 28px rgba(3, 158, 247, 0.2);
        color: var(--ldt-white, #ffffff);
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
export class ServicesPageComponent {}
