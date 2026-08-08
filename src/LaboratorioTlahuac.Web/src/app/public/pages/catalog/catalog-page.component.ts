import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';
import { CatalogProduct, CatalogSection, catalogSections } from '../../data/catalog-data';
import { PublicCatalogService } from '../../services/public-catalog.service';

type CatalogLoadState = 'api' | 'fallback' | 'loading';

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <div class="catalog-page public-animation-scope" appPublicScrollAnimations>
      <section class="catalog-hero">
        <div class="catalog-hero-copy" data-animate="fade-up">
          <p class="eyebrow">Catálogo 2026</p>
          <h1>Soluciones dentales organizadas para consultar con claridad</h1>
          <p class="catalog-intro">
            Explora materiales, restauraciones y prótesis. Consulta precios de referencia y confirma indicaciones,
            disponibilidad y tiempos directamente con el laboratorio.
          </p>
          <div class="catalog-hero-meta" aria-label="Resumen del catálogo">
            <span>{{ sections().length }} categorías</span>
            <span>{{ totalProducts() }} productos</span>
            <span>Precios MXN</span>
          </div>
          @if (catalogNoticeText(); as catalogNoticeText) {
            <p
              class="catalog-data-status"
              [class.is-fallback]="catalogLoadState() === 'fallback'"
              aria-live="polite"
            >
              {{ catalogNoticeText }}
            </p>
          }
        </div>
        <a class="login-action" routerLink="/contacto" data-animate="fade-in">Contactar al laboratorio</a>
      </section>

      <section class="catalog-workspace" aria-label="Catálogo de productos y precios">
        <aside class="catalog-sidebar" aria-label="Categorías del catálogo">
          <div class="catalog-sidebar-heading">
            <p class="eyebrow">Categorías</p>
            <h2>Explora el catálogo</h2>
            <p>Selecciona un material o tipo de trabajo para consultar únicamente sus productos.</p>
          </div>
          <nav class="category-list">
            @for (section of sections(); track section.id) {
              <button
                class="category-button"
                type="button"
                [class.is-active]="section.id === selectedSectionKey()"
                [attr.aria-current]="section.id === selectedSectionKey() ? 'true' : null"
                (click)="selectSection(section.id)"
              >
                <span>
                  <strong>{{ section.name }}</strong>
                  <small>{{ productCountLabel(section.products.length) }}</small>
                </span>
                <span aria-hidden="true">›</span>
              </button>
            }
          </nav>
        </aside>

        <div class="catalog-main">
          <div class="catalog-mobile-controls">
            <label class="category-select">
              <span>Selecciona una categoría</span>
              <select [value]="selectedSectionKey()" (change)="selectSectionFromEvent($event)">
                @for (section of sections(); track section.id) {
                  <option [value]="section.id">
                    {{ section.name }} · {{ productCountLabel(section.products.length) }}
                  </option>
                }
              </select>
            </label>

            <nav class="category-strip" aria-label="Categorías del catálogo">
              @for (section of sections(); track section.id) {
                <button
                  class="category-pill"
                  type="button"
                  [class.is-active]="section.id === selectedSectionKey()"
                  [attr.aria-current]="section.id === selectedSectionKey() ? 'true' : null"
                  (click)="selectSection(section.id)"
                >
                  {{ section.name }}
                </button>
              }
            </nav>
          </div>

          <article class="category-content" [id]="selectedSection().id">
            <header class="category-header" data-animate="fade-up">
              <div class="category-copy">
                <p class="eyebrow">Sección seleccionada</p>
                <h2>{{ selectedSection().name }}</h2>
                <p>
                  Consulta los productos disponibles y sus precios de referencia. Para indicaciones específicas,
                  materiales o tiempos de entrega, contacta directamente al laboratorio.
                </p>
                <div class="category-meta">
                  <span>{{ productCountLabel(currentProducts().length) }}</span>
                  <span>Precios en MXN</span>
                </div>
              </div>

              @if (getSectionImage(selectedSection()); as sectionImage) {
                <figure class="category-image">
                  <img
                    [src]="sectionImage"
                    [alt]="getSectionImageAlt(selectedSection())"
                    loading="eager"
                    decoding="async"
                    (error)="markImageMissing(sectionImage)"
                  />
                </figure>
              }
            </header>

            @if (currentProducts().length > 0) {
              <div class="product-grid">
                @for (product of currentProducts(); track product.id) {
                  <article class="product-card">
                    <div class="image-frame">
                      @if (getProductImage(product); as productImage) {
                        <img
                          [src]="productImage"
                          [alt]="getProductImageAlt(product)"
                          loading="lazy"
                          decoding="async"
                          (error)="markImageMissing(productImage)"
                        />
                      } @else {
                        <div
                          class="image-placeholder"
                          role="img"
                          [attr.aria-label]="'Sin imagen disponible para ' + product.name"
                        >
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
            } @else {
              <p class="catalog-empty-state">Esta categoría no tiene productos publicados por el momento.</p>
            }
          </article>

          <section class="catalog-cta" data-animate="fade-up">
            <div>
              <p class="eyebrow">Atención directa</p>
              <h2>¿Necesitas confirmar un trabajo?</h2>
              <p>Consulta disponibilidad, indicaciones técnicas y tiempos de entrega con el laboratorio.</p>
            </div>
            <a class="login-action" routerLink="/contacto">Ver datos de contacto</a>
          </section>
        </div>
      </section>
    </div>
  `,
  styleUrl: './catalog-page.component.scss'
})
export class CatalogPageComponent {
  readonly sections = signal<readonly CatalogSection[]>(catalogSections);
  readonly selectedSectionKey = signal(catalogSections[0].id);
  readonly catalogLoadState = signal<CatalogLoadState>('loading');
  readonly missingImageUrls = signal<ReadonlySet<string>>(new Set<string>());

  readonly totalProducts = computed(() =>
    this.sections().reduce((total, section) => total + section.products.length, 0)
  );
  readonly selectedSection = computed(() => {
    const sections = this.sections();

    return sections.find((section) => section.id === this.selectedSectionKey()) ?? sections[0] ?? catalogSections[0];
  });
  readonly currentProducts = computed(() => this.selectedSection().products);
  readonly catalogNoticeText = computed(() => {
    if (this.catalogLoadState() === 'loading') {
      return 'Actualizando catálogo...';
    }

    if (this.catalogLoadState() === 'fallback') {
      return 'Mostrando el catálogo de referencia disponible.';
    }

    return '';
  });

  private readonly destroyRef = inject(DestroyRef);
  private readonly publicCatalogService = inject(PublicCatalogService);
  private readonly priceFormatter = new Intl.NumberFormat('es-MX', {
    currency: 'MXN',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency'
  });

  private readonly onHashChange = () => {
    this.selectSectionFromHash();
  };

  constructor() {
    this.selectSectionFromHash();
    this.loadPublicCatalog();

    if (typeof window !== 'undefined') {
      window.addEventListener('hashchange', this.onHashChange);
    }

    this.destroyRef.onDestroy(() => {
      if (typeof window !== 'undefined') {
        window.removeEventListener('hashchange', this.onHashChange);
      }
    });
  }

  formatPrice(price: number) {
    return this.priceFormatter.format(price);
  }

  productCountLabel(count: number) {
    return `${count} ${count === 1 ? 'producto' : 'productos'}`;
  }

  selectSection(sectionKey: string, updateUrl = true) {
    if (!this.sections().some((section) => section.id === sectionKey)) {
      return;
    }

    this.selectedSectionKey.set(sectionKey);

    if (updateUrl) {
      this.updateSectionHash(sectionKey);
    }
  }

  selectSectionFromEvent(event: Event) {
    const target = event.target;

    if (target instanceof HTMLSelectElement) {
      this.selectSection(target.value);
    }
  }

  getSectionImage(section: CatalogSection) {
    return section.imageUrl && !this.isImageMissing(section.imageUrl) ? section.imageUrl : '';
  }

  getSectionImageAlt(section: CatalogSection) {
    return section.altText ?? `Imagen representativa de ${section.name}`;
  }

  getProductImage(product: CatalogProduct) {
    return product.imageUrl && !this.isImageMissing(product.imageUrl) ? product.imageUrl : '';
  }

  getProductImageAlt(product: CatalogProduct) {
    return product.altText ?? `${product.name} - ${this.selectedSection().name}`;
  }

  markImageMissing(imageUrl: string) {
    this.missingImageUrls.update((currentUrls) => new Set(currentUrls).add(imageUrl));
  }

  getInitials(name: string) {
    return name
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0])
      .join('')
      .toUpperCase();
  }

  private loadPublicCatalog() {
    this.catalogLoadState.set('loading');

    this.publicCatalogService
      .getPublicCatalog()
      .pipe(
        timeout(2500),
        finalize(() => {
          if (this.catalogLoadState() === 'loading') {
            this.useFallbackCatalog();
          }
        })
      )
      .subscribe({
        next: (sections) => {
          this.applyCatalogSections(sections);
          this.catalogLoadState.set('api');
        },
        error: () => this.useFallbackCatalog()
      });
  }

  private useFallbackCatalog() {
    this.applyCatalogSections(catalogSections);
    this.catalogLoadState.set('fallback');
  }

  private applyCatalogSections(sections: readonly CatalogSection[]) {
    const currentSectionKey = this.selectedSectionKey();
    const hashSectionKey = this.getCurrentHashSectionId();

    this.sections.set(sections);

    const nextSectionKey =
      [hashSectionKey, currentSectionKey].find((key) => sections.some((section) => section.id === key)) ?? sections[0].id;

    this.selectedSectionKey.set(nextSectionKey);
  }

  private selectSectionFromHash() {
    const sectionKey = this.getCurrentHashSectionId();

    if (sectionKey) {
      this.selectSection(sectionKey, false);
    }
  }

  private updateSectionHash(sectionKey: string) {
    if (typeof window === 'undefined') {
      return;
    }

    const url = `${window.location.pathname}${window.location.search}#${encodeURIComponent(sectionKey)}`;
    window.history.replaceState(window.history.state, '', url);
  }

  private getCurrentHashSectionId() {
    if (typeof window === 'undefined') {
      return '';
    }

    const sectionKey = window.location.hash.replace('#', '');

    try {
      return decodeURIComponent(sectionKey);
    } catch {
      return sectionKey;
    }
  }

  private isImageMissing(imageUrl: string) {
    return this.missingImageUrls().has(imageUrl);
  }
}
