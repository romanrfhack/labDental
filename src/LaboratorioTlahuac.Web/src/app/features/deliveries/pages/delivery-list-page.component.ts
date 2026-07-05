import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize, forkJoin, map } from 'rxjs';

import { DeliveryResponse, DeliveryStatus } from '../delivery.models';
import { DeliveryService } from '../delivery.service';

type DriverSummaryStatus = Extract<DeliveryStatus, 'Assigned' | 'OutForDelivery' | 'FailedDelivery' | 'Delivered'>;
type DeliveryListFilter = 'All' | DriverSummaryStatus;
type DeliverySummaryCounts = Record<DriverSummaryStatus, number>;

interface DeliveryFilterOption {
  value: DeliveryListFilter;
  label: string;
}

const DELIVERY_FILTERS: readonly DeliveryFilterOption[] = [
  { value: 'All', label: 'Todas' },
  { value: 'OutForDelivery', label: 'En ruta' },
  { value: 'Assigned', label: 'Asignadas' },
  { value: 'FailedDelivery', label: 'No entregadas' },
  { value: 'Delivered', label: 'Entregadas' }
];

const EMPTY_SUMMARY: DeliverySummaryCounts = {
  Assigned: 0,
  OutForDelivery: 0,
  FailedDelivery: 0,
  Delivered: 0
};

