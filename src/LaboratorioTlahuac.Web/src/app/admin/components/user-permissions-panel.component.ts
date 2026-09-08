import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, effect, input, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import { AdminSecurityService } from '../admin-security.service';
import {
  AdminUserPermissionOverrideEffect,
  AdminUserPermissionOverrideRequest,
  AdminUserPermissionState,
  AdminUserPermissions
} from '../admin-security.models';

type PermissionSelection = 'Inherited' | AdminUserPermissionOverrideEffect;

interface PermissionGroup {
  key: string;
  label: string;
  permissions: AdminUserPermissionState[];
}

@Component({
  selector: 'app-user-permissions-panel',
  imports: [FormsModule],
  template: `
    <section class="admin-panel user-permissions-panel">
      <header>
        <div>
          <h2>Permisos individuales</h2>
          <p>El rol define la base. Usa excepciones solo cuando este usuario necesite un acceso diferente.</p>
        </div>
      </header>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando permisos efectivos...</p>
      } @else if (permissions(); as state) {
        <div class="permission-summary">
          <div>
            <strong>Roles</strong>
            <span>{{ roleNames() }}</span>
          </div>
          <div>
            <strong>Permisos efectivos</strong>
            <span>{{ effectivePermissionCount() }}</span>
          </div>
          <div>
            <strong>Excepciones</strong>
            <span>{{ overrideCount() }}</span>
          </div>
        </div>

        @if (isAdminUser()) {
          <p class="permission-help">
            Este usuario pertenece al rol Admin. Sus permisos individuales estan protegidos y no admiten excepciones.
          </p>
        } @else {
          <p class="permission-help">
            <strong>Heredado</strong> usa lo definido por los roles; <strong>Permitir</strong> agrega acceso y
            <strong>Denegar</strong> lo revoca para este usuario aunque el rol lo conceda.
          </p>
        }

        <div class="permission-groups">
          @for (group of permissionGroups(); track group.key) {
            <section class="permission-group">
              <h3>{{ group.label }}</h3>
              <div class="permission-options">
                @for (permissionState of group.permissions; track permissionState.permission.id) {
                  <article class="permission-row">
                    <div class="permission-copy">
                      <strong>{{ permissionState.permission.description }}</strong>
                      <small>{{ permissionState.permission.key }}</small>
                      <span class="permission-source">
                        @if (permissionState.sourceRoles.length > 0) {
                          Heredado de: {{ permissionState.sourceRoles.join(', ') }}
                        } @else {
                          Sin rol que lo conceda
                        }
                      </span>
                    </div>

                    <label class="permission-select">
                      <span>Configuracion</span>
                      <select
                        [ngModel]="selectionFor(permissionState.permission.id)"
                        (ngModelChange)="setSelection(permissionState.permission.id, $event)"
                        [disabled]="isAdminUser() || isSaving()"
                      >
                        <option value="Inherited">Heredado</option>
                        <option value="Allow">Permitir</option>
                        <option value="Deny">Denegar</option>
                      </select>
                    </label>

                    <span
                      class="effective-pill"
                      [class.allowed]="previewAllowed(permissionState)"
                      [class.denied]="!previewAllowed(permissionState)"
                    >
                      {{ previewAllowed(permissionState) ? 'Permitido' : 'Denegado' }}
                    </span>
                  </article>
                }
              </div>
            </section>
          }
        </div>

        @if (!isAdminUser()) {
          <div class="page-actions permission-actions">
            <button
              class="primary-button"
              type="button"
              [disabled]="isSaving() || !hasChanges()"
              (click)="save()"
            >
              {{ isSaving() ? 'Guardando...' : 'Guardar permisos individuales' }}
            </button>
            <button
              class="secondary-button"
              type="button"
              [disabled]="isSaving() || !hasChanges()"
              (click)="reset()"
            >
              Descartar cambios
            </button>
          </div>
        }
      }
    </section>
  `,
  styles: `
    .user-permissions-panel header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
    }

    .permission-summary {
      display: grid;
      grid-template-columns: repeat(3, minmax(0, 1fr));
      gap: 0.75rem;
      margin: 1rem 0;
    }

    .permission-summary > div {
      display: grid;
      gap: 0.25rem;
      padding: 0.8rem;
      border: 1px solid var(--border-color, #d7e2ec);
      border-radius: 0.75rem;
    }

    .permission-summary span,
    .permission-help,
    .permission-source,
    .permission-copy small {
      color: var(--text-muted, #52657a);
    }

    .permission-help {
      line-height: 1.5;
    }

    .permission-groups {
      display: grid;
      gap: 1rem;
      margin-top: 1rem;
    }

    .permission-group {
      border-top: 1px solid var(--border-color, #d7e2ec);
      padding-top: 1rem;
    }

    .permission-group h3 {
      margin: 0 0 0.75rem;
      font-size: 1rem;
    }

    .permission-options {
      display: grid;
      gap: 0.75rem;
    }

    .permission-row {
      display: grid;
      grid-template-columns: minmax(0, 1fr) minmax(150px, 190px) auto;
      align-items: center;
      gap: 0.9rem;
      padding: 0.75rem 0;
      border-bottom: 1px solid var(--border-color, #e6edf3);
    }

    .permission-copy {
      display: grid;
      gap: 0.15rem;
      min-width: 0;
    }

    .permission-copy small,
    .permission-source {
      overflow-wrap: anywhere;
    }

    .permission-select {
      display: grid;
      gap: 0.25rem;
    }

    .permission-select span {
      font-size: 0.82rem;
      font-weight: 600;
    }

    .permission-select select {
      min-height: 44px;
    }

    .effective-pill {
      border-radius: 999px;
      padding: 0.35rem 0.65rem;
      font-size: 0.82rem;
      font-weight: 700;
      white-space: nowrap;
    }

    .effective-pill.allowed {
      background: #dcfce7;
      color: #166534;
    }

    .effective-pill.denied {
      background: #fee2e2;
      color: #991b1b;
    }

    .permission-actions {
      margin-top: 1.25rem;
    }

    @media (max-width: 760px) {
      .permission-summary {
        grid-template-columns: 1fr;
      }

      .permission-row {
        grid-template-columns: 1fr;
      }

      .effective-pill {
        justify-self: start;
      }
    }
  `
})
export class UserPermissionsPanelComponent {
  readonly userId = input.required<string>();
  readonly permissions = signal<AdminUserPermissions | null>(null);
  readonly selections = signal<Map<string, PermissionSelection>>(new Map());
  readonly originalSelections = signal<Map<string, PermissionSelection>>(new Map());
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly isAdminUser = computed(() =>
    this.permissions()?.roles.some((role) => role.name.trim().toLocaleLowerCase() === 'admin') ?? false
  );

