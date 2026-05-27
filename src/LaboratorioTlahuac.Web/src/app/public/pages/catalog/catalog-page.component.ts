import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';
import { CatalogProduct, CatalogSection, catalogSections } from '../../data/catalog-data';

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <div class="catalog-page public-animation-scope" appPublicScrollAnimations>
    <section class="catalog-hero">
      <div data-animate="fade-up">
        <p class="eyebrow">Catálogo público</p>
        <h1>Productos y precios</h1>
        <p>
          Catálogo de precios 2026 organizado por secciones. Las imágenes se cargan desde assets locales cuando
          existen; si falta una imagen específica, se usa una referencia de sección o un placeholder. Precios de
          referencia 2026 sujetos a confirmación.
        </p>
      </div>
      <a class="login-action" routerLink="/contacto" data-animate="fade-in">Contactar</a>
    </section>

    <nav class="category-nav" aria-label="Secciones del catálogo">
      @for (section of sections; track section.id) {
        <a [attr.href]="'/catalogo#' + section.id">{{ section.name }}</a>
      }
    </nav>

    <section class="catalog-summary" aria-label="Resumen del catálogo">
      <div data-animate="stagger-card">
        <strong>{{ sections.length }}</strong>
        <span>secciones</span>
      </div>
      <div data-animate="stagger-card">
        <strong>{{ totalProducts }}</strong>
        <span>productos</span>
      </div>
      <div data-animate="stagger-card">
        <strong>MXN</strong>
        <span>precios de referencia</span>
      </div>
    </section>

    <section class="catalog-contact" aria-label="Contacto y condiciones del catálogo">
      <div data-animate="fade-up">
        <p class="eyebrow">Contacto</p>
        <h2>Laboratorio Dental Tláhuac</h2>
        <p>Prótesis, restauraciones y soluciones dentales.</p>
        <div class="contact-links">
          <a href="tel:+525533319445">55 3331 9445</a>
          <a href="tel:+525521612311">55 2161 2311</a>
          <a href="tel:+525598029816">55 9802 9816</a>
          <a href="mailto:contacto@laboratoriodentaltlahuac.com">contacto@laboratoriodentaltlahuac.com</a>
        </div>
      </div>
      <div class="commercial-note" data-animate="fade-up">
        <strong>Condiciones visibles en cartel</strong>
        <span>Anticipo 50% y trabajos urgentes +40% requieren confirmación final del cliente antes de publicarse como condiciones definitivas.</span>
      </div>
    </section>

    <section class="catalog-content">
      @for (section of sections; track section.id) {
        <article class="catalog-section" [id]="section.id">
          <div class="section-heading" data-animate="fade-up">
            <p class="eyebrow">Sección</p>
            <h2>{{ section.name }}</h2>
          </div>

          <div class="product-grid">
            @for (product of section.products; track product.id; let productIndex = $index) {
              <article class="product-card" [attr.data-animate]="productIndex < 6 ? 'stagger-card' : null">
                <div class="image-frame">
                  @if (shouldShowImage(section, product)) {
                    <img
                      [src]="getProductImage(section, product)"
                      [alt]="product.name"
                      loading="lazy"
                      (error)="markImageMissing(section, product)"
                    />
                    @if (!product.imageUrl && section.imageUrl) {
                      <span class="image-note">Imagen de sección</span>
                    }
                  } @else {
                    <div class="image-placeholder" aria-hidden="true">
                      <span>{{ getInitials(product.name) }}</span>
                    </div>
                  }
                </div>
                <div class="product-info">
                  <h3>{{ product.name }}</h3>
                  <span class="price">{{ formatPrice(product.price) }}</span>
                </div>
              </article>
            }
          </div>
        </article>
      }
    </section>
    </div>
  `,
  styleUrl: './catalog-page.component.scss'
})
export class CatalogPageComponent {
  readonly sections = catalogSections;
  readonly totalProducts = catalogSections.reduce((total, section) => total + section.products.length, 0);

  private readonly missingImages = new Set<string>();
  private readonly priceFormatter = new Intl.NumberFormat('es-MX', {
    currency: 'MXN',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency'
  });

  formatPrice(price: number) {
    return this.priceFormatter.format(price);
  }

  getProductImage(section: CatalogSection, product: CatalogProduct) {
    return product.imageUrl ?? section.imageUrl ?? '';
  }

  shouldShowImage(section: CatalogSection, product: CatalogProduct) {
    return Boolean(this.getProductImage(section, product)) && !this.missingImages.has(this.getImageKey(section, product));
  }

  markImageMissing(section: CatalogSection, product: CatalogProduct) {
    this.missingImages.add(this.getImageKey(section, product));
  }

  getInitials(productName: string) {
    return productName
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0])
      .join('')
      .toUpperCase();
  }

  private getImageKey(section: CatalogSection, product: CatalogProduct) {
    return `${section.id}:${product.id}`;
  }
}
