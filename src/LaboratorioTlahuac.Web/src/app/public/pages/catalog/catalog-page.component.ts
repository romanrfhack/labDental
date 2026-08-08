import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';
import { CatalogProduct, CatalogSection, catalogSections } from '../../data/catalog-data';
import { PublicCatalogService } from '../../services/public-catalog.service';

type CatalogLoadState = 'api' | 'fallback' | 'loading';
type CatalogHistoryMode = 'push' | 'replace';

@Component({
  selector: 'app-catalog-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <div class="catalog-page public-animation-scope" appPublicScrollAnimations>
      <section class="catalog-v2-hero">
        <div class="catalog-v2-hero-inner">
          <div data-animate="fade-up">
            <p class="eyebrow">Catálogo 2026</p>
            <h1>Soluciones dentales organizadas para consultar con claridad</h1>
            <p class="catalog-v2-hero-copy">
              Explora materiales, restauraciones y prótesis. Consulta precios de referencia y confirma indicaciones,
              disponibilidad y tiempos directamente con el laboratorio.
            </p>

            <div class="catalog-v2-meta" aria-label="Resumen del catálogo">
              <span><strong>{{ sections().length }}</strong> categorías</span>
              <span><strong>{{ totalProducts() }}</strong> productos</span>
              <span><strong>MXN</strong> moneda</span>
            </div>

            @if (catalogNoticeText(); as catalogNoticeText) {
              <p
                class="catalog-v2-data-status"
                [class.is-fallback]="catalogLoadState() === 'fallback'"
                aria-live="polite"
              >
                {{ catalogNoticeText }}
              </p>
            }
          </div>

          <div class="catalog-v2-hero-action" data-animate="fade-in">
            <a class="login-action" routerLink="/contacto">Contactar</a>
            <span>Precios sujetos a confirmación.</span>
          </div>
        </div>
      </section>

      <section class="catalog-workspace" aria-label="Catálogo de productos y precios">
        <div class="catalog-workspace-inner">
          <aside class="catalog-category-panel">
            <div class="catalog-category-heading">
              <p class="eyebrow">Categorías</p>
              <h2>Explora por material o tipo de trabajo</h2>
              <p>Selecciona una categoría para consultar únicamente sus productos.</p>
            </div>

            <label class="catalog-category-select">
              <span>Selecciona una categoría</span>
              <select
                [value]="selectedSectionKey()"
                (change)="selectSectionByKey($any($event.target).value)"
              >
                @for (section of sections(); track section.id) {
                  <option [value]="section.id">
                    {{ section.name }} · {{ productCountLabel(section.products.length) }}
                  </option>
                }
              </select>
            </label>

            <nav class="catalog-category-list" aria-label="Categorías del catálogo">
              @for (section of sections(); track section.id) {
                <button
                  class="catalog-category-button"
                  type="button"
                  [class.is-active]="section.id === selectedSectionKey()"
                  [attr.aria-current]="section.id === selectedSectionKey() ? 'true' : null"
                  (click)="selectSectionByKey(section.id)"
                >
                  <span class="catalog-category-marker" aria-hidden="true">
                    {{ getInitials(section.name) }}
                  </span>
                  <span class="catalog-category-copy">
                    <strong>{{ section.name }}</strong>
                    <span>{{ productCountLabel(section.products.length) }}</span>
                  </span>
                </button>
              }
            </nav>
          </aside>

          <section
            class="catalog-results"
            [attr.aria-labelledby]="'catalog-section-title-' + selectedSection().id"
          >
            <header
              class="catalog-section-intro"
              [class.has-media]="getSectionImage(selectedSection())"
            >
              <div class="catalog-section-copy">
                <p class="eyebrow">Categoría seleccionada</p>
                <h2 [id]="'catalog-section-title-' + selectedSection().id">
                  {{ selectedSection().name }}
                </h2>

                @if (selectedSection().description; as description) {
                  <p>{{ description }}</p>
                } @else {
                  <p>
                    Consulta los trabajos disponibles, sus imágenes y precios de referencia en esta categoría.
                  </p>
                }

                <div class="catalog-section-meta">
                  <span>{{ productCountLabel(currentProducts().length) }}</span>
                  <span>Precios en MXN</span>
                </div>
              </div>

              @if (getSectionImage(selectedSection()); as sectionImage) {
                <figure class="catalog-section-media">
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
              <div class="catalog-product-grid">
                @for (product of currentProducts(); track product.id) {
                  <article class="catalog-product-card">
                    <div class="catalog-product-media">
                      @if (getProductImage(product); as productImage) {
                        <img
                          [src]="productImage"
                          [alt]="getProductImageAlt(selectedSection(), product)"
                          loading="lazy"
                          decoding="async"
                          (error)="markImageMissing(productImage)"
                        />
                      } @else {
                        <div
                          class="catalog-product-placeholder"
                          role="img"
                          [attr.aria-label]="'Sin imagen disponible para ' + product.name"
                        >
                          <span>{{ getInitials(product.name) }}</span>
                        </div>
                      }
                    </div>

                    <div class="catalog-product-content">
                      <h3>{{ product.name }}</h3>

                      @if (product.description; as description) {
                        <p>{{ description }}</p>
                      }

                      <span class="catalog-product-price">{{ formatPrice(product.price) }}</span>
                    </div>
                  </article>
                }
              </div>
            } @else {
              <div class="catalog-empty-state" role="status">
                <strong>No hay productos publicados en esta categoría.</strong>
                <span>Contacta al laboratorio para consultar disponibilidad.</span>
              </div>
            }
          </section>
        </div>
      </section>

      <section class="catalog-final-cta" aria-label="Contacto del laboratorio">
        <div class="catalog-final-cta-inner">
          <div data-animate="fade-up">
            <p class="eyebrow">Contacto</p>
            <h2>¿Necesitas confirmar un trabajo?</h2>
            <p>
              Comunícate con el laboratorio para validar indicaciones, tiempos y el precio vigente antes de enviar
              tu caso.
            </p>
          </div>
          <a class="catalog-final-cta-action" routerLink="/contacto" data-animate="fade-in">
            Contactar al laboratorio
          </a>
        </div>
      </section>
    </div>
  `,
  styleUrl: './catalog-page.component.scss'
})
export class CatalogPageComponent {
  readonly sections = signal<readonly CatalogSection[]>(catalogSections);
  readonly selectedSectionKey = signal(catalogSections[0]?.id ?? '');
  readonly catalogLoadState = signal<CatalogLoadState>('loading');
  readonly missingImageUrls = signal<ReadonlySet<string>>(new Set<string>());

  readonly totalProducts = computed(() =>
    this.sections().reduce((total, section) => total + section.products.length, 0)
  );

  readonly selectedSection = computed<CatalogSection>(() => {
    const sections = this.sections();
    const selectedKey = this.selectedSectionKey();

    return sections.find((section) => section.id === selectedKey) ?? sections[0] ?? catalogSections[0]!;
  });

  readonly currentProducts = computed(() => this.selectedSection().products);

  readonly catalogNoticeText = computed(() => {
    if (this.catalogLoadState() === 'loading') {
      return 'Actualizando catálogo...';
    }

    if (this.catalogLoadState() === 'fallback') {
      return 'Mostrando la versión de referencia del catálogo.';
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

  private readonly onLocationChange = () => {
    this.selectSectionFromLocation();
  };

  constructor() {
    this.selectSectionFromLocation();
    this.loadPublicCatalog();

    if (typeof window !== 'undefined') {
      window.addEventListener('hashchange', this.onLocationChange);
      window.addEventListener('popstate', this.onLocationChange);
    }

    this.destroyRef.onDestroy(() => {
      if (typeof window !== 'undefined') {
        window.removeEventListener('hashchange', this.onLocationChange);
        window.removeEventListener('popstate', this.onLocationChange);
      }
    });
  }

  formatPrice(price: number) {
    return this.priceFormatter.format(price);
  }

  productCountLabel(count: number) {
    return `${count} ${count === 1 ? 'producto' : 'productos'}`;
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

  getProductImageAlt(section: CatalogSection, product: CatalogProduct) {
    return product.altText ?? `${product.name} - ${section.name}`;
  }

  markImageMissing(imageUrl: string) {
    this.missingImageUrls.update((currentUrls) => new Set(currentUrls).add(imageUrl));
  }

  getInitials(text: string) {
    return text
      .split(/\s+/)
      .filter(Boolean)
      .slice(0, 2)
      .map((word) => word[0])
      .join('')
      .toUpperCase();
  }

  selectSectionByKey(sectionKey: string) {
    const section = this.sections().find((candidate) => candidate.id === sectionKey);

    if (!section) {
      return;
    }

    this.selectedSectionKey.set(section.id);

    if (this.getCurrentHashSectionKey() !== section.id) {
      this.updateSectionHash(section.id, 'push');
    }
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
    const currentKey = this.selectedSectionKey();
    const hashKey = this.getCurrentHashSectionKey();

    this.sections.set(sections);

    const nextSection =
      sections.find((section) => section.id === hashKey) ??
      sections.find((section) => section.id === currentKey) ??
      sections[0];

    if (!nextSection) {
      return;
    }

    this.selectedSectionKey.set(nextSection.id);

    if (hashKey !== nextSection.id) {
      this.updateSectionHash(nextSection.id, 'replace');
    }
  }

  private selectSectionFromLocation() {
    const sections = this.sections();
    const hashKey = this.getCurrentHashSectionKey();
    const hashSection = sections.find((section) => section.id === hashKey);

    if (hashSection) {
      this.selectedSectionKey.set(hashSection.id);
      return;
    }

    if (hashKey && this.catalogLoadState() === 'loading') {
      return;
    }

    const firstSection = sections[0];

    if (!firstSection) {
      return;
    }

    this.selectedSectionKey.set(firstSection.id);

    if (hashKey !== firstSection.id) {
      this.updateSectionHash(firstSection.id, 'replace');
    }
  }

  private updateSectionHash(sectionKey: string, historyMode: CatalogHistoryMode) {
    if (typeof window === 'undefined') {
      return;
    }

    const nextUrl = `${window.location.pathname}${window.location.search}#${encodeURIComponent(sectionKey)}`;

    if (historyMode === 'push') {
      window.history.pushState(window.history.state, '', nextUrl);
      return;
    }

    window.history.replaceState(window.history.state, '', nextUrl);
  }

  private getCurrentHashSectionKey() {
    if (typeof window === 'undefined') {
      return '';
    }

    const rawHash = window.location.hash.replace(/^#/, '');

    if (!rawHash) {
      return '';
    }

    try {
      return decodeURIComponent(rawHash);
    } catch {
      return rawHash;
    }
  }

  private isImageMissing(imageUrl: string) {
    return this.missingImageUrls().has(imageUrl);
  }
}
