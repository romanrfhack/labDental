import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Observable, finalize, map, switchMap } from 'rxjs';

import { AdminSecurityService } from '../../../admin/admin-security.service';
import { AdminUserListItem } from '../../../admin/admin-security.models';
import { AuthService } from '../../../core/auth/auth.service';
import { DeliveryResponse, DeliveryStatus } from '../delivery.models';
import { DeliveryService } from '../delivery.service';

type DeliveryAction = 'create' | 'assign' | 'outForDelivery' | 'complete' | 'failed' | 'retry';

@Component({
  selector: 'app-delivery-admin-section',
  imports: [DatePipe, FormsModule],
  template: `
    <section class="admin-panel delivery-admin-panel">
      <header class="section-header">
        <div>
          <h2>Entrega</h2>
          <p>Seguimiento de asignación, salida y cierre de entrega.</p>
        </div>

        @if (delivery(); as delivery) {
          <span [class]="deliveryStatusClass(delivery.status)">
            {{ deliveryStatusLabel(delivery.status) }}
          </span>
        }
      </header>

      @if (!canViewDelivery) {
        <p class="empty-state">No tienes permiso para ver entregas.</p>
      } @else if (isLoading()) {
        <p class="loading-state">Cargando entrega...</p>
      } @else {
        @if (deliveryErrorMessage(); as message) {
          <p class="alert-error" role="alert">{{ message }}</p>
        }

        @if (successMessage(); as message) {
          <p class="alert-success" role="status">{{ message }}</p>
        }

        @if (delivery(); as delivery) {
          <div class="detail-grid">
            <div class="detail-item">
              <strong>Estado</strong>
              <span [class]="deliveryStatusClass(delivery.status)">
                {{ deliveryStatusLabel(delivery.status) }}
              </span>
            </div>
            <div class="detail-item">
              <strong>Repartidor</strong>
              <span>{{ delivery.assignedToUserFullName || '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Asignada</strong>
              <span>{{ delivery.assignedAtUtc ? (delivery.assignedAtUtc | date: 'medium') : '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Salida</strong>
              <span>{{ delivery.outForDeliveryAtUtc ? (delivery.outForDeliveryAtUtc | date: 'medium') : '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Entregada</strong>
              <span>{{ delivery.deliveredAtUtc ? (delivery.deliveredAtUtc | date: 'medium') : '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Fallida</strong>
              <span>{{ delivery.failedAtUtc ? (delivery.failedAtUtc | date: 'medium') : '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Recibió</strong>
              <span>{{ delivery.recipientName || '-' }}</span>
            </div>
            <div class="detail-item">
              <strong>Motivo de falla</strong>
              <span>{{ delivery.failedReason || '-' }}</span>
            </div>
          </div>

          @if (actionErrorMessage(); as message) {
            <p class="alert-error" role="alert">{{ message }}</p>
          }

          @if (hasAnyAction(delivery)) {
            <div class="delivery-actions">
              @if (canAssignDelivery && canAssignCurrentDelivery(delivery)) {
                <section class="delivery-action-panel">
                  <header>
                    <h3>Asignar repartidor</h3>
                  </header>

                  @if (driverWarningMessage(); as warning) {
                    <p class="delivery-warning">{{ warning }}</p>
                  }

                  @if (driverLoadErrorMessage(); as message) {
                    <p class="alert-error" role="alert">{{ message }}</p>
                  }

                  @if (isDriverLoading()) {
                    <p class="loading-state">Cargando repartidores...</p>
                  } @else {
                    <label class="form-field">
                      <span>Repartidor</span>
                      <select name="deliveryDriverUserId" [(ngModel)]="selectedDriverUserId">
                        <option value="">Selecciona repartidor</option>
                        @if (delivery.assignedToUserId && !driverCandidateExists(delivery.assignedToUserId)) {
                          <option [value]="delivery.assignedToUserId">
                            {{ delivery.assignedToUserFullName || 'Repartidor asignado' }}
                          </option>
                        }
                        @for (driver of driverCandidates(); track driver.id) {
                          <option [value]="driver.id">{{ driver.fullName }}</option>
                        }
                      </select>
                    </label>

                    @if (driverCandidates().length === 0 && !delivery.assignedToUserId && !driverLoadErrorMessage()) {
                      <p class="empty-state">No hay repartidores activos disponibles.</p>
                    }
                  }

                  <div class="page-actions">
                    <button
                      class="secondary-button"
                      type="button"
                      [disabled]="isActionBusy() || isDriverLoading() || !!driverLoadErrorMessage()"
                      (click)="assignDriver(delivery)"
                    >
                      {{ activeAction() === 'assign' ? 'Asignando...' : 'Asignar repartidor' }}
                    </button>
                  </div>
                </section>
              }

              @if (canUpdateDelivery && canMarkOutForDelivery(delivery)) {
                <section class="delivery-action-panel">
                  <header>
                    <h3>Salida a entrega</h3>
                  </header>
                  <div class="page-actions">
                    <button
                      class="secondary-button"
                      type="button"
                      [disabled]="isActionBusy()"
                      (click)="markOutForDelivery(delivery)"
                    >
                      {{ activeAction() === 'outForDelivery' ? 'Registrando...' : 'Marcar salida' }}
                    </button>
                  </div>
                </section>
              }

              @if (canUpdateDelivery && canRetryDelivery(delivery)) {
                <section class="delivery-action-panel">
                  <header>
                    <h3>Reintentar entrega</h3>
                    <p>La entrega volverá a marcarse como En ruta.</p>
                  </header>
                  <div class="page-actions">
                    <button
                      class="secondary-button"
                      type="button"
                      [disabled]="isActionBusy()"
                      (click)="retryDelivery(delivery)"
                    >
                      {{ activeAction() === 'retry' ? 'Reintentando...' : 'Reintentar entrega' }}
                    </button>
                  </div>
                </section>
              }

              @if (canCompleteDelivery && canMarkDelivered(delivery)) {
                <section class="delivery-action-panel">
                  <header>
                    <h3>Entrega completada</h3>
                  </header>
                  <label class="form-field">
                    <span>Recibió</span>
                    <input
                      name="deliveryRecipientName"
                      type="text"
                      maxlength="150"
                      [(ngModel)]="recipientName"
                    />
                  </label>
                  <div class="page-actions">
                    <button
                      class="primary-button"
                      type="button"
                      [disabled]="isActionBusy()"
                      (click)="markDelivered(delivery)"
                    >
                      {{ activeAction() === 'complete' ? 'Guardando...' : 'Marcar entregada' }}
                    </button>
                  </div>
                </section>
              }

              @if (canCompleteDelivery && canMarkFailed(delivery)) {
                <section class="delivery-action-panel">
                  <header>
                    <h3>No entregada</h3>
                  </header>
                  <label class="form-field">
                    <span>Motivo</span>
                    <textarea
                      name="deliveryFailedReason"
                      maxlength="1000"
                      [(ngModel)]="failedReason"
                    ></textarea>
                  </label>
                  <div class="page-actions">
                    <button
                      class="danger-button"
                      type="button"
                      [disabled]="isActionBusy()"
                      (click)="markFailed(delivery)"
                    >
                      {{ activeAction() === 'failed' ? 'Guardando...' : 'Marcar no entregada' }}
                    </button>
                  </div>
                </section>
              }
            </div>
          } @else {
            <p class="empty-state">No hay acciones disponibles para esta entrega.</p>
          }
        } @else {
          <p class="empty-state">Esta orden no tiene entrega registrada.</p>

          @if (isWorkOrderCancelled) {
            <p class="empty-state">La orden cancelada no permite crear entrega.</p>
          } @else if (canCreateDelivery) {
            @if (actionErrorMessage(); as message) {
              <p class="alert-error" role="alert">{{ message }}</p>
            }

            <div class="page-actions">
              <button
                class="primary-button"
                type="button"
                [disabled]="isActionBusy()"
                (click)="createDelivery()"
              >
                {{ activeAction() === 'create' ? 'Creando...' : 'Crear entrega' }}
              </button>
            </div>
          } @else {
            <p class="empty-state">No tienes permiso para crear entregas.</p>
          }
        }
      }
    </section>
  `,
  styles: [`
    .delivery-admin-panel {
      align-content: start;
    }

    .delivery-status {
      background: var(--color-info-100);
      color: var(--color-info-700);
    }

    .delivery-status.Assigned,
    .delivery-status.OutForDelivery {
      background: var(--color-warning-100);
      color: var(--color-warning-700);
    }

    .delivery-status.Delivered {
      background: var(--color-success-100);
      color: var(--color-success-700);
    }

    .delivery-status.FailedDelivery {
      background: var(--color-danger-100);
      color: var(--color-danger-700);
    }

    .delivery-actions {
      display: grid;
      gap: var(--space-4);
    }

    .delivery-action-panel {
      border: 1px solid var(--color-neutral-100);
      border-radius: var(--radius-sm);
      display: grid;
      gap: var(--space-4);
      padding: var(--space-4);
    }

    .delivery-action-panel header {
      display: grid;
      gap: 4px;
    }

    .delivery-action-panel h3 {
      margin: 0;
    }

    .delivery-action-panel p {
      color: var(--color-neutral-600);
      line-height: 1.45;
      margin: 0;
    }

    .delivery-warning {
      background: var(--color-warning-100);
      border: 1px solid #fde68a;
      border-radius: var(--radius-sm);
      color: var(--color-warning-700);
      margin: 0;
      padding: var(--space-3) var(--space-4);
    }

    textarea {
      min-height: 96px;
      resize: vertical;
    }
  `]
})
export class DeliveryAdminSectionComponent implements OnInit {
  @Input({ required: true }) workOrderId!: string;
  @Input() isWorkOrderCancelled = false;
  @Output() readonly deliveryChanged = new EventEmitter<void>();

