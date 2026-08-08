import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, ElementRef, OnDestroy, OnInit, ViewChild, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { AdminCatalogService } from '../admin-catalog.service';
import {
  CATALOG_IMAGE_OPTIONS,
  isAllowedCatalogImagePath,
  isServerGeneratedCatalogImagePath
} from '../catalog-image-options';
import {
  CatalogProduct,
  CatalogProductUpsertRequest,
  CatalogSection,
  CatalogSectionUpsertRequest
} from '../catalog.models';

type CatalogStatusFilter = 'all' | 'active' | 'inactive';

const MAX_CATALOG_IMAGE_BYTES = 2_097_152;
const CATALOG_IMAGE_MIME_BY_EXTENSION: Readonly<Record<string, string>> = {
  webp: 'image/webp',
  jpg: 'image/jpeg',
  jpeg: 'image/jpeg',
  png: 'image/png'
};

interface CatalogSectionForm {
  key: string;
  name: string;
  description: string;
  imagePath: string;
  altText: string;
  sortOrder: number;
  isActive: boolean;
}

interface CatalogProductForm {
  catalogSectionId: string;
  key: string;
  name: string;
  description: string;
  priceAmount: number | null;
  currency: string;
  imagePath: string;
  altText: string;
  sortOrder: number;
  isActive: boolean;
}

