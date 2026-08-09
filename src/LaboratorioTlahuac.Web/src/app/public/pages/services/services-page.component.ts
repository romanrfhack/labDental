import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-services-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <section class="services-v2 public-animation-scope" appPublicScrollAnimations>
      <header class="services-v2-hero" data-animate="fade-up">
        <p class="services-v2-eyebrow">Servicios</p>
        <h1>Soluciones para distintas necesidades clínicas</h1>
        <p>
          Usa esta página como guía rápida y entra directamente a la familia de trabajos que quieres consultar en el catálogo.
        </p>
      </header>

      <div class="services-v2-grid">
        <article class="services-v2-card" data-animate="stagger-card">
          <img src="/assets/catalog/products/zirconia-corona-estratificada.webp" alt="Corona de zirconia" width="620" height="420" />
          <div class="services-v2-card-body">
            <span class="services-v2-index">01</span>
            <h2>Restauraciones estéticas</h2>
            <p>Opciones libres de metal y restauraciones cerámicas para distintos requerimientos.</p>
            <nav aria-label="Restauraciones estéticas">
              <a routerLink="/catalogo" fragment="zirconia">Zirconia</a>
              <a routerLink="/catalogo" fragment="emax">E-MAX</a>
              <a routerLink="/catalogo" fragment="signum">SIGNUM</a>
              <a routerLink="/catalogo" fragment="metal-porcelana">Metal-porcelana</a>
            </nav>
          </div>
        </article>

        <article class="services-v2-card" data-animate="stagger-card">
          <img src="/assets/catalog/products/iflex-protesis-bilateral.webp" alt="Prótesis removible" width="620" height="420" />
          <div class="services-v2-card-body">
            <span class="services-v2-index">02</span>
            <h2>Prótesis removible</h2>
            <p>Alternativas parciales y totales con diferentes materiales y configuraciones.</p>
            <nav aria-label="Prótesis removible">
              <a routerLink="/catalogo" fragment="totally-natural">Totally Natural</a>
              <a routerLink="/catalogo" fragment="iflex">iFlex</a>
              <a routerLink="/catalogo" fragment="prostodoncia-parcial-total">Parcial y total</a>
              <a routerLink="/catalogo" fragment="protesis-removible-metal-acrilico">Metal-acrílico</a>
            </nav>
          </div>
        </article>

        <article class="services-v2-card" data-animate="stagger-card">
          <img src="/assets/catalog/products/provisionales-guarda-oclusal-acrilico.webp" alt="Guarda oclusal de acrílico" width="620" height="420" />
          <div class="services-v2-card-body">
            <span class="services-v2-index">03</span>
            <h2>Provisionales y auxiliares</h2>
            <p>Trabajos provisionales, guardas y alternativas para etapas específicas del tratamiento.</p>
            <nav aria-label="Provisionales y auxiliares">
              <a routerLink="/catalogo" fragment="provisionales-guardas">Provisionales y guardas</a>
              <a routerLink="/catalogo" fragment="protesis-inmediata-provisional">Prótesis inmediata</a>
              <a routerLink="/catalogo" fragment="metalicos-auxiliares">Metálicos y auxiliares</a>
            </nav>
          </div>
        </article>

        <article class="services-v2-card" data-animate="stagger-card">
          <img src="/assets/catalog/products/prostodoncia-dentadura-total-luciton.webp" alt="Dentadura total" width="620" height="420" />
          <div class="services-v2-card-body">
            <span class="services-v2-index">04</span>
            <h2>Servicios complementarios</h2>
            <p>Reparaciones, rebases y servicios prostodónticos para complementar el trabajo clínico.</p>
            <nav aria-label="Servicios complementarios">
              <a routerLink="/catalogo" fragment="servicios-prostodonticos">Servicios prostodónticos</a>
              <a routerLink="/catalogo">Ver catálogo completo</a>
            </nav>
          </div>
        </article>
      </div>

      <div class="services-v2-band" data-animate="fade-up">
        <div>
          <p class="services-v2-eyebrow">Orientación</p>
          <h2>¿No estás seguro de qué opción corresponde al caso?</h2>
          <p>Consulta el catálogo y confirma indicaciones, disponibilidad y precio final directamente con el laboratorio.</p>
        </div>
        <div class="services-v2-band-actions">
          <a routerLink="/catalogo">Abrir catálogo</a>
          <a routerLink="/contacto">Ver contacto</a>
        </div>
      </div>
    </section>
  `,
  styleUrl: './services-page.component.scss'
})
export class ServicesPageComponent {}
