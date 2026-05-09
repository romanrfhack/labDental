import { CurrencyPipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
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
          <input type="search" name="search" [(ngModel)]="search" />
        </label>
        <label class="filter-field">
          <span>Cliente</span>
          <select name="customerId" [(ngModel)]="customerId">
            <option value="">Todos</option>
            @for (customer of customers; track customer.id) {
              <option [value]="customer.id">{{ customer.displayName }}</option>
            }
          </select>
        </label>
        <label class="filter-field">
          <span>Estado</span>
          <select name="status" [(ngModel)]="status">
            <option value="">Todos</option>
            @for (statusOption of statuses; track statusOption.value) {
              <option [value]="statusOption.value">{{ statusOption.label }}</option>
            }
          </select>
        </label>
        <label class="filter-field">
          <span>Entrega desde</span>
          <input type="date" name="deliveryDateFrom" [(ngModel)]="deliveryDateFrom" />
        </label>
        <label class="filter-field">
          <span>Entrega hasta</span>
          <input type="date" name="deliveryDateTo" [(ngModel)]="deliveryDateTo" />
        </label>
        <label class="check-field">
          <input type="checkbox" name="includeCancelled" [(ngModel)]="includeCancelled" />
          <span>Incluir canceladas</span>
        </label>
        <button class="secondary-button" type="submit">Filtrar</button>
      </form>

      @if (errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }

      @if (isLoading) {
        <p class="loading-state">Cargando ordenes...</p>
      } @else if (items.length === 0) {
        <p class="empty-state">No hay ordenes con los filtros actuales.</p>
      } @else {
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
            @for (order of items; track order.id) {
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
      }

      <div class="page-actions">
        <button class="ghost-button" type="button" [disabled]="page <= 1 || isLoading" (click)="changePage(page - 1)">
          Anterior
        </button>
        <span>Pagina {{ page }} de {{ totalPages }}</span>
        <button
          class="ghost-button"
          type="button"
          [disabled]="page >= totalPages || isLoading"
          (click)="changePage(page + 1)"
        >
          Siguiente
        </button>
      </div>
    </section>
  `
})
export class WorkOrderListPageComponent implements OnInit {
  items: WorkOrderListItem[] = [];
  customers: CustomerListItem[] = [];
  statuses: WorkOrderStatusOption[] = [];
  search = '';
  customerId = '';
  status: WorkOrderStatus | '' = '';
  deliveryDateFrom = '';
  deliveryDateTo = '';
  includeCancelled = false;
  page = 1;
  pageSize = 20;
  totalCount = 0;
  isLoading = false;
  errorMessage = '';

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

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadStatuses();
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) {
      return;
    }

    this.page = page;
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
    this.isLoading = true;
    this.errorMessage = '';

    this.workOrderService
      .list({
        search: this.search.trim() || undefined,
        customerId: this.customerId || undefined,
        status: this.status || undefined,
        deliveryDateFrom: this.deliveryDateFrom || undefined,
        deliveryDateTo: this.deliveryDateTo || undefined,
        includeCancelled: this.includeCancelled,
        page: this.page,
        pageSize: this.pageSize
      })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          this.items = response.items;
          this.page = response.page;
          this.pageSize = response.pageSize;
          this.totalCount = response.totalCount;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.toErrorMessage(error);
        }
      });
  }

  private loadCustomers(): void {
    this.customerService.list({ isActive: true, pageSize: 100 }).subscribe({
      next: (response) => {
        this.customers = response.items;
      }
    });
  }

  private loadStatuses(): void {
    this.workOrderService.getStatuses().subscribe({
      next: (statuses) => {
        this.statuses = statuses;
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
}