@Component({
  selector: 'app-admin-catalog-page',
  imports: [CurrencyPipe, FormsModule],
  template: `
    <section class="feature-page catalog-admin-page">
      <header class="page-header">
        <div>
          <h1>Catalogo</h1>
          <p>Administracion privada de secciones, productos, precios e imagenes existentes.</p>
        </div>
        <div class="page-actions">
          @if (!canManage) {
            <span class="readonly-note">Solo lectura</span>
          }
          <button class="secondary-button" type="button" [disabled]="isLoading()" (click)="load()">
            {{ isLoading() ? 'Actualizando...' : 'Actualizar' }}
          </button>
        </div>
      </header>

      <section class="catalog-summary-grid" aria-label="Resumen de catalogo">
        <div class="catalog-summary-card">
          <strong>{{ sections().length }}</strong>
          <span>Secciones</span>
        </div>
        <div class="catalog-summary-card">
          <strong>{{ activeSectionsCount() }}</strong>
          <span>Secciones activas</span>
        </div>
        <div class="catalog-summary-card">
          <strong>{{ products().length }}</strong>
          <span>Productos</span>
        </div>
        <div class="catalog-summary-card">
          <strong>{{ activeProductsCount() }}</strong>
          <span>Productos activos</span>
        </div>
      </section>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (isLoading() && sections().length === 0 && products().length === 0) {
        <p class="loading-state">Cargando catalogo...</p>
      }

      <section class="catalog-admin-grid">
        <section class="catalog-admin-column">
          <div class="section-header">
            <div>
              <h2>Secciones</h2>
              <p>Orden, estado e imagen representativa.</p>
            </div>
            @if (canManage) {
              <button class="primary-button" type="button" (click)="startCreateSection()">
                Nueva seccion
              </button>
            }
          </div>

          @if (showSectionForm()) {
            <form class="admin-panel catalog-editor-form" novalidate (ngSubmit)="saveSection()">
              <header>
                <h2>{{ editingSectionId() ? 'Editar seccion' : 'Nueva seccion' }}</h2>
              </header>

              <div class="field-grid">
                <label class="form-field">
                  <span>Nombre</span>
                  <input
                    name="sectionName"
                    type="text"
                    required
                    [ngModel]="sectionForm.name"
                    (ngModelChange)="updateSectionName($event)"
                  />
                </label>
                <label class="form-field">
                  <span>Clave</span>
                  <input name="sectionKey" type="text" required [(ngModel)]="sectionForm.key" />
                </label>
                <label class="form-field">
                  <span>Orden</span>
                  <input name="sectionSortOrder" type="number" step="1" [(ngModel)]="sectionForm.sortOrder" />
                </label>
                <label class="check-field catalog-check-field">
                  <input name="sectionIsActive" type="checkbox" [(ngModel)]="sectionForm.isActive" />
                  <span>Activa</span>
                </label>
                <label class="form-field full-field">
                  <span>Descripcion</span>
                  <textarea name="sectionDescription" rows="3" [(ngModel)]="sectionForm.description"></textarea>
                </label>
                <label class="form-field">
                  <span>Imagen</span>
                  <select name="sectionImagePath" [(ngModel)]="sectionForm.imagePath">
                    <option value="">Sin imagen</option>
                    @for (option of imageOptions; track option.path) {
                      <option [value]="option.path">{{ option.label }}</option>
                    }
                  </select>
                </label>
                <label class="form-field">
                  <span>Texto alternativo</span>
                  <input name="sectionAltText" type="text" [(ngModel)]="sectionForm.altText" />
                </label>
              </div>

              @if (sectionForm.imagePath) {
                <figure class="catalog-image-preview">
                  <img [src]="previewSrc(sectionForm.imagePath)" [alt]="sectionForm.altText || sectionForm.name" />
                  <figcaption>{{ sectionForm.imagePath }}</figcaption>
                </figure>
                @if (imageNote(sectionForm.imagePath); as note) {
                  <p class="catalog-image-note">{{ note }}</p>
                }
              }

              <div class="page-actions">
                <button class="primary-button" type="submit" [disabled]="isSaving()">
                  {{ isSaving() ? 'Guardando...' : 'Guardar seccion' }}
                </button>
                <button class="ghost-button" type="button" [disabled]="isSaving()" (click)="cancelSectionForm()">
                  Cancelar
                </button>
              </div>
            </form>
          }

          @if (!isLoading() && sections().length === 0) {
            <p class="empty-state">No hay secciones registradas.</p>
          } @else if (sections().length > 0) {
            <div class="table-scroll catalog-sections-table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Seccion</th>
                    <th>Estado</th>
                    <th>Orden</th>
                    <th>Imagen</th>
                    @if (canManage) {
                      <th>Acciones</th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (section of sections(); track section.id) {
                    <tr>
                      <td>
                        <strong>{{ section.name }}</strong>
                        <small class="muted-block">{{ section.key }}</small>
                      </td>
                      <td>
                        <span class="status-pill" [class.active]="section.isActive" [class.inactive]="!section.isActive">
                          {{ section.isActive ? 'Activo' : 'Inactivo' }}
                        </span>
                      </td>
                      <td>{{ section.sortOrder }}</td>
                      <td>
                        @if (section.imagePath) {
                          <img class="catalog-thumb" [src]="previewSrc(section.imagePath)" [alt]="section.altText || section.name" />
                        } @else {
                          <span class="catalog-image-empty">Sin imagen</span>
                        }
                      </td>
                      @if (canManage) {
                        <td>
                          <div class="page-actions">
                            <button class="secondary-button" type="button" (click)="startEditSection(section)">
                              Editar
                            </button>
                            <button
                              type="button"
                              [class.danger-button]="section.isActive"
                              [class.secondary-button]="!section.isActive"
                              [disabled]="isSaving()"
                              (click)="toggleSectionStatus(section)"
                            >
                              {{ section.isActive ? 'Desactivar' : 'Activar' }}
                            </button>
                          </div>
                        </td>
                      }
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <div class="admin-mobile-list catalog-section-mobile-list">
              @for (section of sections(); track section.id) {
                <article class="admin-card catalog-admin-card">
                  <header>
                    <div>
                      <strong>{{ section.name }}</strong>
                      <span>{{ section.key }}</span>
                    </div>
                    <span class="status-pill" [class.active]="section.isActive" [class.inactive]="!section.isActive">
                      {{ section.isActive ? 'Activo' : 'Inactivo' }}
                    </span>
                  </header>
                  @if (section.imagePath) {
                    <img class="catalog-card-image" [src]="previewSrc(section.imagePath)" [alt]="section.altText || section.name" />
                  }
                  <dl>
                    <div>
                      <dt>Orden</dt>
                      <dd>{{ section.sortOrder }}</dd>
                    </div>
                    <div>
                      <dt>Descripcion</dt>
                      <dd>{{ section.description || '-' }}</dd>
                    </div>
                    <div>
                      <dt>Imagen</dt>
                      <dd>{{ section.imagePath || '-' }}</dd>
                    </div>
                  </dl>
                  @if (canManage) {
                    <div class="page-actions">
                      <button class="secondary-button" type="button" (click)="startEditSection(section)">Editar</button>
                      <button
                        type="button"
                        [class.danger-button]="section.isActive"
                        [class.secondary-button]="!section.isActive"
                        [disabled]="isSaving()"
                        (click)="toggleSectionStatus(section)"
                      >
                        {{ section.isActive ? 'Desactivar' : 'Activar' }}
                      </button>
                    </div>
                  }
                </article>
              }
            </div>
          }
        </section>

        <section class="catalog-admin-column">
          <div class="section-header">
            <div>
              <h2>Productos</h2>
              <p>Precio MXN, estado, seccion e imagen del producto.</p>
            </div>
            @if (canManage) {
              <button class="primary-button" type="button" [disabled]="sections().length === 0" (click)="startCreateProduct()">
                Nuevo producto
              </button>
            }
          </div>

          <form class="toolbar catalog-products-toolbar" (ngSubmit)="applyProductFilters()">
            <label class="filter-field">
              <span>Seccion</span>
              <select
                name="productSectionFilter"
                [ngModel]="selectedProductSectionId()"
                (ngModelChange)="selectedProductSectionId.set($event)"
              >
                <option value="">Todas</option>
                @for (section of sections(); track section.id) {
                  <option [value]="section.id">{{ section.name }}</option>
                }
              </select>
            </label>
            <label class="filter-field">
              <span>Estado</span>
              <select
                name="productStatusFilter"
                [ngModel]="productStatusFilter()"
                (ngModelChange)="productStatusFilter.set($event)"
              >
                <option value="all">Todos</option>
                <option value="active">Activos</option>
                <option value="inactive">Inactivos</option>
              </select>
            </label>
            <button class="secondary-button" type="submit">Filtrar</button>
          </form>

          @if (showProductForm()) {
            <form class="admin-panel catalog-editor-form" novalidate (ngSubmit)="saveProduct()">
              <header>
                <h2>{{ editingProductId() ? 'Editar producto' : 'Nuevo producto' }}</h2>
              </header>

              <div class="field-grid">
                <label class="form-field">
                  <span>Seccion</span>
                  <select name="productSectionId" required [(ngModel)]="productForm.catalogSectionId">
                    <option value="">Selecciona seccion</option>
                    @for (section of sections(); track section.id) {
                      <option [value]="section.id">{{ section.name }}</option>
                    }
                  </select>
                </label>
                <label class="form-field">
                  <span>Nombre</span>
                  <input
                    name="productName"
                    type="text"
                    required
                    [ngModel]="productForm.name"
                    (ngModelChange)="updateProductName($event)"
                  />
                </label>
                <label class="form-field">
                  <span>Clave</span>
                  <input name="productKey" type="text" required [(ngModel)]="productForm.key" />
                </label>
                <label class="form-field">
                  <span>Precio</span>
                  <input name="productPrice" type="number" min="0" step="0.01" required [(ngModel)]="productForm.priceAmount" />
                </label>
                <label class="form-field">
                  <span>Moneda</span>
                  <input name="productCurrency" type="text" readonly [(ngModel)]="productForm.currency" />
                </label>
                <label class="form-field">
                  <span>Orden</span>
                  <input name="productSortOrder" type="number" step="1" [(ngModel)]="productForm.sortOrder" />
                </label>
                <label class="check-field catalog-check-field">
                  <input name="productIsActive" type="checkbox" [(ngModel)]="productForm.isActive" />
                  <span>Activo</span>
                </label>
                <label class="form-field full-field">
                  <span>Descripcion</span>
                  <textarea name="productDescription" rows="3" [(ngModel)]="productForm.description"></textarea>
                </label>
                <label class="form-field">
                  <span>Texto alternativo</span>
                  <input name="productAltText" type="text" [(ngModel)]="productForm.altText" />
                </label>
              </div>

              <section class="catalog-product-image-editor" aria-labelledby="product-image-editor-title">
                <header>
                  <div>
                    <h3 id="product-image-editor-title">Imagen del producto</h3>
                    <p>{{ productImageOrigin(productForm.imagePath) }}</p>
                  </div>
                </header>

                <label class="form-field">
                  <span>Imagen existente del catálogo</span>
                  <select
                    name="productImagePath"
                    [ngModel]="productForm.imagePath"
                    (ngModelChange)="onProductAssetImageChange($event)"
                  >
                    <option value="">Sin imagen</option>
                    @if (isServerGeneratedImagePath(productForm.imagePath)) {
                      <option [value]="productForm.imagePath">Imagen cargada actual</option>
                    }
                    @for (option of imageOptions; track option.path) {
                      <option [value]="option.path">{{ option.label }}</option>
                    }
                  </select>
                </label>

                @if (localProductImagePreviewUrl() || productForm.imagePath) {
                  <figure class="catalog-image-preview catalog-product-image-preview">
                    <img
                      [src]="localProductImagePreviewUrl() || previewSrc(productForm.imagePath)"
                      [alt]="productForm.altText || productForm.name || 'Vista previa del producto'"
                    />
                    <figcaption>
                      {{ localProductImagePreviewUrl() ? 'Vista previa del archivo seleccionado' : productImageOrigin(productForm.imagePath) }}
                    </figcaption>
                  </figure>
                } @else {
                  <p class="catalog-image-empty">Sin imagen</p>
                }

                @if (imageNote(productForm.imagePath); as note) {
                  <p class="catalog-image-note">{{ note }}</p>
                }

                @if (canManage) {
                  @if (editingProductId()) {
                    <div class="catalog-image-upload-controls">
                      <label class="form-field">
                        <span>Archivo nuevo</span>
                        <input
                          #productImageInput
                          type="file"
                          accept=".webp,.jpg,.jpeg,.png,image/webp,image/jpeg,image/png"
                          [disabled]="isImageBusy() || isSaving()"
                          (change)="onProductImageFileSelected($event)"
                        />
                        <small>WebP, JPG o PNG. Máximo 2 MB.</small>
                      </label>

                      <div class="page-actions catalog-image-actions">
                        <button
                          class="primary-button"
                          type="button"
                          [disabled]="isImageBusy() || isSaving()"
                          (click)="uploadSelectedProductImage()"
                        >
                          @if (isUploadingImage()) {
                            Subiendo imagen...
                          } @else {
                            {{ productForm.imagePath ? 'Reemplazar imagen' : 'Subir imagen' }}
                          }
                        </button>
                        @if (productForm.imagePath) {
                          <button
                            class="danger-button"
                            type="button"
                            [disabled]="isImageBusy() || isSaving()"
                            (click)="clearCurrentProductImage()"
                          >
                            {{ isClearingImage() ? 'Quitando imagen...' : 'Quitar imagen' }}
                          </button>
                        }
                      </div>
                    </div>
                  } @else {
                    <p class="catalog-image-guidance">Guarda el producto antes de subir una imagen personalizada.</p>
                  }
                }
              </section>

              <div class="page-actions">
                <button class="primary-button" type="submit" [disabled]="isSaving() || isImageBusy()">
                  {{ isSaving() ? 'Guardando...' : 'Guardar producto' }}
                </button>
                <button class="ghost-button" type="button" [disabled]="isSaving() || isImageBusy()" (click)="cancelProductForm()">
                  Cancelar
                </button>
              </div>
            </form>
          }

          @if (!isLoading() && filteredProducts().length === 0) {
            <p class="empty-state">No hay productos con los filtros actuales.</p>
          } @else if (filteredProducts().length > 0) {
            <div class="table-scroll catalog-products-table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Producto</th>
                    <th>Seccion</th>
                    <th>Precio</th>
                    <th>Estado</th>
                    <th>Imagen</th>
                    @if (canManage) {
                      <th>Acciones</th>
                    }
                  </tr>
                </thead>
                <tbody>
                  @for (product of filteredProducts(); track product.id) {
                    <tr>
                      <td>
                        <strong>{{ product.name }}</strong>
                        <small class="muted-block">{{ product.key }}</small>
                      </td>
                      <td>{{ product.catalogSectionName }}</td>
                      <td>
                        @if (priceProductId() === product.id) {
                          <form class="catalog-price-form" novalidate (ngSubmit)="saveProductPrice(product)">
                            <input
                              name="priceAmount-{{ product.id }}"
                              type="number"
                              min="0"
                              step="0.01"
                              required
                              [(ngModel)]="priceAmount"
                            />
                            <button class="primary-button" type="submit" [disabled]="isSaving()">Guardar</button>
                            <button class="ghost-button" type="button" [disabled]="isSaving()" (click)="cancelPriceEdit()">
                              Cancelar
                            </button>
                          </form>
                        } @else {
                          <strong>{{ product.priceAmount | currency: product.currency:'symbol-narrow' }}</strong>
                        }
                      </td>
                      <td>
                        <span class="status-pill" [class.active]="product.isActive" [class.inactive]="!product.isActive">
                          {{ product.isActive ? 'Activo' : 'Inactivo' }}
                        </span>
                      </td>
                      <td>
                        @if (product.imagePath) {
                          <img class="catalog-thumb" [src]="previewSrc(product.imagePath)" [alt]="product.altText || product.name" />
                        } @else {
                          <span class="catalog-image-empty">Sin imagen</span>
                        }
                      </td>
                      @if (canManage) {
                        <td>
                          <div class="page-actions">
                            <button class="secondary-button" type="button" (click)="startEditProduct(product)">
                              Editar
                            </button>
                            <button class="ghost-button" type="button" (click)="startPriceEdit(product)">
                              Precio
                            </button>
                            <button
                              type="button"
                              [class.danger-button]="product.isActive"
                              [class.secondary-button]="!product.isActive"
                              [disabled]="isSaving()"
                              (click)="toggleProductStatus(product)"
                            >
                              {{ product.isActive ? 'Desactivar' : 'Activar' }}
                            </button>
                          </div>
                        </td>
                      }
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <div class="admin-mobile-list catalog-product-mobile-list">
              @for (product of filteredProducts(); track product.id) {
                <article class="admin-card catalog-admin-card">
                  <header>
                    <div>
                      <strong>{{ product.name }}</strong>
                      <span>{{ product.catalogSectionName }}</span>
                    </div>
                    <span class="status-pill" [class.active]="product.isActive" [class.inactive]="!product.isActive">
                      {{ product.isActive ? 'Activo' : 'Inactivo' }}
                    </span>
                  </header>
                  @if (product.imagePath) {
                    <img class="catalog-card-image" [src]="previewSrc(product.imagePath)" [alt]="product.altText || product.name" />
                  }
                  <dl>
                    <div>
                      <dt>Clave</dt>
                      <dd>{{ product.key }}</dd>
                    </div>
                    <div>
                      <dt>Precio</dt>
                      <dd>{{ product.priceAmount | currency: product.currency:'symbol-narrow' }}</dd>
                    </div>
                    <div>
                      <dt>Orden</dt>
                      <dd>{{ product.sortOrder }}</dd>
                    </div>
                    <div>
                      <dt>Imagen</dt>
                      <dd>{{ product.imagePath || '-' }}</dd>
                    </div>
                  </dl>

                  @if (priceProductId() === product.id) {
                    <form class="catalog-price-form" novalidate (ngSubmit)="saveProductPrice(product)">
                      <input
                        name="mobilePriceAmount-{{ product.id }}"
                        type="number"
                        min="0"
                        step="0.01"
                        required
                        [(ngModel)]="priceAmount"
                      />
                      <button class="primary-button" type="submit" [disabled]="isSaving()">Guardar precio</button>
                      <button class="ghost-button" type="button" [disabled]="isSaving()" (click)="cancelPriceEdit()">
                        Cancelar
                      </button>
                    </form>
                  }

                  @if (canManage) {
                    <div class="page-actions">
                      <button class="secondary-button" type="button" (click)="startEditProduct(product)">Editar</button>
                      <button class="ghost-button" type="button" (click)="startPriceEdit(product)">Precio</button>
                      <button
                        type="button"
                        [class.danger-button]="product.isActive"
                        [class.secondary-button]="!product.isActive"
                        [disabled]="isSaving()"
                        (click)="toggleProductStatus(product)"
                      >
                        {{ product.isActive ? 'Desactivar' : 'Activar' }}
                      </button>
                    </div>
                  }
                </article>
              }
            </div>
          }
        </section>
      </section>
    </section>
  `
})
export class AdminCatalogPageComponent implements OnInit, OnDestroy {
  @ViewChild('productImageInput') private productImageInput?: ElementRef<HTMLInputElement>;

