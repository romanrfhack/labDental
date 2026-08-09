import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <div class="home-page home-v2 public-animation-scope" appPublicScrollAnimations>
      <section class="home-v2-hero">
        <div class="home-v2-hero-inner">
          <div class="home-v2-copy" data-animate="fade-up">
            <p class="home-v2-eyebrow">Laboratorio Dental Tláhuac</p>
            <h1>Prótesis y restauraciones dentales con una consulta más clara</h1>
            <p class="home-v2-lead">
              Explora materiales, tipos de trabajo y precios de referencia antes de coordinar directamente con el laboratorio.
            </p>

            <div class="home-v2-actions" aria-label="Acciones principales">
              <a class="home-v2-btn primary" routerLink="/catalogo">Explorar catálogo</a>
              <a class="home-v2-btn secondary" routerLink="/contacto">Hablar con el laboratorio</a>
            </div>

            <div class="home-v2-trust" aria-label="Ventajas del sitio">
              <span>Catálogo por categorías</span>
              <span>Precios de referencia</span>
              <span>Contacto directo</span>
            </div>
          </div>

          <div class="home-v2-visual" data-animate="fade-in" aria-label="Muestra de trabajos dentales">
            <figure class="home-v2-main-image">
              <img
                src="/assets/catalog/products/zirconia-corona-estratificada.webp"
                alt="Corona de zirconia, imagen representativa del catálogo"
                width="760"
                height="570"
              />
              <figcaption>
                <span>Zirconia</span>
                <strong>Restauraciones estéticas</strong>
              </figcaption>
            </figure>

            <a class="home-v2-mini-card" routerLink="/catalogo" fragment="emax">
              <img
                src="/assets/catalog/products/emax-corona-estratificada.webp"
                alt="Corona E-MAX"
                width="360"
                height="270"
              />
              <span>E-MAX</span>
            </a>

            <a class="home-v2-mini-card" routerLink="/catalogo" fragment="prostodoncia-parcial-total">
              <img
                src="/assets/catalog/products/prostodoncia-dentadura-total-luciton.webp"
                alt="Prótesis total"
                width="360"
                height="270"
              />
              <span>Prótesis removible</span>
            </a>
          </div>
        </div>
      </section>

      <section class="home-v2-section home-v2-categories" aria-labelledby="categorias-title">
        <div class="home-v2-section-heading" data-animate="fade-up">
          <p class="home-v2-eyebrow">Catálogo</p>
          <h2 id="categorias-title">Encuentra rápido el tipo de trabajo que necesitas</h2>
          <p>Accede directamente a algunas de las familias más consultadas. El catálogo conserva el detalle completo y administrable.</p>
        </div>

        <div class="home-v2-category-grid">
          <a routerLink="/catalogo" fragment="zirconia" class="home-v2-category-card" data-animate="stagger-card">
            <img src="/assets/catalog/products/zirconia-corona-monolitica.webp" alt="Corona monolítica de zirconia" width="520" height="390" />
            <div>
              <span>01</span>
              <strong>Zirconia</strong>
              <small>Coronas, carillas e incrustaciones</small>
            </div>
          </a>

          <a routerLink="/catalogo" fragment="emax" class="home-v2-category-card" data-animate="stagger-card">
            <img src="/assets/catalog/products/emax-incrustacion.webp" alt="Incrustación E-MAX" width="520" height="390" />
            <div>
              <span>02</span>
              <strong>E-MAX</strong>
              <small>Restauraciones libres de metal</small>
            </div>
          </a>

          <a routerLink="/catalogo" fragment="provisionales-guardas" class="home-v2-category-card" data-animate="stagger-card">
            <img src="/assets/catalog/products/provisionales-guarda-oclusal-acrilico.webp" alt="Guarda oclusal de acrílico" width="520" height="390" />
            <div>
              <span>03</span>
              <strong>Provisionales y guardas</strong>
              <small>Auxiliares para distintas etapas clínicas</small>
            </div>
          </a>

          <a routerLink="/catalogo" fragment="prostodoncia-parcial-total" class="home-v2-category-card" data-animate="stagger-card">
            <img src="/assets/catalog/products/prostodoncia-dentadura-total-kulzer.webp" alt="Dentadura total en acrílico" width="520" height="390" />
            <div>
              <span>04</span>
              <strong>Prótesis parcial y total</strong>
              <small>Opciones removibles y acrílicas</small>
            </div>
          </a>
        </div>

        <a class="home-v2-text-link" routerLink="/catalogo">Ver todas las categorías <span aria-hidden="true">→</span></a>
      </section>

      <section class="home-v2-section home-v2-process" aria-labelledby="proceso-title">
        <div class="home-v2-section-heading" data-animate="fade-up">
          <p class="home-v2-eyebrow">Proceso</p>
          <h2 id="proceso-title">Del catálogo a la coordinación del trabajo</h2>
          <p>Una ruta sencilla para reducir dudas antes del primer contacto.</p>
        </div>

        <ol class="home-v2-steps">
          <li data-animate="stagger-card">
            <span>01</span>
            <strong>Explora</strong>
            <p>Ubica el material o tipo de trabajo dentro del catálogo.</p>
          </li>
          <li data-animate="stagger-card">
            <span>02</span>
            <strong>Revisa</strong>
            <p>Consulta opciones, imágenes y precios de referencia disponibles.</p>
          </li>
          <li data-animate="stagger-card">
            <span>03</span>
            <strong>Coordina</strong>
            <p>Confirma indicaciones, disponibilidad, tiempos y precio final con el laboratorio.</p>
          </li>
        </ol>
      </section>

      <section class="home-v2-section home-v2-prep" aria-labelledby="preparar-title">
        <div class="home-v2-prep-copy" data-animate="fade-up">
          <p class="home-v2-eyebrow">Antes de contactar</p>
          <h2 id="preparar-title">Ten a la mano la información clave del caso</h2>
          <p>Con estos datos la coordinación puede ser más directa desde el primer contacto.</p>
        </div>
        <div class="home-v2-prep-list" data-animate="fade-in">
          <span>Tipo de trabajo</span>
          <span>Material o alternativa deseada</span>
          <span>Indicaciones relevantes</span>
          <span>Fecha requerida</span>
        </div>
      </section>

      <section class="home-v2-contact" aria-labelledby="contacto-title">
        <div data-animate="fade-up">
          <p class="home-v2-eyebrow">Contacto</p>
          <h2 id="contacto-title">¿Ya sabes qué trabajo necesitas?</h2>
          <p>Comunícate directamente por teléfono o correo para confirmar el caso.</p>
        </div>
        <div class="home-v2-contact-links" data-animate="fade-in">
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
          <a class="home-v2-contact-more" routerLink="/contacto">Ver todos los datos de contacto →</a>
        </div>
      </section>
    </div>
  `,
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {}
