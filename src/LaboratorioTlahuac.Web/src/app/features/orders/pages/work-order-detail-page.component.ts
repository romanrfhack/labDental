import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryAdminSectionComponent } from '../../deliveries/components/delivery-admin-section.component';
import { WorkOrderPaymentsSectionComponent } from '../../payments/components/work-order-payments-section.component';
import { WorkOrderStatusBadgeComponent } from '../components/work-order-status-badge.component';
import { WorkOrderStatusChangeComponent } from '../components/work-order-status-change.component';
import { WorkOrderStatusHistorySectionComponent } from '../components/work-order-status-history-section.component';
import {
  WorkOrderChangeStatusRequest,
  WorkOrderDetail,
  WorkOrderStatusOption
} from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-detail-page',
  imports: [
    CurrencyPipe,
    DatePipe,
    DeliveryAdminSectionComponent,
    RouterLink,
    WorkOrderPaymentsSectionComponent,
    WorkOrderStatusBadgeComponent,
    WorkOrderStatusChangeComponent,
    WorkOrderStatusHistorySectionComponent
  ],
  template: `
    <section class="feature-page">
      @if (isLoading()) {
        <p class="loading-state">Cargando orden...</p>
      } @else if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      } @else if (order(); as order) {
        @if (successMessage(); as message) {
          <p class="alert-success" role="status">{{ message }}</p>
        }

        <header class="page-header">
          <div>
            <h1>{{ order.orderNumber }}</h1>
            <p>{{ order.customerDisplayName }} - {{ order.patientName }}</p>
          </div>
          <div class="page-actions">
            <a class="ghost-button" routerLink="/app/ordenes">Volver</a>
            <a class="secondary-button" [routerLink]="['/app/ordenes', order.id, 'etiqueta-trabajo']">
              Etiqueta interna
            </a>
            <a class="secondary-button" [routerLink]="['/app/ordenes', order.id, 'etiqueta-entrega']">
              Etiqueta entrega
            </a>
            @if (canEdit && !order.isCancelled) {
              <a class="secondary-button" [routerLink]="['/app/ordenes', order.id, 'editar']">Editar</a>
            }
          </div>
        </header>

        <div class="detail-grid">
          <div class="detail-item">
            <strong>Estado</strong>
            <app-work-order-status-badge [status]="order.status" [label]="order.statusLabel" />
          </div>
          <div class="detail-item">
            <strong>Cliente</strong>
            <span>{{ order.customerDisplayName }}</span>
          </div>
          <div class="detail-item">
            <strong>Doctor interno</strong>
            <span>{{ order.internalDoctorFullName || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Paciente</strong>
            <span>{{ order.patientName }}</span>
          </div>
          <div class="detail-item">
            <strong>Recepcion</strong>
            <span>{{ formatDateOnly(order.receivedDate) }}</span>
          </div>
          <div class="detail-item">
            <strong>Entrega</strong>
            <span>{{ formatDateOnly(order.deliveryDate) }}</span>
          </div>
          <div class="detail-item">
            <strong>Primera prueba</strong>
            <span>{{ formatDateOnly(order.firstTrialDate) }}</span>
          </div>
          <div class="detail-item">
            <strong>Segunda prueba</strong>
            <span>{{ formatDateOnly(order.secondTrialDate) }}</span>
          </div>
          <div class="detail-item">
            <strong>Referencia</strong>
            <span>{{ order.referenceNumber || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Color</strong>
            <span>{{ order.dentalColor || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Costo total</strong>
            <span>{{ order.totalAmount === null ? '-' : (order.totalAmount | currency: 'MXN':'symbol-narrow') }}</span>
          </div>
          <div class="detail-item">
            <strong>Actualizada</strong>
            <span>{{ order.updatedAtUtc | date: 'medium' }}</span>
          </div>
          <div class="detail-item full-field">
            <strong>Trabajo solicitado</strong>
            <span>{{ order.workDescription }}</span>
          </div>
          <div class="detail-item full-field">
            <strong>Observaciones</strong>
            <span>{{ order.notes || '-' }}</span>
          </div>
        </div>

        <app-delivery-admin-section
          [workOrderId]="order.id"
          [isWorkOrderCancelled]="order.isCancelled"
          (deliveryChanged)="refreshOrderAfterDeliveryChange()"
        />

        @if (canChangeStatus) {
          <section class="feature-page">
            <h2>Cambiar estado</h2>
            <app-work-order-status-change
              [currentStatus]="order.status"
              [statuses]="statuses()"
              [isCancelled]="order.isCancelled"
              [isSubmitting]="isChangingStatus()"
              [errorMessage]="statusErrorMessage() ?? ''"
              (changeStatus)="changeStatus($event)"
            />
          </section>
        }

        @if (canViewPayments) {
          <app-work-order-payments-section
            [workOrderId]="order.id"
            [totalAmount]="order.totalAmount"
            [isWorkOrderCancelled]="order.isCancelled"
          />
        }

        <app-work-order-status-history-section [history]="order.statusHistory" />
      }
    </section>
  `
})
export class WorkOrderDetailPageComponent implements OnInit {
  readonly order = signal<WorkOrderDetail | null>(null);
  readonly statuses = signal<WorkOrderStatusOption[]>([]);
  readonly isLoading = signal(false);
  readonly isChangingStatus = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly statusErrorMessage = signal<string | null>(null);

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    const info = this.router.currentNavigation()?.extras.info;

    if (this.isSuccessNavigationInfo(info)) {
      this.successMessage.set(info.successMessage);
    }
  }

  get canEdit(): boolean {
    return this.authService.hasPermission('orders.edit');
  }

  get canChangeStatus(): boolean {
    return this.authService.hasPermission('orders.changeStatus');
  }

  get canViewPayments(): boolean {
    return this.authService.hasPermission('payments.view');
  }

  ngOnInit(): void {
    this.loadStatuses();
    this.load();
  }

  changeStatus(request: WorkOrderChangeStatusRequest): void {
    const currentOrder = this.order();

    if (!currentOrder) {
      return;
    }

    this.isChangingStatus.set(true);
    this.statusErrorMessage.set(null);

    this.workOrderService
      .changeStatus(currentOrder.id, request)
      .pipe(finalize(() => this.isChangingStatus.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
        },
        error: (error: HttpErrorResponse) => {
          this.statusErrorMessage.set(this.toStatusErrorMessage(error));
        }
    });
  }

  refreshOrderAfterDeliveryChange(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      return;
    }

    this.workOrderService.getById(id).subscribe({
      next: (order) => {
        this.order.set(order);
      }
    });
  }

  formatDateOnly(value: string | null): string {
    if (!value) {
      return '-';
    }

    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigateByUrl('/app/ordenes');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.order.set(null);

    this.workOrderService
      .getById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toLoadErrorMessage(error));
          this.order.set(null);
        }
      });
  }

  private loadStatuses(): void {
    this.workOrderService.getStatuses().subscribe({
      next: (statuses) => {
        this.statuses.set(statuses);
      }
    });
  }

  private isSuccessNavigationInfo(value: unknown): value is { successMessage: string } {
    return typeof value === 'object'
      && value !== null
      && 'successMessage' in value
      && typeof value.successMessage === 'string'
      && value.successMessage.length > 0;
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 404) {
      return 'Orden no encontrada.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para ver ordenes.';
    }

    return 'No fue posible cargar la orden.';
  }

  private toStatusErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Revisa el estado y las notas.';
    }

    if (error.status === 409) {
      return 'La orden no permite ese cambio de estado.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para cambiar estado.';
    }

    return 'No fue posible cambiar el estado.';
  }
}
