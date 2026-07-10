import { Component, DestroyRef, ElementRef, ViewChild, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, timeout } from 'rxjs';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';
import { CatalogProduct, CatalogSection, catalogSections } from '../../data/catalog-data';
import { PublicCatalogService } from '../../services/public-catalog.service';

type CatalogPauseReason = 'focus' | 'hover' | 'manual';
type CatalogLoadState = 'api' | 'fallback' | 'loading';

type CatalogGalleryImage = {
  alt: string;
  url: string;
};

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
        <a class="login-action" routerLink="/contacto" data-animate="fade-in">Contactar</a>
      </section>

      <section
        class="catalog-explorer"
        aria-label="Explorador visual del catálogo"
        (mouseenter)="pauseAutoplay('hover')"
        (mouseleave)="resumeAutoplay('hover')"
        (focusin)="pauseAutoplay('focus')"
        (focusout)="resumeAutoplay('focus', $event)"
      >
        <div class="section-carousel" data-animate="fade-up">
          <div class="section-carousel-heading">
            <div>
              <p class="eyebrow">Secciones</p>
              <h2>Explora por material o servicio</h2>
            </div>
            <span class="autoplay-status">
              {{ prefersReducedMotion() ? 'Autoplay desactivado' : isSectionCarouselPaused() ? 'Autoplay pausado' : 'Autoplay activo' }}
            </span>
          </div>

          <div class="carousel-shell">
            <button class="carousel-arrow" type="button" aria-label="Ver sección anterior" (click)="previousSection()">
              <span aria-hidden="true">‹</span>
            </button>

            <nav class="carousel-viewport" aria-label="Secciones del catálogo">
              <div #sectionTrack class="section-track">
                @for (section of sections(); track section.id; let sectionIndex = $index) {
                  <button
                    class="section-card"
                    type="button"
                    [class.is-active]="sectionIndex === selectedSectionIndex()"
                    [attr.aria-label]="'Ver sección ' + section.name"
                    [attr.aria-current]="sectionIndex === selectedSectionIndex() ? 'true' : null"
                    (click)="selectSection(sectionIndex)"
                  >
                    <span class="section-card-media" aria-hidden="true">
                      @if (getSectionThumbnail(section); as thumbnailUrl) {
                        <img
                          [src]="thumbnailUrl"
                          [alt]="getSectionThumbnailAlt(section)"
                          [attr.loading]="sectionIndex === selectedSectionIndex() ? 'eager' : 'lazy'"
                          decoding="async"
                          (error)="markImageMissing(thumbnailUrl)"
                        />
                      } @else {
                        <span>{{ getInitials(section.name) }}</span>
                      }
                    </span>
                    <span class="section-card-copy">
                      <strong>{{ section.name }}</strong>
                      <span>{{ section.products.length }} productos</span>
                    </span>
                  </button>
                }
              </div>
            </nav>

            <button class="carousel-arrow" type="button" aria-label="Ver sección siguiente" (click)="nextSection()">
              <span aria-hidden="true">›</span>
            </button>
          </div>
        </div>

        <div class="image-gallery">
          <div class="gallery-heading">
            <div>
              <p class="eyebrow">Galería</p>
              <h2>{{ selectedSection().name }}</h2>
            </div>
            <span>{{ galleryStatusText() }}</span>
          </div>

          <div class="gallery-stage">
            @if (previousGalleryImage(); as image) {
              <button
                class="gallery-thumb gallery-thumb--previous"
                type="button"
                aria-label="Ver imagen anterior"
                (click)="previousImage()"
              >
                <img [src]="image.url" [alt]="image.alt" loading="lazy" decoding="async" (error)="markImageMissing(image.url)" />
              </button>
            }

            <div class="gallery-main">
              @if (currentImage(); as image) {
                <img
                  [src]="image.url"
                  [alt]="image.alt"
                  loading="eager"
                  decoding="async"
                  fetchpriority="high"
                  (error)="markImageMissing(image.url)"
                />
              } @else {
                <div
                  class="gallery-placeholder"
                  role="img"
                  [attr.aria-label]="'Sin imagen disponible para ' + selectedSection().name"
                >
                  <span>{{ getInitials(selectedSection().name) }}</span>
                </div>
              }
            </div>

            @if (nextGalleryImage(); as image) {
              <button
                class="gallery-thumb gallery-thumb--next"
                type="button"
                aria-label="Ver imagen siguiente"
                (click)="nextImage()"
              >
                <img [src]="image.url" [alt]="image.alt" loading="lazy" decoding="async" (error)="markImageMissing(image.url)" />
              </button>
            }
          </div>
        </div>
      </section>

      <section class="catalog-summary" aria-label="Resumen del catálogo">
        <div data-animate="stagger-card">
          <strong>{{ sections().length }}</strong>
          <span>secciones</span>
        </div>
        <div data-animate="stagger-card">
          <strong>{{ totalProducts() }}</strong>
          <span>productos</span>
        </div>
        <div data-animate="stagger-card">
          <strong>MXN</strong>
          <span>precios de referencia</span>
        </div>
      </section>

      <section class="catalog-content">
        <article class="catalog-section" [id]="selectedSection().id">
          <div class="section-heading">
            <p class="eyebrow">Productos de la sección</p>
            <h2>{{ selectedSection().name }}</h2>
            <p>{{ selectedSection().products.length }} productos con precios de referencia 2026.</p>
          </div>

          <div class="product-grid">
            @for (product of currentProducts(); track product.id) {
              <article class="product-card">
                <div class="image-frame">
                  @if (getProductImage(selectedSection(), product); as productImage) {
                    <img
                      [src]="productImage"
                      [alt]="getProductImageAlt(selectedSection(), product)"
                      loading="lazy"
                      decoding="async"
                      (error)="markImageMissing(productImage)"
                    />
                    @if (!product.imageUrl && selectedSection().imageUrl) {
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
    </div>
  `,
  styleUrl: './catalog-page.component.scss'
})
export class CatalogPageComponent {
  @ViewChild('sectionTrack') private sectionTrack?: ElementRef<HTMLElement>;

  readonly sections = signal<readonly CatalogSection[]>(catalogSections);
  readonly selectedSectionIndex = signal(0);
  readonly selectedImageIndex = signal(0);
  readonly catalogLoadState = signal<CatalogLoadState>('loading');
  readonly isSectionCarouselPaused = signal(false);
  readonly prefersReducedMotion = signal(false);
  readonly missingImageUrls = signal<ReadonlySet<string>>(new Set<string>());

  readonly totalProducts = computed(() =>
    this.sections().reduce((total, section) => total + section.products.length, 0)
  );
  readonly selectedSection = computed(() => this.sections()[this.selectedSectionIndex()] ?? this.sections()[0]);
  readonly currentProducts = computed(() => this.selectedSection().products);
  readonly sectionImages = computed(() => this.getSectionImages(this.selectedSection()));
  readonly currentImage = computed(() => this.sectionImages()[this.selectedImageIndex()] ?? null);
  readonly previousGalleryImage = computed(() => {
    const index = this.selectedImageIndex();

    return index > 0 ? this.sectionImages()[index - 1] : null;
  });
  readonly nextGalleryImage = computed(() => {
    const images = this.sectionImages();
    const index = this.selectedImageIndex();

    return index < images.length - 1 ? images[index + 1] : null;
  });
  readonly galleryStatusText = computed(() => {
    const images = this.sectionImages();

    if (!images.length) {
      return 'Sin imagen disponible';
    }

    return `${this.selectedImageIndex() + 1} de ${images.length}`;
  });
  readonly catalogNoticeText = computed(() => {
    if (this.catalogLoadState() === 'loading') {
      return 'Actualizando catálogo...';
    }

    if (this.catalogLoadState() === 'fallback') {
      return 'Mostrando catálogo de referencia disponible.';
    }

    return '';
  });

  private readonly destroyRef = inject(DestroyRef);
  private readonly publicCatalogService = inject(PublicCatalogService);
  private readonly pauseReasons = new Set<CatalogPauseReason>();
  private readonly priceFormatter = new Intl.NumberFormat('es-MX', {
    currency: 'MXN',
    maximumFractionDigits: 2,
    minimumFractionDigits: 2,
    style: 'currency'
  });
  private autoplayTimerId: number | undefined;
  private manualPauseTimerId: number | undefined;
  private motionPreferenceQuery: MediaQueryList | undefined;

  private readonly onMotionPreferenceChange = (event: MediaQueryListEvent) => {
    this.setReducedMotionPreference(event.matches);
  };

  private readonly onHashChange = () => {
    this.selectSectionFromHash();
  };

  constructor() {
    this.setupMotionPreference();
    this.selectSectionFromHash();
    this.startAutoplay();
    this.loadPublicCatalog();

    if (typeof window !== 'undefined') {
      window.addEventListener('hashchange', this.onHashChange);
    }

    this.destroyRef.onDestroy(() => {
      this.stopAutoplay();
      this.clearManualPauseTimer();
      this.motionPreferenceQuery?.removeEventListener('change', this.onMotionPreferenceChange);

      if (typeof window !== 'undefined') {
        window.removeEventListener('hashchange', this.onHashChange);
      }
    });
  }

  formatPrice(price: number) {
    return this.priceFormatter.format(price);
  }

  getProductImage(section: CatalogSection, product: CatalogProduct) {
    if (product.imageUrl && !this.isImageMissing(product.imageUrl)) {
      return product.imageUrl;
    }

    if (section.imageUrl && !this.isImageMissing(section.imageUrl)) {
      return section.imageUrl;
    }

    return '';
  }

  getSectionThumbnail(section: CatalogSection) {
    if (section.imageUrl && !this.isImageMissing(section.imageUrl)) {
      return section.imageUrl;
    }

    return section.products.find((product) => product.imageUrl && !this.isImageMissing(product.imageUrl))?.imageUrl ?? '';
  }

  getSectionThumbnailAlt(section: CatalogSection) {
    if (section.imageUrl && !this.isImageMissing(section.imageUrl)) {
      return section.altText ?? section.name;
    }

    const productWithImage = section.products.find((product) => product.imageUrl && !this.isImageMissing(product.imageUrl));

    return productWithImage?.altText ?? productWithImage?.name ?? section.name;
  }

  getProductImageAlt(section: CatalogSection, product: CatalogProduct) {
    if (product.imageUrl && !this.isImageMissing(product.imageUrl)) {
      return product.altText ?? `${product.name} - ${section.name}`;
    }

    return section.altText ?? `${product.name} - ${section.name}`;
  }

  markImageMissing(imageUrl: string) {
    this.missingImageUrls.update((currentUrls) => new Set(currentUrls).add(imageUrl));
    this.keepSelectedImageInRange();
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

  selectSection(index: number) {
    this.noteManualInteraction();
    this.setSelectedSection(index);
  }

  nextSection(options: { autoplay?: boolean } = {}) {
    if (!options.autoplay) {
      this.noteManualInteraction();
    }

    const sectionCount = this.sections().length;

    if (sectionCount < 2) {
      return;
    }

    this.setSelectedSection((this.selectedSectionIndex() + 1) % sectionCount);
  }

  previousSection() {
    this.noteManualInteraction();

    const sectionCount = this.sections().length;

    if (sectionCount < 2) {
      return;
    }

    this.setSelectedSection((this.selectedSectionIndex() - 1 + sectionCount) % sectionCount);
  }

  nextImage() {
    this.selectImage(this.selectedImageIndex() + 1);
  }

  previousImage() {
    this.selectImage(this.selectedImageIndex() - 1);
  }

  selectImage(index: number) {
    this.noteManualInteraction();

    const images = this.sectionImages();

    if (!images.length) {
      this.selectedImageIndex.set(0);
      return;
    }

    this.selectedImageIndex.set(Math.max(0, Math.min(index, images.length - 1)));
  }

  pauseAutoplay(reason: CatalogPauseReason = 'manual') {
    this.pauseReasons.add(reason);
    this.isSectionCarouselPaused.set(true);
  }

  resumeAutoplay(reason: CatalogPauseReason = 'manual', event?: FocusEvent) {
    if (reason === 'focus' && this.isFocusStillInside(event)) {
      return;
    }

    this.pauseReasons.delete(reason);
    this.isSectionCarouselPaused.set(this.prefersReducedMotion() || this.pauseReasons.size > 0);
  }

  private setupMotionPreference() {
    if (typeof window === 'undefined') {
      return;
    }

    this.motionPreferenceQuery = window.matchMedia('(prefers-reduced-motion: reduce)');
    this.motionPreferenceQuery.addEventListener('change', this.onMotionPreferenceChange);
    this.setReducedMotionPreference(this.motionPreferenceQuery.matches);
  }

  private setReducedMotionPreference(prefersReducedMotion: boolean) {
    this.prefersReducedMotion.set(prefersReducedMotion);

    if (prefersReducedMotion) {
      this.isSectionCarouselPaused.set(true);
      this.stopAutoplay();
      return;
    }

    this.isSectionCarouselPaused.set(this.pauseReasons.size > 0);
    this.startAutoplay();
  }

  private startAutoplay() {
    this.stopAutoplay();

    if (typeof window === 'undefined' || this.prefersReducedMotion()) {
      return;
    }

    this.autoplayTimerId = window.setInterval(() => {
      if (this.isSectionCarouselPaused() || this.prefersReducedMotion()) {
        return;
      }

      this.nextSection({ autoplay: true });
    }, 4000);
  }

  private stopAutoplay() {
    if (typeof window === 'undefined' || this.autoplayTimerId === undefined) {
      return;
    }

    window.clearInterval(this.autoplayTimerId);
    this.autoplayTimerId = undefined;
  }

  private setSelectedSection(index: number) {
    const sections = this.sections();

    if (!sections.length) {
      this.selectedSectionIndex.set(0);
      this.selectedImageIndex.set(0);
      return;
    }

    const nextIndex = Math.max(0, Math.min(index, sections.length - 1));
    this.selectedSectionIndex.set(nextIndex);
    this.selectedImageIndex.set(0);
    this.scrollSelectedSectionIntoView();
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
    const selectedSectionId = this.selectedSection().id;
    const currentIndex = this.selectedSectionIndex();

    this.sections.set(sections);

    const hashIndex = this.getHashSectionIndex(sections);
    const selectedIdIndex = sections.findIndex((section) => section.id === selectedSectionId);
    const nextIndex =
      hashIndex >= 0
        ? hashIndex
        : selectedIdIndex >= 0
          ? selectedIdIndex
          : Math.min(currentIndex, sections.length - 1);

    this.setSelectedSection(Math.max(0, nextIndex));
    this.keepSelectedImageInRange();
  }

  private selectSectionFromHash() {
    if (typeof window === 'undefined') {
      return;
    }

    const sectionId = window.location.hash.replace('#', '');

    const sectionIndex = this.getHashSectionIndex(this.sections(), sectionId);

    if (sectionIndex >= 0) {
      this.setSelectedSection(sectionIndex);
    }
  }

  private getHashSectionIndex(sections: readonly CatalogSection[], sectionId = this.getCurrentHashSectionId()) {
    if (!sectionId) {
      return -1;
    }

    return sections.findIndex((section) => section.id === sectionId);
  }

  private getCurrentHashSectionId() {
    if (typeof window === 'undefined') {
      return '';
    }

    return window.location.hash.replace('#', '');
  }

  private getSectionImages(section: CatalogSection): CatalogGalleryImage[] {
    const images: CatalogGalleryImage[] = [];
    const imageUrls = new Set<string>();

    this.addSectionImage(images, imageUrls, section.imageUrl, section.altText ?? `Imagen representativa de ${section.name}`);

    for (const product of section.products) {
      this.addSectionImage(images, imageUrls, product.imageUrl, product.altText ?? `${product.name} - ${section.name}`);
    }

    return images;
  }

  private addSectionImage(
    images: CatalogGalleryImage[],
    imageUrls: Set<string>,
    imageUrl: string | undefined,
    alt: string
  ) {
    if (!imageUrl || imageUrls.has(imageUrl) || this.isImageMissing(imageUrl)) {
      return;
    }

    images.push({ alt, url: imageUrl });
    imageUrls.add(imageUrl);
  }

  private isImageMissing(imageUrl: string) {
    return this.missingImageUrls().has(imageUrl);
  }

  private keepSelectedImageInRange() {
    const images = this.sectionImages();

    if (!images.length) {
      this.selectedImageIndex.set(0);
      return;
    }

    if (this.selectedImageIndex() > images.length - 1) {
      this.selectedImageIndex.set(images.length - 1);
    }
  }

  private noteManualInteraction() {
    this.pauseAutoplay('manual');
    this.clearManualPauseTimer();

    if (typeof window === 'undefined') {
      return;
    }

    this.manualPauseTimerId = window.setTimeout(() => {
      this.manualPauseTimerId = undefined;
      this.resumeAutoplay('manual');
    }, 8000);
  }

  private clearManualPauseTimer() {
    if (typeof window === 'undefined' || this.manualPauseTimerId === undefined) {
      return;
    }

    window.clearTimeout(this.manualPauseTimerId);
    this.manualPauseTimerId = undefined;
  }

  private scrollSelectedSectionIntoView() {
    const selectedCard = this.getSelectedSectionCard();

    selectedCard?.scrollIntoView({
      behavior: this.prefersReducedMotion() ? 'auto' : 'smooth',
      block: 'nearest',
      inline: 'center'
    });
  }

  private getSelectedSectionCard() {
    return this.sectionTrack?.nativeElement.querySelector<HTMLButtonElement>(
      `.section-card:nth-of-type(${this.selectedSectionIndex() + 1})`
    );
  }

  private isFocusStillInside(event: FocusEvent | undefined) {
    const currentTarget = event?.currentTarget;
    const nextTarget = event?.relatedTarget;

    return currentTarget instanceof HTMLElement && nextTarget instanceof HTMLElement && currentTarget.contains(nextTarget);
  }
}