  readonly delivery = signal<DeliveryResponse | null>(null);
  readonly driverCandidates = signal<AdminUserListItem[]>([]);
  readonly isLoading = signal(false);
  readonly isDriverLoading = signal(false);
  readonly activeAction = signal<DeliveryAction | null>(null);
  readonly deliveryErrorMessage = signal<string | null>(null);
  readonly actionErrorMessage = signal<string | null>(null);
  readonly driverLoadErrorMessage = signal<string | null>(null);
  readonly driverWarningMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  selectedDriverUserId = '';
  recipientName = '';
  failedReason = '';

  constructor(
    private readonly deliveryService: DeliveryService,
    private readonly adminSecurityService: AdminSecurityService,
    private readonly authService: AuthService
  ) {}

  get canViewDelivery(): boolean {
    return this.authService.hasPermission('deliveries.view');
  }

  get canCreateDelivery(): boolean {
    return this.authService.hasPermission('deliveries.assign');
  }

  get canAssignDelivery(): boolean {
    return this.authService.hasPermission('deliveries.assign');
  }

  get canUpdateDelivery(): boolean {
    return this.authService.hasPermission('deliveries.update');
  }

  get canCompleteDelivery(): boolean {
    return this.authService.hasPermission('deliveries.complete');
  }

  ngOnInit(): void {
    this.loadDelivery();
    this.loadDriverCandidates();
  }

