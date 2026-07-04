import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable, filter, finalize, take } from 'rxjs';

import { AuthUser } from '../../../core/auth/auth.models';
import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryResponse, DeliveryStatus } from '../delivery.models';
import { DeliveryService } from '../delivery.service';

type DriverDeliveryAction = 'complete' | 'failed';

@Component({
  selector: 'app-delivery-detail-page',
  imports: [DatePipe, FormsModule, RouterLink],
  template: `
    <section class="feature-page driver-delivery-detail-page">
      <header class="page-header">
        <div>
          <h1>Detalle de entrega</h1>
          @if (delivery(); as delivery) {
            <p>{{ delivery.orderNumber }} · {{ delivery.customerDisplayName }}</p>
          } @else {
            <p>Entrega asignada.</p>
          }
        </div>
        <a class="ghost-button" routerLink="/app/entregas">Volver</a>
      </header>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (isLoading() && !delivery()) {
        <p class="loading-state">Cargando entrega...</p>
      }

      @if (delivery(); as delivery) {
        @if (isLoading()) {
          <p class="loading-state">Actualizando entrega...</p>
        }

        <section class="driver-detail-hero">
          <div>
            <span class="driver-section-label">Cliente</span>
            <h2>{{ delivery.customerDisplayName || 'Cliente sin nombre' }}</h2>
            @if (delivery.customerAddress) {
              <p>{{ delivery.customerAddress }}</p>
            }
          </div>
          <span [class]="deliveryStatusClass(delivery.status)">{{ delivery.statusLabel }}</span>
        </section>

        <section class="driver-contact-panel">
          <div>
            <strong>Contacto</strong>
            <span>{{ contactLine(delivery) }}</span>
          </div>
          @if (delivery.customerPhone) {
            <a class="secondary-button" [href]="phoneHref(delivery.customerPhone)">Llamar</a>
          }
        </section>

        <section class="driver-detail-grid">
          <div class="driver-detail-item">
            <strong>Folio</strong>
            <span>{{ delivery.orderNumber }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Paciente</strong>
            <span>{{ delivery.patientName || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Referencia</strong>
            <span>{{ delivery.referenceNumber || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Fecha de entrega</strong>
            <span>{{ formatDateOnly(delivery.deliveryDate) }}</span>
          </div>
          <div class="driver-detail-item driver-detail-wide">
            <strong>Trabajo</strong>
            <span>{{ delivery.workSummary || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Estado de orden</strong>
            <span>{{ delivery.workOrderStatusLabel || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Doctor interno</strong>
            <span>{{ delivery.internalDoctorFullName || '-' }}</span>
          </div>
        </section>

        <section class="driver-timeline-panel">
          <h2>Seguimiento</h2>
          <dl>
            <div>
              <dt>Asignada</dt>
              <dd>{{ delivery.assignedAtUtc ? (delivery.assignedAtUtc | date: 'medium') : '-' }}</dd>
            </div>
            <div>
              <dt>Salida</dt>
              <dd>{{ delivery.outForDeliveryAtUtc ? (delivery.outForDeliveryAtUtc | date: 'medium') : '-' }}</dd>
            </div>
            <div>
              <dt>Entregada</dt>
              <dd>{{ delivery.deliveredAtUtc ? (delivery.deliveredAtUtc | date: 'medium') : '-' }}</dd>
            </div>
            <div>
              <dt>No entregada</dt>
              <dd>{{ delivery.failedAtUtc ? (delivery.failedAtUtc | date: 'medium') : '-' }}</dd>
            </div>
            <div>
              <dt>Recibio</dt>
              <dd>{{ delivery.recipientName || '-' }}</dd>
            </div>
            <div>
              <dt>Motivo</dt>
              <dd>{{ delivery.failedReason || '-' }}</dd>
            </div>
          </dl>
        </section>

        @if (actionErrorMessage(); as message) {
          <p class="alert-error" role="alert">{{ message }}</p>
        }

        @if (!canCompleteDelivery) {
          <p class="empty-state">No tienes permiso para cerrar entregas.</p>
        } @else if (canShowCloseActions(delivery)) {
          <section class="driver-action-stack">
            @if (canMarkDelivered(delivery)) {
              <form class="driver-action-panel" novalidate (ngSubmit)="markDelivered(delivery)">
                <header>
                  <h2>Entregada</h2>
                </header>
                <label class="form-field">
                  <span>Recibio</span>
                  <input
                    name="recipientName"
                    type="text"
                    maxlength="150"
                    required
                    [(ngModel)]="recipientName"
                  />
                </label>
                <button class="primary-button" type="submit" [disabled]="isActionBusy()">
                  {{ activeAction() === 'complete' ? 'Guardando...' : 'Marcar entregada' }}
                </button>
              </form>
            }

            @if (canMarkFailed(delivery)) {
              <form class="driver-action-panel" novalidate (ngSubmit)="markFailed(delivery)">
                <header>
                  <h2>No entregada</h2>
                </header>
                <label class="form-field">
                  <span>Motivo</span>
                  <textarea
                    name="failedReason"
                    maxlength="1000"
                    required
                    [(ngModel)]="failedReason"
                  ></textarea>
                </label>
                <button class="danger-button" type="submit" [disabled]="isActionBusy()">
                  {{ activeAction() === 'failed' ? 'Guardando...' : 'Marcar no entregada' }}
                </button>
              </form>
            }
          </section>
        } @else {
          <p class="empty-state">Esta entrega no tiene acciones disponibles.</p>
        }
      }
    </section>
  `,
  styles: [`
    .driver-delivery-detail-page {
      max-width: 940px;
      width: 100%;
    }

    .driver-detail-hero,
    .driver-contact-panel,
    .driver-timeline-panel,
    .driver-action-panel,
    .driver-detail-item {
      background: linear-gradient(180deg, rgba(255, 255, 255, 0.98), rgba(248, 251, 255, 0.97));
      border: 1px solid var(--color-neutral-100);
      border-radius: var(--radius-md);
      box-shadow: var(--shadow-sm);
    }

    .driver-detail-hero {
      align-items: flex-start;
      display: flex;
      gap: var(--space-4);
      justify-content: space-between;
      padding: var(--space-5);
    }

    .driver-detail-hero > div {
      display: grid;
      gap: var(--space-2);
      min-width: 0;
    }

    .driver-section-label {
      color: var(--color-neutral-500);
      font-size: 0.76rem;
      font-weight: 800;
      text-transform: uppercase;
    }

    .driver-detail-hero h2 {
      color: var(--color-neutral-900);
      font-size: 1.42rem;
      line-height: 1.12;
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-detail-hero p {
      color: var(--color-neutral-700);
      line-height: 1.45;
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-contact-panel {
      align-items: center;
      display: flex;
      gap: var(--space-4);
      justify-content: space-between;
      padding: var(--space-4) var(--space-5);
    }

    .driver-contact-panel div {
      display: grid;
      gap: 4px;
      min-width: 0;
    }

    .driver-contact-panel strong,
    .driver-detail-item strong {
      color: var(--color-neutral-700);
      font-size: 0.84rem;
    }

    .driver-contact-panel span,
    .driver-detail-item span {
      color: var(--color-neutral-800);
      overflow-wrap: anywhere;
    }

    .driver-detail-grid {
      display: grid;
      gap: var(--space-4);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .driver-detail-item {
      display: grid;
      gap: 6px;
      padding: var(--space-4);
    }

    .driver-detail-wide {
      grid-column: 1 / -1;
    }

    .driver-timeline-panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .driver-timeline-panel h2,
    .driver-action-panel h2 {
      font-size: 1rem;
      margin: 0;
    }

    .driver-timeline-panel dl {
      display: grid;
      gap: var(--space-3);
      grid-template-columns: repeat(2, minmax(0, 1fr));
      margin: 0;
    }

    .driver-timeline-panel dl div {
      display: grid;
      gap: 4px;
    }

    .driver-timeline-panel dt {
      color: var(--color-neutral-500);
      font-size: 0.8rem;
      font-weight: 800;
    }

    .driver-timeline-panel dd {
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-action-stack {
      display: grid;
      gap: var(--space-4);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .driver-action-panel {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .driver-action-panel textarea {
      min-height: 112px;
      resize: vertical;
    }

    .driver-action-panel button {
      width: 100%;
    }

    .driver-action-panel button.danger-button[type='submit'] {
      background: linear-gradient(135deg, #dc2626, #b91c1c);
      color: var(--color-neutral-0);
    }

    @media (max-width: 760px) {
      .driver-detail-hero,
      .driver-contact-panel {
        display: grid;
      }

      .driver-contact-panel .secondary-button {
        width: 100%;
      }

      .driver-detail-grid,
      .driver-timeline-panel dl,
      .driver-action-stack {
        grid-template-columns: 1fr;
      }
    }

    @media (max-width: 640px) {
      .driver-detail-hero,
      .driver-contact-panel,
      .driver-timeline-panel,
      .driver-action-panel {
        padding: var(--space-4);
      }
    }
  `]
})
export class DeliveryDetailPageComponent implements OnInit {
  readonly delivery = signal<DeliveryResponse | null>(null);
  readonly isLoading = signal(false);
  readonly activeAction = signal<DriverDeliveryAction | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly actionErrorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  recipientName = '';
  failedReason = '';

