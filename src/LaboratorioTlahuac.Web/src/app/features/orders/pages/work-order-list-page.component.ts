import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { CustomerListItem } from '../../customers/customer.models';
import { CustomerService } from '../../customers/customer.service';
import { WorkOrderStatusBadgeComponent } from '../components/work-order-status-badge.component';
import {
  WorkOrderListItem,
  WorkOrderStatus,
  WorkOrderStatusOption
} from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-list-page',
  imports: [CurrencyPipe, FormsModule, RouterLink, WorkOrderStatusBadgeComponent],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Ordenes de trabajo</h1>
          <p>Seguimiento operativo de trabajos dentales.</p>
        </div>
        @if (canCreate) {
          <a class="primary-button" routerLink="/app/ordenes/nueva">Nueva orden</a>
        }
      </header>

      <form class="toolbar orders-toolbar" (ngSubmit)="applyFilters()">
        <label class="filter-field">
          <span>Busqueda</span>
          <input type="search" name="search" [ngModel]="search()" (ngModelChange)="search.set($event)" />
        </label>
        <label class="filter-field">
          <span>Cliente</span>
          <select name="customerId" [ngModel]="customerId()" (ngModelChange)="customerId.set($event)">
            <option value="">Todos</option>
            @for (customer of customers(); track customer.id) {
              <option [value]="customer.id">{{ customer.displayName }}</option>
            }
          </select>
        </label>
        <label class="filter-field">
          <span>Estado</span>
          <select name="status" [ngModel]="status()" (ngModelChange)="status.set($event)">
            <option value="">Todos</option>
            @for (statusOption of statuses(); track statusOption.value) {
              <option [value]="statusOption.value">{{ statusOption.label }}</option>
            }
          </select>
        </label>
        <label class="filter-field">
          <span>Entrega desde</span>
          <input
            type="date"
            name="deliveryDateFrom"
            [ngModel]="deliveryDateFrom()"
            (ngModelChange)="deliveryDateFrom.set($event)"
          />
        </label>
        <label class="filter-field">
          <span>Entrega hasta</span>
          <input
            type="date"
            name="deliveryDateTo"
            [ngModel]="deliveryDateTo()"
            (ngModelChange)="deliveryDateTo.set($event)"
          />
        </label>
        <label class="check-field">
          <input
            type="checkbox"
            name="includeCancelled"
            [ngModel]="includeCancelled()"
            (ngModelChange)="includeCancelled.set($event)"
          />
          <span>Incluir canceladas</span>
        </label>
        <button class="secondary-button" type="submit">Filtrar</button>
      </form>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando ordenes...</p>
      } @else if (items().length === 0) {
        <p class="empty-state">No hay ordenes con los filtros actuales.</p>
      } @else {
        <div class="table-scroll orders-table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>Orden</th>
                <th>Cliente</th>
                <th>Paciente</th>
                <th>Trabajo</th>
                <th>Entrega</th>
                <th>Estado</th>
                <th>Total</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              @for (order of items(); track order.id) {
                <tr>
                  <td>
                    <a [routerLink]="['/app/ordenes', order.id]">{{ order.orderNumber }}</a>
                  </td>
                  <td>
                    {{ order.customerDisplayName }}
                    @if (order.internalDoctorFullName) {
                      <small class="muted-block">{{ order.internalDoctorFullName }}</small>
                    }
                  </td>
                  <td>{{ order.patientName }}</td>
                  <td>{{ order.workDescription }}</td>
                  <td>{{ formatDateOnly(order.deliveryDate) }}</td>
                  <td>
                    <app-work-order-status-badge [status]="order.status" [label]="order.statusLabel" />
                  </td>
                  <td>{{ order.totalAmount === null ? '-' : (order.totalAmount | currency: 'MXN':'symbol-narrow') }}</td>
                  <td>
                    <div class="page-actions">
                      <a class="ghost-button" [routerLink]="['/app/ordenes', order.id]">Ver</a>
                      @if (canEdit && !order.isCancelled) {
                        <a class="secondary-button" [routerLink]="['/app/ordenes', order.id, 'editar']">Editar</a>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="orders-mobile-list">
          @for (order of items(); track order.id) {
            <article class="order-card">
              <header>
                <div>
                  <a [routerLink]="['/app/ordenes', order.id]">{{ order.orderNumber }}</a>
                  <strong>{{ order.patientName }}</strong>
                </div>
                <app-work-order-status-badge [status]="order.status" [label]="order.statusLabel" />
              </header>

              <dl>
                <div>
                  <dt>Cliente</dt>
                  <dd>
                    {{ order.customerDisplayName }}
                    @if (order.internalDoctorFullName) {
                      <small class="muted-block">{{ order.internalDoctorFullName }}</small>
                    }
                  </dd>
                </div>
                <div>
                  <dt>Trabajo</dt>
                  <dd>{{ order.workDescription }}</dd>
                </div>
                <div>
                  <dt>Entrega</dt>
                  <dd>{{ formatDateOnly(order.deliveryDate) }}</dd>
                </div>
                <div>
                  <dt>Total</dt>
                  <dd>{{ order.totalAmount === null ? '-' : (order.totalAmount | currency: 'MXN':'symbol-narrow') }}</dd>
                </div>
              </dl>

              <div class="page-actions">
                <a class="ghost-button" [routerLink]="['/app/ordenes', order.id]">Ver</a>
                @if (canEdit && !order.isCancelled) {
                  <a class="secondary-button" [routerLink]="['/app/ordenes', order.id, 'editar']">Editar</a>
                }
              </div>
            </article>
          }
        </div>
      }

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
    </section>
  `
})
export class WorkOrderListPageComponent implements OnInit {
  readonly items = signal<WorkOrderListItem[]>([]);
  readonly customers = signal<CustomerListItem[]>([]);
  readonly statuses = signal<WorkOrderStatusOption[]>([]);
  readonly search = signal('');
  readonly customerId = signal('');
  readonly status = signal<WorkOrderStatus | ''>('');
  readonly deliveryDateFrom = signal('');
  readonly deliveryDateTo = signal('');
  readonly includeCancelled = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly customerService: CustomerService,
    private readonly authService: AuthService
  ) {}

  get canCreate(): boolean {
    return this.authService.hasPermission('orders.create');
  }

  get canEdit(): boolean {
    return this.authService.hasPermission('orders.edit');
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadStatuses();
    this.load();
  }

  applyFilters(): void {
    this.page.set(1);
    this.load();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }

    this.page.set(page);
    this.load();
  }

  formatDateOnly(value: string | null): string {
    if (!value) {
      return '-';
    }

    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.workOrderService
      .list({
        search: this.search().trim() || undefined,
        customerId: this.customerId() || undefined,
        status: this.status() || undefined,
        deliveryDateFrom: this.deliveryDateFrom() || undefined,
        deliveryDateTo: this.deliveryDateTo() || undefined,
        includeCancelled: this.includeCancelled(),
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
          this.errorMessage.set(this.toErrorMessage(error));
          this.items.set([]);
          this.totalCount.set(0);
        }
      });
  }

  private loadCustomers(): void {
    this.customerService.list({ isActive: true, pageSize: 100 }).subscribe({
      next: (response) => {
        this.customers.set(response.items);
      },
      error: (error: HttpErrorResponse) => {
        this.customers.set([]);
        this.errorMessage.set(this.toCustomerLoadErrorMessage(error));
      }
    });
  }

  private loadStatuses(): void {
    this.workOrderService.getStatuses().subscribe({
      next: (statuses) => {
        this.statuses.set(statuses);
      },
      error: (error: HttpErrorResponse) => {
        this.statuses.set([]);
        this.errorMessage.set(this.toStatusLoadErrorMessage(error));
      }
    });
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar ordenes.';
    }

    if (error.status === 400) {
      return 'Revisa los filtros capturados.';
    }

    return 'No fue posible cargar ordenes.';
  }

  private toCustomerLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar clientes.';
    }

    return 'No fue posible cargar clientes para filtros.';
  }

  private toStatusLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar estados de ordenes.';
    }

    return 'No fue posible cargar estados de ordenes.';
  }
}
