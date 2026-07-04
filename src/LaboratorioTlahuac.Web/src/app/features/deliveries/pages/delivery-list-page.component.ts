import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { DeliveryResponse, DeliveryStatus } from '../delivery.models';
import { DeliveryService } from '../delivery.service';

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

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (isLoading() && items().length === 0) {
        <p class="loading-state">Cargando entregas...</p>
      } @else if (!errorMessage() && items().length === 0) {
        <section class="driver-empty-state">
          <h2>Sin entregas asignadas</h2>
          <p>No hay entregas para mostrar en este momento.</p>
        </section>
      } @else if (items().length > 0) {
        <div class="driver-delivery-grid">
          @for (delivery of items(); track delivery.id) {
            <article class="driver-delivery-card" [class.is-closed]="isClosed(delivery)">
              <header>
                <div>
                  <span class="driver-order-label">Orden</span>
                  <a [routerLink]="['/app/entregas', delivery.id]">{{ delivery.orderNumber }}</a>
                </div>
                <span [class]="deliveryStatusClass(delivery.status)">
                  {{ delivery.statusLabel }}
                </span>
              </header>

              <div class="driver-card-main">
                <strong>{{ delivery.customerDisplayName || 'Cliente sin nombre' }}</strong>
                <span>{{ delivery.patientName || 'Paciente sin capturar' }}</span>
                @if (delivery.referenceNumber) {
                  <small>Referencia {{ delivery.referenceNumber }}</small>
                }
              </div>

              <dl>
                <div>
                  <dt>Fecha entrega</dt>
                  <dd>{{ formatDateOnly(delivery.deliveryDate) }}</dd>
                </div>
                <div>
                  <dt>Trabajo</dt>
                  <dd>{{ delivery.workSummary || '-' }}</dd>
                </div>
                @if (delivery.customerAddress) {
                  <div>
                    <dt>Direccion</dt>
                    <dd>{{ delivery.customerAddress }}</dd>
                  </div>
                }
                @if (hasContact(delivery)) {
                  <div>
                    <dt>Contacto</dt>
                    <dd>{{ contactLine(delivery) }}</dd>
                  </div>
                }
              </dl>

              <div class="driver-card-actions">
                <a class="primary-button" [routerLink]="['/app/entregas', delivery.id]">Ver detalle</a>
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

    .driver-delivery-card header {
      align-items: flex-start;
      display: flex;
      gap: var(--space-3);
      justify-content: space-between;
    }

    .driver-delivery-card header > div {
      display: grid;
      gap: 4px;
      min-width: 0;
    }

    .driver-order-label {
      color: var(--color-neutral-500);
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .driver-delivery-card a:not(.primary-button) {
      color: var(--color-primary-600);
      font-size: 1.2rem;
      font-weight: 900;
      overflow-wrap: anywhere;
      text-decoration: none;
    }

    .driver-card-main {
      display: grid;
      gap: 4px;
    }

    .driver-card-main strong {
      color: var(--color-neutral-900);
      font-size: 1.06rem;
      line-height: 1.25;
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

    .driver-delivery-card dl {
      display: grid;
      gap: var(--space-3);
      margin: 0;
    }

    .driver-delivery-card dl div {
      display: grid;
      gap: 4px;
    }

    .driver-delivery-card dt {
      color: var(--color-neutral-500);
      font-size: 0.8rem;
      font-weight: 800;
    }

    .driver-delivery-card dd {
      color: var(--color-neutral-700);
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-card-actions {
      display: grid;
    }

    @media (min-width: 760px) {
      .driver-delivery-grid {
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }
    }

    @media (max-width: 640px) {
      .driver-delivery-card {
        padding: var(--space-4);
      }

      .driver-delivery-card header {
        display: grid;
      }
    }
  `]
})
export class DeliveryListPageComponent implements OnInit {
  readonly items = signal<DeliveryResponse[]>([]);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  constructor(private readonly deliveryService: DeliveryService) {}

  ngOnInit(): void {
    this.load();
  }

  refresh(): void {
    this.load();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }

    this.page.set(page);
    this.load();
  }

  isClosed(delivery: DeliveryResponse): boolean {
    return delivery.status === 'Delivered' || delivery.status === 'FailedDelivery';
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
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.deliveryService
      .list({
        assignedToMe: true,
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