  readonly imageOptions = CATALOG_IMAGE_OPTIONS;
  readonly sections = signal<CatalogSection[]>([]);
  readonly products = signal<CatalogProduct[]>([]);
  readonly selectedProductSectionId = signal('');
  readonly productStatusFilter = signal<CatalogStatusFilter>('all');
  readonly showSectionForm = signal(false);
  readonly showProductForm = signal(false);
  readonly editingSectionId = signal<string | null>(null);
  readonly editingProductId = signal<string | null>(null);
  readonly priceProductId = signal<string | null>(null);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isUploadingImage = signal(false);
  readonly isClearingImage = signal(false);
  readonly selectedProductImageFile = signal<File | null>(null);
  readonly localProductImagePreviewUrl = signal<string | null>(null);
  readonly isImageBusy = computed(() => this.isUploadingImage() || this.isClearingImage());
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly activeSectionsCount = computed(() => this.sections().filter((section) => section.isActive).length);
  readonly activeProductsCount = computed(() => this.products().filter((product) => product.isActive).length);
  readonly filteredProducts = computed(() => {
    const sectionId = this.selectedProductSectionId();
    const statusFilter = this.productStatusFilter();

    return this.products().filter((product) => {
      const matchesSection = !sectionId || product.catalogSectionId === sectionId;
      const matchesStatus =
        statusFilter === 'all'
        || (statusFilter === 'active' && product.isActive)
        || (statusFilter === 'inactive' && !product.isActive);

      return matchesSection && matchesStatus;
    });
  });