  readonly roleNames = computed(() =>
    this.permissions()?.roles.map((role) => role.name).join(', ') || '-'
  );

  readonly overrideCount = computed(() =>
    Array.from(this.selections().values()).filter((selection) => selection !== 'Inherited').length
  );

  readonly effectivePermissionCount = computed(() => {
    const state = this.permissions();

    if (!state) {
      return 0;
    }

    return state.permissions.filter((permission) => this.previewAllowed(permission)).length;
  });

  readonly permissionGroups = computed<PermissionGroup[]>(() => {
    const groups = new Map<string, AdminUserPermissionState[]>();

    for (const state of this.permissions()?.permissions ?? []) {
      const prefix = state.permission.key.split('.')[0] || 'other';
      const current = groups.get(prefix) ?? [];
      current.push(state);
      groups.set(prefix, current);
    }

    return Array.from(groups.entries())
      .sort(([left], [right]) => left.localeCompare(right))
      .map(([key, permissions]) => ({
        key,
        label: this.groupLabel(key),
        permissions: permissions.sort((left, right) => left.permission.key.localeCompare(right.permission.key))
      }));
  });

  readonly hasChanges = computed(() => !this.mapsEqual(this.selections(), this.originalSelections()));

  constructor(private readonly adminSecurityService: AdminSecurityService) {
    effect(() => {
      const id = this.userId();

      if (id) {
        this.load(id);
      }
    });
  }

  selectionFor(permissionId: string): PermissionSelection {
    return this.selections().get(permissionId) ?? 'Inherited';
  }

  setSelection(permissionId: string, selection: PermissionSelection): void {
    if (!['Inherited', 'Allow', 'Deny'].includes(selection)) {
      return;
    }

    const next = new Map(this.selections());
    next.set(permissionId, selection);
    this.selections.set(next);
    this.successMessage.set(null);
  }

  previewAllowed(state: AdminUserPermissionState): boolean {
    const selection = this.selectionFor(state.permission.id);

    if (selection === 'Allow') {
      return true;
    }

    if (selection === 'Deny') {
      return false;
    }

    return state.inherited;
  }

  reset(): void {
    this.selections.set(new Map(this.originalSelections()));
    this.successMessage.set(null);
  }

  save(): void {
    const current = this.permissions();

    if (!current || this.isAdminUser() || !this.hasChanges()) {
      return;
    }

    const overrides: AdminUserPermissionOverrideRequest[] = Array.from(this.selections().entries())
      .filter(([, selection]) => selection !== 'Inherited')
      .map(([permissionId, selection]) => ({
        permissionId,
        effect: selection as AdminUserPermissionOverrideEffect
      }));

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateUserPermissionOverrides(current.userId, overrides)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.applyState(updated);
          this.successMessage.set('Permisos individuales actualizados correctamente.');
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private load(userId: string): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .getUserPermissions(userId)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (state) => this.applyState(state),
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private applyState(state: AdminUserPermissions): void {
    const selections = new Map<string, PermissionSelection>();

    for (const permission of state.permissions) {
      selections.set(permission.permission.id, permission.overrideEffect ?? 'Inherited');
    }

    this.permissions.set(state);
    this.selections.set(new Map(selections));
    this.originalSelections.set(new Map(selections));
  }

  private mapsEqual(left: Map<string, PermissionSelection>, right: Map<string, PermissionSelection>): boolean {
    return left.size === right.size && Array.from(left.entries()).every(([key, value]) => right.get(key) === value);
  }

  private groupLabel(key: string): string {
    const labels: Record<string, string> = {
      catalog: 'Catalogo',
      customers: 'Clientes',
      deliveries: 'Entregas',
      inventory: 'Inventario',
      orders: 'Ordenes',
      payments: 'Pagos',
      reports: 'Reportes y dashboard',
      roles: 'Roles y seguridad',
      suppliers: 'Proveedores',
      users: 'Usuarios y seguridad'
    };

    return labels[key] ?? key.charAt(0).toUpperCase() + key.slice(1);
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar permisos de usuarios.';
    }

    if (error.status === 409) {
      return error.error?.title ?? 'Los permisos de este usuario estan protegidos.';
    }

    if (error.status === 400) {
      return 'La configuracion contiene un permiso o efecto invalido.';
    }

    return 'No fue posible cargar o guardar los permisos individuales.';
  }
}