  createDelivery(): void {
    if (this.isWorkOrderCancelled || this.isActionBusy()) {
      return;
    }

    this.runDeliveryAction(
      'create',
      () => this.deliveryService.createForWorkOrder(this.workOrderId, { deliveryNotes: null }),
      'Entrega creada correctamente.'
    );
  }

  assignDriver(delivery: DeliveryResponse): void {
    const selectedDriverUserId = this.selectedDriverUserId.trim();

    if (!selectedDriverUserId) {
      this.actionErrorMessage.set('Selecciona un repartidor.');
      return;
    }

    if (
      delivery.assignedToUserId
      && delivery.assignedToUserId !== selectedDriverUserId
      && !window.confirm('Cambiar el repartidor asignado?')
    ) {
      return;
    }

    this.runDeliveryAction(
      'assign',
      () => this.deliveryService.assign(delivery.id, {
        assignedToUserId: selectedDriverUserId,
        deliveryNotes: null
      }),
      'Repartidor asignado correctamente.'
    );
  }

  markOutForDelivery(delivery: DeliveryResponse): void {
    this.runDeliveryAction(
      'outForDelivery',
      () => this.deliveryService.markOutForDelivery(delivery.id, { deliveryNotes: null }),
      'Salida registrada correctamente.'
    );
  }