@Component({
  selector: 'app-delivery-list-page',
  imports: [RouterLink],
  template: `
    <section class="feature-page driver-deliveries-page">
      <header class="page-header">
        <div>
          <h1>Entregas</h1>
          <p>Ruta y cierre de entregas asignadas.</p>
        </div>
        <button class="secondary-button" type="button" [disabled]="isLoading()" (click)="refresh()">
          {{ isLoading() ? 'Actualizando...' : 'Actualizar' }}
        </button>
      </header>

      <section class="driver-summary-grid" aria-label="Resumen de entregas">
        @for (card of summaryCards(); track card.status) {
          <div class="driver-summary-card">
            <strong>{{ card.value }}</strong>
            <span>{{ card.label }}</span>
          </div>
        }
      </section>

      <nav class="driver-filter-bar" aria-label="Filtros de entregas">
        @for (filter of filters; track filter.value) {
          <button
            class="driver-filter-button"
            type="button"
            [class.is-active]="selectedFilter() === filter.value"
            [attr.aria-pressed]="selectedFilter() === filter.value"
            [disabled]="isLoading()"
            (click)="selectFilter(filter.value)"
          >
            <span>{{ filter.label }}</span>
            <strong>{{ filterCount(filter.value) }}</strong>
          </button>
        }
      </nav>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (isLoading() && items().length === 0) {
        <p class="loading-state">Cargando entregas...</p>
      } @else if (!errorMessage() && items().length === 0) {
        <section class="driver-empty-state">
          <h2>{{ emptyStateTitle() }}</h2>
          <p>{{ emptyStateDescription() }}</p>
        </section>
      } @else if (items().length > 0) {
        <div class="driver-delivery-grid">
          @for (delivery of items(); track delivery.id) {
            <article class="driver-delivery-card" [class.is-closed]="isClosed(delivery)">
              <header class="driver-card-header">
                <div>
                  <span class="driver-order-label">Folio</span>
                  <a class="driver-folio-link" [routerLink]="['/app/entregas', delivery.id]">
                    {{ delivery.orderNumber }}
                  </a>
                </div>
                <div class="driver-card-status-block">
                  <span [class]="deliveryStatusClass(delivery.status)">
                    {{ delivery.statusLabel }}
                  </span>
                  <span class="driver-card-date">{{ formatDateOnly(delivery.deliveryDate) }}</span>
                </div>
              </header>

              <div class="driver-card-main">
                <span class="driver-section-label">Cliente</span>
                <strong>{{ delivery.customerDisplayName || 'Cliente sin nombre' }}</strong>
                <span>{{ delivery.patientName || 'Paciente sin capturar' }}</span>
                @if (delivery.referenceNumber) {
                  <small>Referencia {{ delivery.referenceNumber }}</small>
                }
              </div>

              <dl class="driver-card-facts">
                <div>
                  <dt>Fecha entrega</dt>
                  <dd>{{ formatDateOnly(delivery.deliveryDate) }}</dd>
                </div>
                <div>
                  <dt>Trabajo</dt>
                  <dd>{{ delivery.workSummary || '-' }}</dd>
                </div>
                @if (delivery.customerAddress) {
                  <div class="driver-card-wide">
                    <dt>Direccion</dt>
                    <dd>{{ delivery.customerAddress }}</dd>
                  </div>
                }
                @if (hasContact(delivery)) {
                  <div class="driver-card-wide">
                    <dt>Contacto</dt>
                    <dd>{{ contactLine(delivery) }}</dd>
                  </div>
                }
              </dl>

              <div class="driver-card-actions">
                <a class="primary-button" [routerLink]="['/app/entregas', delivery.id]">
                  {{ primaryActionLabel(delivery) }}
                </a>
              </div>
            </article>
          }
        </div>

        <div class="page-actions pagination-actions">
          <button
            class="ghost-button"
            type="button"
            [disabled]="page() <= 1 || isLoading()"
            (click)="changePage(page() - 1)"
          >
            Anterior
          </button>
          <span>Pagina {{ page() }} de {{ totalPages() }}</span>
          <button
            class="ghost-button"
            type="button"
            [disabled]="page() >= totalPages() || isLoading()"
            (click)="changePage(page() + 1)"
          >
            Siguiente
          </button>
        </div>
      }
    </section>
  `,
  styles: [`
    .driver-deliveries-page {
      max-width: 980px;
      width: 100%;
    }

    .driver-summary-grid {
      display: grid;
      gap: var(--space-3);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .driver-summary-card {
      background: rgba(255, 255, 255, 0.98);
      border: 1px solid var(--color-neutral-100);
      border-radius: var(--radius-sm);
      box-shadow: var(--shadow-sm);
      display: grid;
      gap: 4px;
      min-width: 0;
      padding: var(--space-4);
    }

    .driver-summary-card strong {
      color: var(--color-neutral-900);
      font-size: 1.45rem;
      line-height: 1;
    }

    .driver-summary-card span {
      color: var(--color-neutral-500);
      font-size: 0.84rem;
      font-weight: 800;
    }

    .driver-filter-bar {
      display: flex;
      gap: var(--space-2);
      margin-inline: calc(var(--space-2) * -1);
      overflow-x: auto;
      padding: 0 var(--space-2) var(--space-1);
      scroll-snap-type: x proximity;
    }

    .driver-filter-button {
      align-items: center;
      background: var(--color-neutral-0);
      border: 1px solid var(--color-neutral-100);
      border-radius: var(--radius-pill);
      color: var(--color-neutral-600);
      cursor: pointer;
      display: inline-flex;
      flex: 0 0 auto;
      font-weight: 800;
      gap: var(--space-2);
      min-height: 42px;
      padding: 9px 12px;
      scroll-snap-align: start;
      white-space: nowrap;
    }

    .driver-filter-button strong {
      background: rgba(233, 247, 255, 0.9);
      border-radius: var(--radius-pill);
      color: var(--color-primary-600);
      min-width: 1.8rem;
      padding: 3px 7px;
      text-align: center;
    }

    .driver-filter-button.is-active {
      background: var(--color-navy-700);
      border-color: var(--color-navy-700);
      color: var(--color-neutral-0);
    }

    .driver-filter-button.is-active strong {
      background: rgba(255, 255, 255, 0.18);
      color: var(--color-neutral-0);
    }

    .driver-filter-button:disabled {
      cursor: not-allowed;
      opacity: 0.65;
    }

    .driver-empty-state {
      background: rgba(233, 247, 255, 0.72);
      border: 1px solid rgba(166, 223, 247, 0.78);
      border-radius: var(--radius-md);
      display: grid;
      gap: var(--space-2);
      padding: var(--space-5);
    }

    .driver-empty-state h2,
    .driver-empty-state p {
      margin: 0;
    }

    .driver-empty-state p {
      color: var(--color-neutral-500);
    }

    .driver-delivery-grid {
      display: grid;
      gap: var(--space-4);
    }

    .driver-delivery-card {
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 251, 255, 0.97));
      border: 1px solid var(--color-neutral-100);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-sm);
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .driver-delivery-card.is-closed {
      background: rgba(255, 255, 255, 0.92);
    }

    .driver-card-header {
      align-items: flex-start;
      display: flex;
      gap: var(--space-3);
      justify-content: space-between;
    }

    .driver-card-header > div {
      display: grid;
      gap: 4px;
      min-width: 0;
    }

    .driver-order-label,
    .driver-section-label {
      color: var(--color-neutral-500);
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .driver-folio-link {
      color: var(--color-primary-600);
      font-size: 1.18rem;
      font-weight: 900;
      overflow-wrap: anywhere;
      text-decoration: none;
    }

    .driver-card-status-block {
      align-items: end;
      justify-items: end;
      text-align: right;
    }

    .driver-card-date {
      color: var(--color-neutral-500);
      font-size: 0.84rem;
      font-weight: 800;
    }

    .driver-card-main {
      border-left: 4px solid var(--color-primary-500);
      display: grid;
      gap: 4px;
      padding-left: var(--space-3);
    }

    .driver-card-main strong {
      color: var(--color-neutral-900);
      font-size: 1.16rem;
      line-height: 1.22;
      overflow-wrap: anywhere;
    }

    .driver-card-main span {
      color: var(--color-neutral-700);
      font-weight: 800;
      overflow-wrap: anywhere;
    }

    .driver-card-main small {
      color: var(--color-neutral-500);
      font-weight: 700;
    }

    .driver-card-facts {
      display: grid;
      gap: var(--space-3);
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin: 0;
    }

    .driver-card-facts div {
      display: grid;
      gap: 4px;
      min-width: 0;
    }

    .driver-card-wide {
      grid-column: 1 / -1;
    }

    .driver-card-facts dt {
      color: var(--color-neutral-500);
      font-size: 0.8rem;
      font-weight: 800;
    }

    .driver-card-facts dd {
      color: var(--color-neutral-700);
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-card-actions {
      display: grid;
    }

    .driver-card-actions .primary-button {
      min-height: 48px;
      width: 100%;
    }

    @media (min-width: 760px) {
      .driver-summary-grid {
        grid-template-columns: repeat(4, minmax(0, 1fr));
      }

      .driver-delivery-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 640px) {
      .driver-delivery-card,
      .driver-empty-state {
        padding: var(--space-4);
      }

      .driver-card-header {
        display: grid;
      }

      .driver-card-status-block {
        align-items: start;
        justify-items: start;
        text-align: left;
      }

      .driver-card-facts {
        grid-template-columns: 1fr;
      }
    }
  `]
})
export class DeliveryListPageComponent implements OnInit {
  readonly filters = DELIVERY_FILTERS;
  readonly items = signal<DeliveryResponse[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly selectedFilter = signal<DeliveryListFilter>('All');
  readonly summary = signal<DeliverySummaryCounts>({ ...EMPTY_SUMMARY });
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));
  readonly totalSummaryCount = computed(() =>
    this.summary().Assigned
    + this.summary().OutForDelivery
    + this.summary().FailedDelivery
    + this.summary().Delivered
  );
  readonly summaryCards = computed(() => {
    const summary = this.summary();

    return [
      { label: 'Asignadas', status: 'Assigned' as const, value: summary.Assigned },
      { label: 'En ruta', status: 'OutForDelivery' as const, value: summary.OutForDelivery },
      { label: 'No entregadas', status: 'FailedDelivery' as const, value: summary.FailedDelivery },
      { label: 'Entregadas', status: 'Delivered' as const, value: summary.Delivered }
    ];
  });

  constructor(private readonly deliveryService: DeliveryService) {}

  ngOnInit(): void {
    this.load();
  }

  refresh(): void {
    this.load();
  }

  selectFilter(filter: DeliveryListFilter): void {
    if (filter === this.selectedFilter()) {
      return;
    }

    this.selectedFilter.set(filter);
    this.page.set(1);
    this.loadItems();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }

    this.page.set(page);
    this.loadItems();
  }

  filterCount(filter: DeliveryListFilter): number {
    return filter === 'All' ? this.totalSummaryCount() : this.summary()[filter];
  }

  emptyStateTitle(): string {
    return this.selectedFilter() === 'All'
      ? 'Sin entregas asignadas'
      : 'Sin entregas con este estado';
  }

  emptyStateDescription(): string {
    return this.selectedFilter() === 'All'
      ? 'No hay entregas para mostrar en este momento.'
      : 'Cambia el filtro o actualiza para revisar otras entregas.';
  }

  isClosed(delivery: DeliveryResponse): boolean {
    return delivery.status === 'Delivered' || delivery.status === 'FailedDelivery';
  }

  primaryActionLabel(delivery: DeliveryResponse): string {
    if (delivery.status === 'OutForDelivery' || delivery.status === 'Assigned') {
      return 'Registrar entrega';
    }

    if (delivery.status === 'FailedDelivery') {
      return 'Revisar entrega';
    }

    return 'Abrir entrega';
  }

  deliveryStatusClass(status: DeliveryStatus): string {
    return `status-pill delivery-status ${status}`;
  }

  formatDateOnly(value: string | null): string {
    if (!value) {
      return '-';
    }

    const [year, month, day] = value.split('-');

    return year && month && day ? `${day}/${month}/${year}` : value;
  }

  hasContact(delivery: DeliveryResponse): boolean {
    return !!(delivery.customerContactName || delivery.customerWhatsApp || delivery.customerPhone);
  }

  contactLine(delivery: DeliveryResponse): string {
    const parts: string[] = [];

    if (delivery.customerContactName) {
      parts.push(delivery.customerContactName);
    }

    if (delivery.customerWhatsApp) {
      parts.push(`WhatsApp ${delivery.customerWhatsApp}`);
    }

    if (delivery.customerPhone && delivery.customerPhone !== delivery.customerWhatsApp) {
      parts.push(`Tel. ${delivery.customerPhone}`);
    }

    return parts.length > 0 ? parts.join(' | ') : '-';
  }

  private load(): void {
    this.loadSummary();
    this.loadItems();
  }

  private loadItems(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.deliveryService
      .list({
        assignedToMe: true,
        status: this.selectedStatus(),
        page: this.page(),
        pageSize: this.pageSize()
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.items.set(response.items);
          this.page.set(response.page);
          this.pageSize.set(response.pageSize);
          this.totalCount.set(response.totalCount);
        },
        error: (error: HttpErrorResponse) => {
          this.items.set([]);
          this.totalCount.set(0);
          this.errorMessage.set(this.toLoadErrorMessage(error));
        }
      });
  }

  private loadSummary(): void {
    forkJoin({
      Assigned: this.countByStatus('Assigned'),
      OutForDelivery: this.countByStatus('OutForDelivery'),
      FailedDelivery: this.countByStatus('FailedDelivery'),
      Delivered: this.countByStatus('Delivered')
    }).subscribe({
      next: (summary) => this.summary.set(summary),
      error: () => this.summary.set({ ...EMPTY_SUMMARY })
    });
  }

  private countByStatus(status: DriverSummaryStatus) {
    return this.deliveryService
      .list({
        assignedToMe: true,
        status,
        page: 1,
        pageSize: 1
      })
      .pipe(map((response) => response.totalCount));
  }

  private selectedStatus(): DeliveryStatus | undefined {
    const filter = this.selectedFilter();

    return filter === 'All' ? undefined : filter;
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar entregas.';
    }

    if (error.status === 400) {
      return 'No fue posible aplicar los filtros de entregas.';
    }

    return 'No fue posible cargar tus entregas.';
  }
}
