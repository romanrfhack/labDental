import { CurrencyPipe } from '@angular/common';
import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import {
  PaymentCreateRequest,
  PaymentMethodOption,
  PaymentSummary,
  WorkOrderPayment
} from '../payment.models';
import { PaymentService } from '../payment.service';
import { PaymentCancelActionComponent } from './payment-cancel-action.component';
import { PaymentCreateFormComponent } from './payment-create-form.component';
import { PaymentSummaryCardComponent } from './payment-summary-card.component';

@Component({
  selector: 'app-work-order-payments-section',
  imports: [
    CurrencyPipe,
    FormsModule,
    PaymentCancelActionComponent,
    PaymentCreateFormComponent,
    PaymentSummaryCardComponent
  ],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h2>Pagos</h2>
          <p>Abonos registrados y saldo calculado.</p>
        </div>
        <label class="check-field compact-check">
          <input type="checkbox" [(ngModel)]="includeCancelled" (ngModelChange)="loadPayments()" />
          <span>Incluir cancelados</span>
        </label>
      </header>

      @if (summary) {
        <app-payment-summary-card [summary]="summary" />
      } @else if (isLoadingSummary) {
        <p class="loading-state">Cargando resumen financiero...</p>
      }

      @if (loadErrorMessage) {
        <p class="alert-error" role="alert">{{ loadErrorMessage }}</p>
      }

      @if (canCreate) {
        @if (totalAmount === null) {
          <p class="empty-state">Define el total de la orden antes de registrar pagos.</p>
        } @else if (isWorkOrderCancelled) {
          <p class="empty-state">La orden cancelada no permite registrar pagos.</p>
        } @else {
          <app-payment-create-form
            [methods]="methods"
            [isSubmitting]="isCreating"
            [errorMessage]="createErrorMessage"
            [resetSignal]="createResetSignal"
            (create)="createPayment($event)"
          />
        }
      }

      @if (cancelErrorMessage) {
        <p class="alert-error" role="alert">{{ cancelErrorMessage }}</p>
      }

      @if (isLoadingPayments) {
        <p class="loading-state">Cargando pagos...</p>
      } @else if (payments.length === 0) {
        <p class="empty-state">No hay pagos registrados con los filtros actuales.</p>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Monto</th>
              <th>Metodo</th>
              <th>Referencia</th>
              <th>Observaciones</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            @for (payment of payments; track payment.id) {
              <tr>
                <td>{{ formatDateOnly(payment.paymentDate) }}</td>
                <td>{{ payment.amount | currency: 'MXN':'symbol-narrow' }}</td>
                <td>{{ payment.methodLabel }}</td>
                <td>{{ payment.reference || '-' }}</td>
                <td>
                  {{ payment.notes || '-' }}
                  @if (payment.isCancelled && payment.cancellationReason) {
                    <small class="muted-block">Motivo: {{ payment.cancellationReason }}</small>
                  }
                </td>
                <td>
                  <span class="status-pill" [class.active]="!payment.isCancelled" [class.inactive]="payment.isCancelled">
                    {{ payment.isCancelled ? 'Cancelado' : 'Activo' }}
                  </span>
                </td>
                <td>
                  @if (canCancel && !payment.isCancelled) {
                    <app-payment-cancel-action
                      [payment]="payment"
                      [isSubmitting]="cancellingPaymentId === payment.id"
                      (cancelPayment)="cancelPayment(payment, $event)"
                    />
                  } @else {
                    <span class="muted-block">-</span>
                  }
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `
})
export class WorkOrderPaymentsSectionComponent implements OnInit, OnChanges {
  @Input({ required: true }) workOrderId!: string;
  @Input() totalAmount: number | null = null;
  @Input() isWorkOrderCancelled = false;

  summary: PaymentSummary | null = null;
  payments: WorkOrderPayment[] = [];
  methods: PaymentMethodOption[] = [];
  includeCancelled = false;
  isLoadingSummary = false;
  isLoadingPayments = false;
  isCreating = false;
  cancellingPaymentId = '';
  createResetSignal = 0;
  loadErrorMessage = '';
  createErrorMessage = '';
  cancelErrorMessage = '';

  constructor(
    private readonly paymentService: PaymentService,
    private readonly authService: AuthService
  ) {}

  get canCreate(): boolean {
    return this.authService.hasPermission('payments.create');
  }

  get canCancel(): boolean {
    return this.authService.hasPermission('payments.cancel');
  }

  ngOnInit(): void {
    this.loadMethods();
    this.loadAll();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['workOrderId'] && !changes['workOrderId'].firstChange) {
      this.loadAll();
    }
  }

  loadPayments(): void {
    if (!this.workOrderId) {
      return;
    }

    this.isLoadingPayments = true;
    this.loadErrorMessage = '';

    this.paymentService
      .listForWorkOrder(this.workOrderId, { includeCancelled: this.includeCancelled })
      .pipe(finalize(() => (this.isLoadingPayments = false)))
      .subscribe({
        next: (payments) => {
          this.payments = payments;
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage = this.toLoadErrorMessage(error);
        }
      });
  }

  createPayment(request: PaymentCreateRequest): void {
    if (!this.workOrderId || this.isCreating) {
      return;
    }

    this.isCreating = true;
    this.createErrorMessage = '';

    this.paymentService
      .create(this.workOrderId, request)
      .pipe(finalize(() => (this.isCreating = false)))
      .subscribe({
        next: (response) => {
          this.summary = response.summary;
          this.createResetSignal++;
          this.loadPayments();
        },
        error: (error: HttpErrorResponse) => {
          this.createErrorMessage = this.toCreateErrorMessage(error);
        }
      });
  }

  cancelPayment(payment: WorkOrderPayment, reason: string): void {
    if (!this.workOrderId || this.cancellingPaymentId) {
      return;
    }

    this.cancellingPaymentId = payment.id;
    this.cancelErrorMessage = '';

    this.paymentService
      .cancel(this.workOrderId, payment.id, reason)
      .pipe(finalize(() => (this.cancellingPaymentId = '')))
      .subscribe({
        next: (response) => {
          this.summary = response.summary;
          this.loadPayments();
        },
        error: (error: HttpErrorResponse) => {
          this.cancelErrorMessage = this.toCancelErrorMessage(error);
        }
      });
  }

  formatDateOnly(value: string): string {
    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }

  private loadAll(): void {
    this.loadSummary();
    this.loadPayments();
  }

  private loadSummary(): void {
    if (!this.workOrderId) {
      return;
    }

    this.isLoadingSummary = true;
    this.loadErrorMessage = '';

    this.paymentService
      .getSummary(this.workOrderId)
      .pipe(finalize(() => (this.isLoadingSummary = false)))
      .subscribe({
        next: (summary) => {
          this.summary = summary;
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage = this.toLoadErrorMessage(error);
        }
      });
  }

  private loadMethods(): void {
    this.paymentService.getMethods().subscribe({
      next: (methods) => {
        this.methods = methods;
      }
    });
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar pagos.';
    }

    if (error.status === 404) {
      return 'Orden no encontrada para pagos.';
    }

    return 'No fue posible cargar pagos.';
  }

  private toCreateErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Revisa fecha, monto y metodo. Si la sesion sigue abierta, vuelve a intentar para renovar XSRF.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para registrar pagos.';
    }

    if (error.status === 409) {
      return 'La orden no permite registrar pagos: revisa total y estado.';
    }

    return 'No fue posible registrar el pago.';
  }

  private toCancelErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Captura un motivo de cancelacion valido. Si la sesion sigue abierta, vuelve a intentar para renovar XSRF.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para cancelar pagos.';
    }

    if (error.status === 409) {
      return 'El pago no puede cancelarse porque ya fue cancelado o no pertenece a esta orden.';
    }

    return 'No fue posible cancelar el pago.';
  }
}
