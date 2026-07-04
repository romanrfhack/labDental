import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, computed, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import {
  PaymentListItem,
  PaymentMethod,
  PaymentMethodOption
} from '../payment.models';
import { PaymentService } from '../payment.service';

@Component({
  selector: 'app-payment-list-page',
  imports: [CurrencyPipe, FormsModule, RouterLink],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Pagos</h1>
          <p>Consulta de abonos registrados por orden de trabajo.</p>
        </div>
      </header>

      <form class="toolbar payments-toolbar" (ngSubmit)="applyFilters()">
        <label class="filter-field">
          <span>Busqueda</span>
          <input type="search" name="search" [ngModel]="search()" (ngModelChange)="search.set($event)" />
        </label>
        <label class="filter-field">
          <span>Metodo</span>
          <select name="method" [ngModel]="method()" (ngModelChange)="method.set($event)">
            <option value="">Todos</option>
            @for (methodOption of methods(); track methodOption.value) {
              <option [value]="methodOption.value">{{ methodOption.label }}</option>
            }
          </select>
        </label>
        <label class="filter-field">
          <span>Desde</span>
          <input
            type="date"
            name="paymentDateFrom"
            [ngModel]="paymentDateFrom()"
            (ngModelChange)="paymentDateFrom.set($event)"
          />
        </label>
        <label class="filter-field">
          <span>Hasta</span>
          <input
            type="date"
            name="paymentDateTo"
            [ngModel]="paymentDateTo()"
            (ngModelChange)="paymentDateTo.set($event)"
          />
        </label>
        <label class="check-field">
          <input
            type="checkbox"
            name="includeCancelled"
            [ngModel]="includeCancelled()"
            (ngModelChange)="includeCancelled.set($event)"
          />
          <span>Incluir cancelados</span>
        </label>
        <button class="secondary-button" type="submit">Filtrar</button>
      </form>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando pagos...</p>
      } @else if (items().length === 0) {
        <p class="empty-state">No hay pagos con los filtros actuales.</p>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Orden</th>
              <th>Cliente</th>
              <th>Paciente</th>
              <th>Fecha</th>
              <th>Monto</th>
              <th>Metodo</th>
              <th>Referencia</th>
              <th>Cancelado</th>
            </tr>
          </thead>
          <tbody>
            @for (payment of items(); track payment.id) {
              <tr>
                <td>
                  <a [routerLink]="['/app/ordenes', payment.workOrderId]">{{ payment.orderNumber }}</a>
                </td>
                <td>{{ payment.customerDisplayName }}</td>
                <td>{{ payment.patientName }}</td>
                <td>{{ formatDateOnly(payment.paymentDate) }}</td>
                <td>{{ payment.amount | currency: 'MXN':'symbol-narrow' }}</td>
                <td>{{ payment.methodLabel }}</td>
                <td>{{ payment.reference || '-' }}</td>
                <td>
                  <span class="status-pill" [class.active]="!payment.isCancelled" [class.inactive]="payment.isCancelled">
                    {{ payment.isCancelled ? 'Si' : 'No' }}
                  </span>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }

      <div class="page-actions">
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
export class PaymentListPageComponent implements OnInit {
  readonly items = signal<PaymentListItem[]>([]);
  readonly methods = signal<PaymentMethodOption[]>([]);
  readonly search = signal('');
  readonly method = signal<PaymentMethod | ''>('');
  readonly paymentDateFrom = signal('');
  readonly paymentDateTo = signal('');
  readonly includeCancelled = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  constructor(private readonly paymentService: PaymentService) {}

  ngOnInit(): void {
    this.loadMethods();
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

  formatDateOnly(value: string): string {
    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.paymentService
      .list({
        search: this.search().trim() || undefined,
        method: this.method() || undefined,
        paymentDateFrom: this.paymentDateFrom() || undefined,
        paymentDateTo: this.paymentDateTo() || undefined,
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

  private loadMethods(): void {
    this.paymentService.getMethods().subscribe({
      next: (methods) => {
        this.methods.set(methods);
      },
      error: () => {
        this.methods.set([]);
        this.errorMessage.set('No fue posible cargar metodos de pago.');
      }
    });
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar pagos.';
    }

    if (error.status === 400) {
      return 'Revisa los filtros capturados.';
    }

    return 'No fue posible cargar pagos.';
  }
}