  sectionForm: CatalogSectionForm = this.emptySectionForm();
  productForm: CatalogProductForm = this.emptyProductForm();
  priceAmount: number | null = null;

  constructor(
    private readonly adminCatalogService: AdminCatalogService,
    private readonly authService: AuthService
  ) {}

  get canManage(): boolean {
    return this.authService.hasPermission('catalog.manage');
  }

  ngOnInit(): void {
    this.load();
  }

  ngOnDestroy(): void {
    this.resetProductImageSelection(false);
  }

  load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      sections: this.adminCatalogService.getSections(),
      products: this.adminCatalogService.getProducts()
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ sections, products }) => {
          this.sections.set(sections);
          this.products.set(products);

          if (this.selectedProductSectionId() && !sections.some((section) => section.id === this.selectedProductSectionId())) {
            this.selectedProductSectionId.set('');
          }
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  applyProductFilters(): void {
    this.errorMessage.set(null);
  }

  startCreateSection(): void {
    if (!this.canManage) {
      return;
    }

    this.sectionForm = this.emptySectionForm(this.nextSectionSortOrder());
    this.editingSectionId.set(null);
    this.showSectionForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  startEditSection(section: CatalogSection): void {
    if (!this.canManage) {
      return;
    }

    this.sectionForm = {
      key: section.key,
      name: section.name,
      description: section.description ?? '',
      imagePath: section.imagePath ?? '',
      altText: section.altText ?? '',
      sortOrder: section.sortOrder,
      isActive: section.isActive
    };
    this.editingSectionId.set(section.id);
    this.showSectionForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  cancelSectionForm(): void {
    this.showSectionForm.set(false);
    this.editingSectionId.set(null);
    this.sectionForm = this.emptySectionForm();
  }

  saveSection(): void {
    if (!this.canManage) {
      return;
    }

    const request = this.buildSectionRequest();

    if (!request) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const save$ = this.editingSectionId()
      ? this.adminCatalogService.updateSection(this.editingSectionId()!, request)
      : this.adminCatalogService.createSection(request);

    save$
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set(this.editingSectionId() ? 'Seccion actualizada.' : 'Seccion creada.');
          this.cancelSectionForm();
          this.load();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  toggleSectionStatus(section: CatalogSection): void {
    if (!this.canManage) {
      return;
    }

    const nextState = !section.isActive;

    if (!nextState && !window.confirm(`Desactivar la seccion ${section.name}?`)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminCatalogService
      .setSectionStatus(section.id, { isActive: nextState })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set(nextState ? 'Seccion activada.' : 'Seccion desactivada.');
          this.load();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  startCreateProduct(): void {
    if (!this.canManage) {
      return;
    }

    const sectionId = this.selectedProductSectionId() || this.sections()[0]?.id || '';

    if (!sectionId) {
      this.errorMessage.set('Crea una seccion antes de agregar productos.');
      return;
    }

    this.resetProductImageSelection();
    this.productForm = this.emptyProductForm(sectionId, this.nextProductSortOrder(sectionId));
    this.editingProductId.set(null);
    this.showProductForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  startEditProduct(product: CatalogProduct): void {
    if (!this.canManage) {
      return;
    }

    this.resetProductImageSelection();
    this.productForm = {
      catalogSectionId: product.catalogSectionId,
      key: product.key,
      name: product.name,
      description: product.description ?? '',
      priceAmount: product.priceAmount,
      currency: product.currency || 'MXN',
      imagePath: product.imagePath ?? '',
      altText: product.altText ?? '',
      sortOrder: product.sortOrder,
      isActive: product.isActive
    };
    this.editingProductId.set(product.id);
    this.showProductForm.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  cancelProductForm(): void {
    this.resetProductImageSelection();
    this.showProductForm.set(false);
    this.editingProductId.set(null);
    this.productForm = this.emptyProductForm();
  }

  saveProduct(): void {
    if (!this.canManage) {
      return;
    }

    const request = this.buildProductRequest();

    if (!request) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const save$ = this.editingProductId()
      ? this.adminCatalogService.updateProduct(this.editingProductId()!, request)
      : this.adminCatalogService.createProduct(request);

    save$
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set(this.editingProductId() ? 'Producto actualizado.' : 'Producto creado.');
          this.cancelProductForm();
          this.refreshProducts();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  toggleProductStatus(product: CatalogProduct): void {
    if (!this.canManage) {
      return;
    }

    const nextState = !product.isActive;

    if (!nextState && !window.confirm(`Desactivar el producto ${product.name}?`)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminCatalogService
      .setProductStatus(product.id, { isActive: nextState })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set(nextState ? 'Producto activado.' : 'Producto desactivado.');
          this.refreshProducts();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  startPriceEdit(product: CatalogProduct): void {
    if (!this.canManage) {
      return;
    }

    this.priceProductId.set(product.id);
    this.priceAmount = product.priceAmount;
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  cancelPriceEdit(): void {
    this.priceProductId.set(null);
    this.priceAmount = null;
  }

  saveProductPrice(product: CatalogProduct): void {
    if (!this.canManage) {
      return;
    }

    if (this.priceAmount === null || Number.isNaN(this.priceAmount)) {
      this.errorMessage.set('Captura un precio valido.');
      return;
    }

    if (this.priceAmount < 0) {
      this.errorMessage.set('El precio no puede ser negativo.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminCatalogService
      .updateProductPrice(product.id, { priceAmount: this.priceAmount, currency: 'MXN' })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.products.update((products) =>
            products.map((currentProduct) => currentProduct.id === updated.id ? updated : currentProduct)
          );
          this.successMessage.set('Precio actualizado.');
          this.cancelPriceEdit();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  updateSectionName(value: string): void {
    this.sectionForm.name = value;

    if (!this.editingSectionId() && !this.sectionForm.key.trim()) {
      this.sectionForm.key = this.slugify(value);
    }
  }

  updateProductName(value: string): void {
    this.productForm.name = value;

    if (!this.editingProductId() && !this.productForm.key.trim()) {
      this.productForm.key = this.slugify(value);
    }
  }

  onProductAssetImageChange(value: string): void {
    this.resetProductImageSelection();
    this.productForm.imagePath = value;
    this.errorMessage.set(null);
  }

  onProductImageFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    this.resetProductImageSelection(false);
    this.successMessage.set(null);

    const validationMessage = this.validateProductImageFile(file);

    if (validationMessage) {
      input.value = '';
      this.errorMessage.set(validationMessage);
      return;
    }

    this.selectedProductImageFile.set(file);
    this.localProductImagePreviewUrl.set(URL.createObjectURL(file!));
    this.errorMessage.set(null);
  }

  uploadSelectedProductImage(): void {
    if (!this.canManage || this.isImageBusy()) {
      return;
    }

    const productId = this.editingProductId();

    if (!productId) {
      this.errorMessage.set('Guarda el producto antes de subir una imagen personalizada.');
      return;
    }

    const file = this.selectedProductImageFile();
    const validationMessage = this.validateProductImageFile(file);

    if (validationMessage) {
      this.errorMessage.set(validationMessage);
      return;
    }

    this.isUploadingImage.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminCatalogService
      .uploadProductImage(productId, file!)
      .pipe(finalize(() => this.isUploadingImage.set(false)))
      .subscribe({
        next: (updated) => {
          this.updateProductLocally(updated);
          this.productForm.imagePath = updated.imagePath ?? '';
          this.resetProductImageSelection();
          this.successMessage.set('Imagen actualizada.');
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toImageUploadErrorMessage(error))
      });
  }

  clearCurrentProductImage(): void {
    if (!this.canManage || this.isImageBusy() || !this.productForm.imagePath) {
      return;
    }

    const productId = this.editingProductId();

    if (!productId || !window.confirm('¿Quitar la imagen de este producto?')) {
      return;
    }

    this.isClearingImage.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminCatalogService
      .clearProductImage(productId)
      .pipe(finalize(() => this.isClearingImage.set(false)))
      .subscribe({
        next: (updated) => {
          this.updateProductLocally(updated);
          this.productForm.imagePath = '';
          this.resetProductImageSelection();
          this.successMessage.set('Imagen desasociada.');
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toClearImageErrorMessage(error))
      });
  }

  productImageOrigin(path: string | null): string {
    if (!path) {
      return 'Sin imagen';
    }

    return isServerGeneratedCatalogImagePath(path) ? 'Imagen cargada' : 'Imagen del catálogo';
  }

  isServerGeneratedImagePath(path: string | null): boolean {
    return path ? isServerGeneratedCatalogImagePath(path) : false;
  }

  previewSrc(path: string | null): string {
    return path ? `/${path.replace(/^\/+/, '')}` : '';
  }

  imageNote(path: string | null): string | null {
    if (!path) {
      return null;
    }

    return CATALOG_IMAGE_OPTIONS.find((option) => option.path === path)?.note ?? null;
  }

  private refreshProducts(): void {
    this.isLoading.set(true);

    this.adminCatalogService
      .getProducts()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (products) => this.products.set(products),
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private updateProductLocally(updated: CatalogProduct): void {
    this.products.update((products) =>
      products.map((product) => product.id === updated.id ? updated : product)
    );
  }

  private buildSectionRequest(): CatalogSectionUpsertRequest | null {
    const key = (this.sectionForm.key.trim() || this.slugify(this.sectionForm.name)).trim();
    const name = this.sectionForm.name.trim();
    const imagePath = this.toOptionalSectionImagePath(this.sectionForm.imagePath);

    if (!name) {
      this.errorMessage.set('Captura el nombre de la seccion.');
      return null;
    }

    if (!key) {
      this.errorMessage.set('Captura la clave de la seccion.');
      return null;
    }

    if (imagePath === undefined) {
      this.errorMessage.set('Selecciona una imagen existente del catalogo o limpia el campo.');
      return null;
    }

    return {
      key,
      name,
      description: this.toOptionalText(this.sectionForm.description),
      imagePath,
      altText: this.toOptionalText(this.sectionForm.altText),
      sortOrder: Number(this.sectionForm.sortOrder) || 0,
      isActive: this.sectionForm.isActive
    };
  }

  private buildProductRequest(): CatalogProductUpsertRequest | null {
    const key = (this.productForm.key.trim() || this.slugify(this.productForm.name)).trim();
    const name = this.productForm.name.trim();
    const priceAmount = this.productForm.priceAmount;
    const imagePath = this.toOptionalImagePath(this.productForm.imagePath);

    if (!this.productForm.catalogSectionId) {
      this.errorMessage.set('Selecciona una seccion.');
      return null;
    }

    if (!name) {
      this.errorMessage.set('Captura el nombre del producto.');
      return null;
    }

    if (!key) {
      this.errorMessage.set('Captura la clave del producto.');
      return null;
    }

    if (priceAmount === null || Number.isNaN(priceAmount)) {
      this.errorMessage.set('Captura un precio valido.');
      return null;
    }

    if (priceAmount < 0) {
      this.errorMessage.set('El precio no puede ser negativo.');
      return null;
    }

    if (imagePath === undefined) {
      this.errorMessage.set('Selecciona una imagen existente del catalogo o limpia el campo.');
      return null;
    }

    return {
      catalogSectionId: this.productForm.catalogSectionId,
      key,
      name,
      description: this.toOptionalText(this.productForm.description),
      priceAmount,
      currency: 'MXN',
      imagePath,
      altText: this.toOptionalText(this.productForm.altText),
      sortOrder: Number(this.productForm.sortOrder) || 0,
      isActive: this.productForm.isActive
    };
  }

  private toOptionalText(value: string): string | null {
    const trimmed = value.trim();

    return trimmed ? trimmed : null;
  }

  private toOptionalImagePath(value: string): string | null | undefined {
    const trimmed = value.trim();

    if (!trimmed) {
      return null;
    }

    return isAllowedCatalogImagePath(trimmed) ? trimmed : undefined;
  }

  private toOptionalSectionImagePath(value: string): string | null | undefined {
    const trimmed = value.trim();

    if (!trimmed) {
      return null;
    }

    return CATALOG_IMAGE_OPTIONS.some((option) => option.path === trimmed) ? trimmed : undefined;
  }

  private validateProductImageFile(file: File | null): string | null {
    if (!file || file.size <= 0) {
      return 'Selecciona una imagen.';
    }

    if (file.size > MAX_CATALOG_IMAGE_BYTES) {
      return 'La imagen no puede superar 2 MB.';
    }

    const extension = file.name.includes('.') ? file.name.split('.').pop()!.toLowerCase() : '';
    const expectedMime = CATALOG_IMAGE_MIME_BY_EXTENSION[extension];
    const actualMime = file.type.toLowerCase();

    if (!expectedMime || actualMime !== expectedMime) {
      return 'Formato no permitido. Usa WebP, JPG o PNG.';
    }

    return null;
  }

  private resetProductImageSelection(resetInput = true): void {
    const previewUrl = this.localProductImagePreviewUrl();

    if (previewUrl) {
      URL.revokeObjectURL(previewUrl);
    }

    this.localProductImagePreviewUrl.set(null);
    this.selectedProductImageFile.set(null);

    if (resetInput && this.productImageInput) {
      this.productImageInput.nativeElement.value = '';
    }
  }

  private nextSectionSortOrder(): number {
    return this.sections().reduce((max, section) => Math.max(max, section.sortOrder), 0) + 10;
  }

  private nextProductSortOrder(sectionId: string): number {
    return this.products()
      .filter((product) => product.catalogSectionId === sectionId)
      .reduce((max, product) => Math.max(max, product.sortOrder), 0) + 10;
  }

  private emptySectionForm(sortOrder = 0): CatalogSectionForm {
    return {
      key: '',
      name: '',
      description: '',
      imagePath: '',
      altText: '',
      sortOrder,
      isActive: true
    };
  }

  private emptyProductForm(catalogSectionId = '', sortOrder = 0): CatalogProductForm {
    return {
      catalogSectionId,
      key: '',
      name: '',
      description: '',
      priceAmount: null,
      currency: 'MXN',
      imagePath: '',
      altText: '',
      sortOrder,
      isActive: true
    };
  }

  private slugify(value: string): string {
    return value
      .trim()
      .toLowerCase()
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .replace(/[^a-z0-9]+/g, '-')
      .replace(/^-+|-+$/g, '')
      .slice(0, 80);
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar el catalogo.';
    }

    if (error.status === 409) {
      return this.getProblemTitle(error) ?? 'La clave ya existe o el cambio entra en conflicto.';
    }

    if (error.status === 400) {
      return this.getValidationMessage(error) ?? 'Revisa los datos capturados.';
    }

    return 'No fue posible completar la operacion.';
  }

  private toImageUploadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'El archivo o sus datos no son válidos.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para administrar el catálogo.';
    }

    if (error.status === 404) {
      return 'El producto ya no existe.';
    }

    if (error.status === 413) {
      return 'La imagen no puede superar 2 MB.';
    }

    if (error.status === 503) {
      return 'El almacenamiento de imágenes no está disponible temporalmente.';
    }

    return 'No fue posible subir la imagen.';
  }

  private toClearImageErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar el catálogo.';
    }

    if (error.status === 404) {
      return 'El producto ya no existe.';
    }

    return 'No fue posible quitar la imagen.';
  }

  private getValidationMessage(error: HttpErrorResponse): string | null {
    const payload = error.error;

    if (!payload || typeof payload !== 'object' || !('errors' in payload)) {
      return this.getProblemTitle(error);
    }

    const errors = (payload as { errors?: Record<string, unknown> }).errors;

    if (!errors) {
      return this.getProblemTitle(error);
    }

    const firstError = Object.values(errors)
      .flatMap((value) => Array.isArray(value) ? value : [])
      .find((value): value is string => typeof value === 'string');

    return firstError ?? this.getProblemTitle(error);
  }

  private getProblemTitle(error: HttpErrorResponse): string | null {
    if (error.error && typeof error.error === 'object' && 'title' in error.error) {
      const title = (error.error as { title?: unknown }).title;

      return typeof title === 'string' ? title : null;
    }

    return null;
  }
}