  markDelivered(delivery: DeliveryResponse): void {
    const recipientName = this.recipientName.trim();

    if (!recipientName) {
      this.actionErrorMessage.set('Captura quién recibió la entrega.');
      return;
    }

    if (!window.confirm('Marcar esta entrega como entregada?')) {
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
    const failedReason = this.failedReason.trim();

    if (!failedReason) {
      this.actionErrorMessage.set('Captura el motivo de no entrega.');
      return;
    }

    if (!window.confirm('Marcar esta entrega como no entregada?')) {
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
    if (!window.confirm('Reintentar esta entrega y volver a ponerla en ruta?')) {
      return;
    }

    this.runDeliveryAction(
      'retry',
      () => this.deliveryService.retry(delivery.id, { deliveryNotes: null }),
      'Entrega marcada como En ruta.'
    );
  }

  hasAnyAction(delivery: DeliveryResponse): boolean {
    return (this.canAssignDelivery && this.canAssignCurrentDelivery(delivery))
      || (this.canUpdateDelivery && this.canMarkOutForDelivery(delivery))
      || (this.canUpdateDelivery && this.canRetryDelivery(delivery))
      || (this.canCompleteDelivery && this.canMarkDelivered(delivery))
      || (this.canCompleteDelivery && this.canMarkFailed(delivery));
  }

  canAssignCurrentDelivery(delivery: DeliveryResponse): boolean {
    return delivery.status === 'PendingAssignment' || delivery.status === 'Assigned';
  }

  canMarkOutForDelivery(delivery: DeliveryResponse): boolean {
    return delivery.status === 'Assigned';
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

  deliveryStatusClass(status: DeliveryStatus): string {
    return `status-pill delivery-status ${status}`;
  }

  deliveryStatusLabel(status: DeliveryStatus): string {
    switch (status) {
      case 'PendingAssignment':
        return 'Pendiente de asignación';
      case 'Assigned':
        return 'Asignada';
      case 'OutForDelivery':
        return 'En reparto';
      case 'Delivered':
        return 'Entregada';
      case 'FailedDelivery':
        return 'No entregada';
      default:
        return status;
    }
  }

  driverCandidateExists(userId: string): boolean {
    return this.driverCandidates().some((driver) => driver.id === userId);
  }

  private loadDelivery(clearCurrent = true): void {
    if (!this.canViewDelivery) {
      return;
    }

    this.isLoading.set(true);
    this.deliveryErrorMessage.set(null);

    if (clearCurrent) {
      this.delivery.set(null);
    }

    this.deliveryService
      .getByWorkOrderId(this.workOrderId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (delivery) => this.setDelivery(delivery),
        error: (error: HttpErrorResponse) => {
          if (error.status === 404) {
            this.delivery.set(null);
            this.selectedDriverUserId = '';
            return;
          }

          this.deliveryErrorMessage.set(this.toDeliveryLoadErrorMessage(error));
        }
      });
  }

  private loadDriverCandidates(): void {
    if (!this.canAssignDelivery) {
      return;
    }

    this.driverLoadErrorMessage.set(null);
    this.driverWarningMessage.set(null);

    if (!this.authService.hasPermission('users.manage')) {
      this.driverLoadErrorMessage.set('No se pudieron cargar usuarios disponibles para asignar.');
      return;
    }

    this.isDriverLoading.set(true);

    this.getDriverCandidatesRequest()
      .pipe(finalize(() => this.isDriverLoading.set(false)))
      .subscribe({
        next: (users) => {
          this.driverCandidates.set(users);
          this.ensureSelectedDriverStillValid();
        },
        error: (error: HttpErrorResponse) => {
          this.driverLoadErrorMessage.set(this.toDriverLoadErrorMessage(error));
          this.driverCandidates.set([]);
        }
      });
  }

  private getDriverCandidatesRequest(): Observable<AdminUserListItem[]> {
    if (!this.authService.hasPermission('roles.manage')) {
      return this.adminSecurityService
        .listUsers({ isActive: true, page: 1, pageSize: 100 })
        .pipe(map((response) => this.filterDriversOrFallback(response.items)));
    }

    return this.adminSecurityService.listRoles().pipe(
      switchMap((roles) => {
        const driverRole = roles.find((role) => this.normalizeRoleName(role.name) === 'repartidor');

        if (!driverRole) {
          this.driverWarningMessage.set('No se pudo filtrar por rol Repartidor; se muestran usuarios activos.');

          return this.adminSecurityService
            .listUsers({ isActive: true, page: 1, pageSize: 100 })
            .pipe(map((response) => response.items));
        }

        return this.adminSecurityService
          .listUsers({ isActive: true, roleId: driverRole.id, page: 1, pageSize: 100 })
          .pipe(map((response) => response.items));
      })
    );
  }

  private filterDriversOrFallback(users: AdminUserListItem[]): AdminUserListItem[] {
    const driverUsers = users.filter((user) => this.userHasDriverRole(user));

    if (driverUsers.length > 0) {
      return driverUsers;
    }

    this.driverWarningMessage.set('No se pudo filtrar por rol Repartidor; se muestran usuarios activos.');

    return users;
  }

  private runDeliveryAction(
    action: DeliveryAction,
    request: () => Observable<DeliveryResponse>,
    successMessage: string
  ): void {
    if (this.isActionBusy()) {
      return;
    }

    this.activeAction.set(action);
    this.actionErrorMessage.set(null);
    this.deliveryErrorMessage.set(null);
    this.successMessage.set(null);

    request()
      .pipe(finalize(() => this.activeAction.set(null)))
      .subscribe({
        next: (delivery) => {
          this.setDelivery(delivery);
          this.successMessage.set(successMessage);
          this.recipientName = '';
          this.failedReason = '';
          this.loadDelivery(false);
          this.deliveryChanged.emit();
        },
        error: (error: HttpErrorResponse) => {
          this.actionErrorMessage.set(this.toDeliveryActionErrorMessage(error, action));
        }
      });
  }

  private setDelivery(delivery: DeliveryResponse): void {
    this.delivery.set(delivery);
    this.selectedDriverUserId = delivery.assignedToUserId ?? this.selectedDriverUserId;
  }

  private ensureSelectedDriverStillValid(): void {
    if (!this.selectedDriverUserId) {
      return;
    }

    if (!this.driverCandidateExists(this.selectedDriverUserId)) {
      this.selectedDriverUserId = this.delivery()?.assignedToUserId ?? '';
    }
  }

  private userHasDriverRole(user: AdminUserListItem): boolean {
    return user.roles.some((role) => this.normalizeRoleName(role.name) === 'repartidor');
  }

  private normalizeRoleName(value: string): string {
    return value
      .normalize('NFD')
      .replace(/[\u0300-\u036f]/g, '')
      .trim()
      .toLocaleLowerCase('es-MX');
  }

  private toDeliveryLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para ver entregas.';
    }

    return 'No fue posible cargar la entrega.';
  }

  private toDriverLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para cargar usuarios disponibles.';
    }

    return 'No fue posible cargar usuarios disponibles.';
  }

  private toDeliveryActionErrorMessage(error: HttpErrorResponse, action: DeliveryAction): string {
    if (action === 'retry') {
      if (error.status === 403) {
        return 'No tienes permiso para realizar esta acción.';
      }

      if (error.status === 404) {
        return 'Entrega no encontrada.';
      }

      return 'No se pudo reintentar la entrega.';
    }

    if (error.status === 400) {
      if (action === 'complete') {
        return 'Captura quién recibió la entrega.';
      }

      if (action === 'failed') {
        return 'Captura el motivo de no entrega.';
      }

      return 'Revisa los datos de la entrega.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para realizar esta acción.';
    }

    if (error.status === 404) {
      return 'Entrega no encontrada.';
    }

    if (error.status === 409) {
      return 'La entrega no permite esta acción en su estado actual.';
    }

    return 'No fue posible actualizar la entrega.';
  }
}
