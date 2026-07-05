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

type DriverDeliveryAction = 'complete' | 'failed' | 'retry';

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
          <div class="driver-hero-copy">
            <span class="driver-section-label">Cliente</span>
            <h2>{{ delivery.customerDisplayName || 'Cliente sin nombre' }}</h2>
            <div class="driver-hero-meta">
              <span>Folio {{ delivery.orderNumber }}</span>
              <span>Entrega {{ formatDateOnly(delivery.deliveryDate) }}</span>
            </div>
          </div>
          <span [class]="deliveryStatusClass(delivery.status)">{{ delivery.statusLabel }}</span>
        </section>

        @if (hasRouteDetails(delivery)) {
          <section class="driver-route-panel" aria-label="Datos de ruta y contacto">
            @if (delivery.customerAddress) {
              <article class="driver-route-item">
                <div>
                  <strong>Direccion</strong>
                  <span>{{ delivery.customerAddress }}</span>
                </div>
                <a
                  class="secondary-button"
                  [href]="mapHref(delivery.customerAddress)"
                  target="_blank"
                  rel="noopener"
                >
                  Abrir mapa
                </a>
              </article>
            }

            @if (hasContact(delivery)) {
              <article class="driver-route-item">
                <div>
                  <strong>Contacto</strong>
                  @if (delivery.customerContactName) {
                    <span>{{ delivery.customerContactName }}</span>
                  }
                </div>
                @if (hasPhone(delivery.customerPhone) || hasWhatsApp(delivery.customerWhatsApp)) {
                  <div class="driver-route-actions">
                    @if (hasPhone(delivery.customerPhone)) {
                      <a class="secondary-button" [href]="phoneHref(delivery.customerPhone)">
                        Llamar
                      </a>
                    }
                    @if (hasWhatsApp(delivery.customerWhatsApp)) {
                      <a
                        class="secondary-button"
                        [href]="whatsAppHref(delivery.customerWhatsApp)"
                        target="_blank"
                        rel="noopener"
                      >
                        WhatsApp
                      </a>
                    }
                  </div>
                }
              </article>
            }
          </section>
        }

        <section class="driver-detail-grid" aria-label="Datos de entrega">
          <div class="driver-detail-item">
            <strong>Folio</strong>
            <span>{{ delivery.orderNumber }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Fecha de entrega</strong>
            <span>{{ formatDateOnly(delivery.deliveryDate) }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Estado entrega</strong>
            <span>{{ delivery.statusLabel }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Estado de orden</strong>
            <span>{{ delivery.workOrderStatusLabel || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Paciente</strong>
            <span>{{ delivery.patientName || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Referencia</strong>
            <span>{{ delivery.referenceNumber || '-' }}</span>
          </div>
          <div class="driver-detail-item driver-detail-wide">
            <strong>Trabajo</strong>
            <span>{{ delivery.workSummary || '-' }}</span>
          </div>
          <div class="driver-detail-item">
            <strong>Doctor interno</strong>
            <span>{{ delivery.internalDoctorFullName || '-' }}</span>
          </div>
          @if (delivery.deliveryNotes) {
            <div class="driver-detail-item driver-detail-wide">
              <strong>Indicaciones</strong>
              <span>{{ delivery.deliveryNotes }}</span>
            </div>
          }
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
          <section class="driver-action-area">
            <header class="driver-action-heading">
              <span class="driver-section-label">Accion operativa</span>
              <h2>{{ actionHeading(delivery) }}</h2>
              <p>{{ actionDescription(delivery) }}</p>
            </header>

            <div class="driver-action-stack">
              @if (canRetryDelivery(delivery)) {
                <section class="driver-action-panel is-primary">
                  <header>
                    <h3>Reintentar entrega</h3>
                    <p>La entrega volvera a marcarse como En ruta.</p>
                  </header>
                  <button class="primary-button" type="button" [disabled]="isActionBusy()" (click)="retryDelivery(delivery)">
                    {{ activeAction() === 'retry' ? 'Reintentando...' : 'Reintentar entrega' }}
                  </button>
                </section>
              }

              @if (canMarkDelivered(delivery)) {
                <form class="driver-action-panel is-primary" novalidate (ngSubmit)="markDelivered(delivery)">
                  <header>
                    <h3>Marcar entregada</h3>
                  </header>
                  <label class="form-field">
                    <span>Nombre de quien recibio</span>
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
                <form class="driver-action-panel is-danger" novalidate (ngSubmit)="markFailed(delivery)">
                  <header>
                    <h3>Marcar no entregada</h3>
                  </header>
                  <label class="form-field">
                    <span>Motivo de no entrega</span>
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
            </div>
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
    .driver-route-item,
    .driver-timeline-panel,
    .driver-action-area,
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

    .driver-hero-copy {
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
      font-size: 1.48rem;
      line-height: 1.12;
      margin: 0;
      overflow-wrap: anywhere;
    }

    .driver-hero-meta {
      color: var(--color-neutral-600);
      display: flex;
      flex-wrap: wrap;
      font-size: 0.9rem;
      font-weight: 800;
      gap: var(--space-2);
    }

    .driver-hero-meta span {
      background: rgba(233, 247, 255, 0.72);
      border-radius: var(--radius-pill);
      padding: 5px 9px;
    }

    .driver-route-panel {
      display: grid;
      gap: var(--space-4);
      grid-template-columns: repeat(2, minmax(0, 1fr));
    }

    .driver-route-item {
      align-content: start;
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .driver-route-item > div:first-child {
      display: grid;
      gap: 6px;
      min-width: 0;
    }

    .driver-route-item strong,
    .driver-detail-item strong {
      color: var(--color-neutral-700);
      font-size: 0.84rem;
    }

    .driver-route-item span,
    .driver-detail-item span {
      color: var(--color-neutral-800);
      overflow-wrap: anywhere;
    }

    .driver-route-actions {
      display: flex;
      flex-wrap: wrap;
      gap: var(--space-2);
    }

    .driver-route-actions .secondary-button,
    .driver-route-item > .secondary-button {
      min-height: 46px;
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
    .driver-action-heading h2,
    .driver-action-panel h3 {
      margin: 0;
    }

    .driver-timeline-panel h2,
    .driver-action-heading h2 {
      font-size: 1.05rem;
    }

    .driver-action-panel h3 {
      font-size: 1rem;
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

    .driver-action-area {
      display: grid;
      gap: var(--space-4);
      padding: var(--space-5);
    }

    .driver-action-heading {
      display: grid;
      gap: var(--space-2);
    }

    .driver-action-heading p,
    .driver-action-panel p {
      color: var(--color-neutral-600);
      line-height: 1.45;
      margin: 0;
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

    .driver-action-panel.is-primary {
      border-color: rgba(84, 177, 232, 0.66);
    }

    .driver-action-panel.is-danger {
      border-color: rgba(254, 202, 202, 0.9);
    }

    .driver-action-panel header {
      display: grid;
      gap: var(--space-2);
    }

    .driver-action-panel textarea {
      min-height: 112px;
      resize: vertical;
    }

    .driver-action-panel button {
      min-height: 48px;
      width: 100%;
    }

    .driver-action-panel button.danger-button[type='submit'] {
      background: linear-gradient(135deg, #dc2626, #b91c1c);
      color: var(--color-neutral-0);
    }

    @media (max-width: 760px) {
      .driver-detail-hero {
        display: grid;
      }

      .driver-route-panel,
      .driver-detail-grid,
      .driver-timeline-panel dl,
      .driver-action-stack {
        grid-template-columns: 1fr;
      }

      .driver-route-actions .secondary-button,
      .driver-route-item > .secondary-button {
        width: 100%;
      }
    }

    @media (max-width: 640px) {
      .driver-detail-hero,
      .driver-route-item,
      .driver-timeline-panel,
      .driver-action-area,
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

  retryDelivery(delivery: DeliveryResponse): void {
    if (!this.canCompleteDelivery || !this.canRetryDelivery(delivery) || this.isActionBusy()) {
      return;
    }

    this.runDeliveryAction(
      'retry',
      () => this.deliveryService.retry(delivery.id, { deliveryNotes: null }),
      'Entrega marcada como En ruta.'
    );
  }

  canShowCloseActions(delivery: DeliveryResponse): boolean {
    return this.canRetryDelivery(delivery) || this.canMarkDelivered(delivery) || this.canMarkFailed(delivery);
  }

  canRetryDelivery(delivery: DeliveryResponse): boolean {
    return delivery.status === 'FailedDelivery';
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

  actionHeading(delivery: DeliveryResponse): string {
    if (this.canRetryDelivery(delivery)) {
      return 'Reintentar entrega';
    }

    if (this.canMarkDelivered(delivery)) {
      return 'Marcar entregada';
    }

    if (this.canMarkFailed(delivery)) {
      return 'Marcar no entregada';
    }

    return 'Sin acciones disponibles';
  }

  actionDescription(delivery: DeliveryResponse): string {
    if (this.canRetryDelivery(delivery)) {
      return 'Vuelve a poner esta entrega En ruta para intentar cerrarla nuevamente.';
    }

    if (this.canMarkDelivered(delivery)) {
      return 'Confirma quien recibio la entrega o registra el motivo si no se pudo entregar.';
    }

    if (this.canMarkFailed(delivery)) {
      return 'La entrega aun no esta En ruta; solo puede registrarse como no entregada.';
    }

    return 'El estado actual no requiere accion del repartidor.';
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

  hasRouteDetails(delivery: DeliveryResponse): boolean {
    return !!delivery.customerAddress || this.hasContact(delivery);
  }

  hasContact(delivery: DeliveryResponse): boolean {
    return !!delivery.customerContactName || this.hasPhone(delivery.customerPhone) || this.hasWhatsApp(delivery.customerWhatsApp);
  }

  hasPhone(value: string | null): boolean {
    return !!value?.trim();
  }

  hasWhatsApp(value: string | null): boolean {
    return this.phoneDigits(value).length > 0;
  }

  phoneHref(phone: string | null): string {
    const trimmed = phone?.trim() ?? '';
    const digits = this.phoneDigits(trimmed);

    if (!digits) {
      return `tel:${trimmed.replace(/\s/g, '')}`;
    }

    return `tel:${trimmed.startsWith('+') ? '+' : ''}${digits}`;
  }

  whatsAppHref(whatsApp: string | null): string {
    return `https://wa.me/${this.phoneDigits(whatsApp)}`;
  }

  mapHref(address: string | null): string {
    return `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(address?.trim() ?? '')}`;
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

  private phoneDigits(value: string | null): string {
    return (value ?? '').replace(/\D/g, '');
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
    if (action === 'retry') {
      if (error.status === 403) {
        return 'No tienes permiso para cerrar esta entrega.';
      }

      if (error.status === 404) {
        return 'Entrega no encontrada.';
      }

      return 'No se pudo reintentar la entrega.';
    }

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
