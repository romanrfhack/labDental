import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { CatalogProduct, CatalogSection, catalogSections } from '../../data/catalog-data';

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink],
  template: `
    <section class="catalog-hero">
      <div>
        <p class="eyebrow">Catálogo público</p>
        <h1>Productos y precios</h1>
        <p>
          Lista inicial de laboratorio organizada por secciones. Las imágenes se cargan desde assets locales
          cuando existen; si falta una imagen específica, se usa una referencia de sección o un placeholder.
          Precios de referencia 2026 sujetos a confirmación.
        </p>
      </div>
      <a class="login-action" routerLink="/login">Iniciar sesión</a>
    </section>

    <nav class="category-nav" aria-label="Secciones del catálogo">
      @for (section of sections; track section.id) {
        <a [attr.href]="'/catalogo#' + section.id">{{ section.name }}</a>
      }
    </nav>

    <section class="catalog-summary" aria-label="Resumen del catálogo">
      <div>
        <strong>{{ sections.length }}</strong>
        <span>secciones</span>
      </div>
      <div>
        <strong>{{ totalProducts }}</strong>
        <span>productos</span>
      </div>
      <div>
        <strong>MXN</strong>
        <span>precios de referencia</span>
      </div>
    </section>

    <section class="catalog-content">
      @for (section of sections; track section.id) {
        <article class="catalog-section" [id]="section.id">
          <div class="section-heading">
            <p class="eyebrow">Sección</p>
            <h2>{{ section.name }}</h2>
          </div>

          <div class="product-grid">
            @for (product of section.products; track product.id) {
              <article class="product-card">
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