  private deliveryId = '';
  private currentUserId: string | null = null;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly deliveryService: DeliveryService,
    private readonly authService: AuthService
  ) {}

  get canCompleteDelivery(): boolean {
    return this.authService.hasPermission('deliveries.complete');
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage.set('Entrega no encontrada.');
      return;
    }

    this.deliveryId = id;

    this.authService.currentUser$
      .pipe(
        filter((user): user is AuthUser | null => user !== undefined),
        take(1)
      )
      .subscribe((user) => {
        this.currentUserId = user?.id ?? null;
        this.loadDelivery();
      });
  }

  markDelivered(delivery: DeliveryResponse): void {
    if (!this.canCompleteDelivery || !this.canMarkDelivered(delivery) || this.isActionBusy()) {
      return;
    }

    const recipientName = this.recipientName.trim();

    if (!recipientName) {
      this.actionErrorMessage.set('Captura quien recibio la entrega.');
      return;
    }

    this.runDeliveryAction(
      'complete',
      () => this.deliveryService.complete(delivery.id, {
        recipientName,
        deliveryNotes: null
      }),
      'Entrega marcada como entregada.'
    );
  }

  markFailed(delivery: DeliveryResponse): void {
    if (!this.canCompleteDelivery || !this.canMarkFailed(delivery) || this.isActionBusy()) {
      return;
    }

    const failedReason = this.failedReason.trim();

    if (!failedReason) {
      this.actionErrorMessage.set('Captura el motivo de no entrega.');
      return;
    }

    this.runDeliveryAction(
      'failed',
      () => this.deliveryService.markFailed(delivery.id, {
        failedReason,
        deliveryNotes: null
      }),
      'Entrega marcada como no entregada.'
    );
  }

  canShowCloseActions(delivery: DeliveryResponse): boolean {
    return this.canMarkDelivered(delivery) || this.canMarkFailed(delivery);
  }

  canMarkDelivered(delivery: DeliveryResponse): boolean {
    return delivery.status === 'OutForDelivery';
  }

  canMarkFailed(delivery: DeliveryResponse): boolean {
    return delivery.status === 'Assigned' || delivery.status === 'OutForDelivery';
  }

  isActionBusy(): boolean {
    return this.activeAction() !== null;
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

  phoneHref(phone: string): string {
    return `tel:${phone.replace(/\s/g, '')}`;
  }

  private loadDelivery(clearCurrent = true): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    if (clearCurrent) {
      this.delivery.set(null);
    }

    this.deliveryService
      .getById(this.deliveryId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (delivery) => this.setDeliveryIfAssigned(delivery),
        error: (error: HttpErrorResponse) => {
          if (clearCurrent) {
            this.delivery.set(null);
          }

          this.errorMessage.set(this.toLoadErrorMessage(error));
        }
      });
  }

  private runDeliveryAction(
    action: DriverDeliveryAction,
    request: () => Observable<DeliveryResponse>,
    successMessage: string
  ): void {
    this.activeAction.set(action);
    this.actionErrorMessage.set(null);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    request()
      .pipe(finalize(() => this.activeAction.set(null)))
      .subscribe({
        next: (delivery) => {
          this.setDeliveryIfAssigned(delivery);
          this.recipientName = '';
          this.failedReason = '';
          this.successMessage.set(successMessage);
          this.loadDelivery(false);
        },
        error: (error: HttpErrorResponse) => {
          this.actionErrorMessage.set(this.toActionErrorMessage(error, action));
        }
      });
  }

  private setDeliveryIfAssigned(delivery: DeliveryResponse): void {
    if (!this.isAssignedToCurrentUser(delivery)) {
      this.delivery.set(null);
      this.errorMessage.set('Esta entrega no esta asignada a tu usuario.');
      return;
    }

    this.errorMessage.set(null);
    this.delivery.set(delivery);
  }

  private isAssignedToCurrentUser(delivery: DeliveryResponse): boolean {
    if (!this.currentUserId || !delivery.assignedToUserId) {
      return false;
    }

    return delivery.assignedToUserId.toLocaleLowerCase() === this.currentUserId.toLocaleLowerCase();
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar esta entrega.';
    }

    if (error.status === 404) {
      return 'Entrega no encontrada.';
    }

    return 'No fue posible cargar la entrega.';
  }

  private toActionErrorMessage(error: HttpErrorResponse, action: DriverDeliveryAction): string {
    if (error.status === 400) {
      return action === 'complete'
        ? 'Captura quien recibio la entrega.'
        : 'Captura el motivo de no entrega.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para cerrar esta entrega.';
    }

    if (error.status === 404) {
      return 'Entrega no encontrada.';
    }

    if (error.status === 409) {
      return 'La entrega no permite esta accion en su estado actual.';
    }

    return 'No fue posible actualizar la entrega.';
  }
}
