import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { finalize } from 'rxjs';

import { AdminPermission, AdminRoleDetail, AdminRoleListItem } from '../../admin-security.models';
import { AdminSecurityService } from '../../admin-security.service';

interface PermissionGroup {
  key: string;
  label: string;
  permissions: AdminPermission[];
}

@Component({
  selector: 'app-roles-page',
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Roles</h1>
          <p>Configura los permisos heredados por todos los usuarios asignados a cada rol.</p>
        </div>
        <span class="readonly-note">Permisos por rol</span>
      </header>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando roles...</p>
      } @else if (roles().length === 0) {
        <p class="empty-state">No hay roles configurados.</p>
      } @else {
        <div class="admin-role-layout permission-admin-layout">
          <div>
            <div class="table-scroll admin-roles-table-scroll">
              <table class="data-table">
                <thead>
                  <tr>
                    <th>Rol</th>
                    <th>Tipo</th>
                    <th>Usuarios</th>
                    <th>Permisos</th>
                    <th>Acciones</th>
                  </tr>
                </thead>
                <tbody>
                  @for (role of roles(); track role.id) {
                    <tr>
                      <td>
                        <strong>{{ role.name }}</strong>
                        <small class="text-muted">{{ role.description }}</small>
                      </td>
                      <td>{{ role.isSystem ? 'Sistema' : 'Operativo' }}</td>
                      <td>{{ role.userCount }}</td>
                      <td>{{ role.permissionCount }}</td>
                      <td>
                        <button class="secondary-button" type="button" (click)="selectRole(role)">
                          {{ role.isPermissionEditingLocked ? 'Ver permisos' : 'Configurar' }}
                        </button>
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>

            <div class="admin-mobile-list">
              @for (role of roles(); track role.id) {
                <article class="admin-card">
                  <header>
                    <div>
                      <strong>{{ role.name }}</strong>
                      <span>{{ role.description }}</span>
                    </div>
                    <span class="status-pill active">{{ role.isSystem ? 'Sistema' : 'Operativo' }}</span>
                  </header>
                  <dl>
                    <div>
                      <dt>Usuarios</dt>
                      <dd>{{ role.userCount }}</dd>
                    </div>
                    <div>
                      <dt>Permisos</dt>
                      <dd>{{ role.permissionCount }}</dd>
                    </div>
                  </dl>
                  <button class="secondary-button" type="button" (click)="selectRole(role)">
                    {{ role.isPermissionEditingLocked ? 'Ver permisos' : 'Configurar' }}
                  </button>
                </article>
              }
            </div>
          </div>

          <aside class="admin-panel permission-editor-panel">
            @if (selectedRole(); as role) {
              <header class="permission-editor-header">
                <div>
                  <h2>{{ role.name }}</h2>
                  <p>{{ role.description }}</p>
                </div>
                @if (role.isPermissionEditingLocked) {
                  <span class="permission-lock-badge">Protegido</span>
                } @else {
                  <span class="permission-edit-badge">Editable</span>
                }
              </header>

              <dl class="detail-grid">
                <div class="detail-item">
                  <strong>Tipo</strong>
                  <span>{{ role.isSystem ? 'Sistema' : 'Operativo' }}</span>
                </div>
                <div class="detail-item">
                  <strong>Usuarios activos</strong>
                  <span>{{ role.activeUserCount }} de {{ role.userCount }}</span>
                </div>
              </dl>

              @if (role.isPermissionEditingLocked) {
                <p class="permission-guidance">
                  El rol Admin conserva todos los permisos del sistema. No puede reducirse desde esta pantalla.
                </p>
                <ul class="permission-list">
                  @for (permission of role.permissions; track permission.id) {
                    <li>
                      <strong>{{ permissionLabel(permission.key) }}</strong>
                      <span>{{ permission.description }}</span>
                      <small class="text-muted">{{ permission.key }}</small>
                    </li>
                  }
                </ul>
              } @else {
                <p class="permission-guidance">
                  Los cambios se aplican a los {{ role.userCount }} usuario(s) asignados a este rol y son efectivos en
                  su siguiente solicitud autenticada.
                </p>

                <div class="permission-groups">
                  @for (group of permissionGroups(role.availablePermissions); track group.key) {
                    <fieldset class="admin-fieldset permission-group">
                      <legend>{{ group.label }}</legend>
                      <div class="permission-checkbox-list">
                        @for (permission of group.permissions; track permission.id) {
                          <label class="permission-check-row">
                            <input
                              type="checkbox"
                              [checked]="isPermissionSelected(permission.id)"
                              [disabled]="isSaving()"
                              (change)="togglePermission(permission.id, $event)"
                            />
                            <span>
                              <strong>{{ permissionLabel(permission.key) }}</strong>
                              <small>{{ permission.description }}</small>
                              <code>{{ permission.key }}</code>
                            </span>
                          </label>
                        }
                      </div>
                    </fieldset>
                  }
                </div>

                <div class="page-actions">
                  <button class="primary-button" type="button" [disabled]="isSaving()" (click)="savePermissions(role)">
                    {{ isSaving() ? 'Guardando...' : 'Guardar permisos' }}
                  </button>
                  <button class="ghost-button" type="button" [disabled]="isSaving()" (click)="resetPermissions(role)">
                    Descartar cambios
                  </button>
                </div>
              }
            } @else {
              <p class="empty-state">Selecciona un rol para ver o configurar sus permisos.</p>
            }
          </aside>
        </div>
      }
    </section>
  `
})
export class RolesPageComponent implements OnInit {
  readonly roles = signal<AdminRoleListItem[]>([]);
  readonly selectedRole = signal<AdminRoleDetail | null>(null);
  readonly selectedPermissionIds = signal<string[]>([]);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  constructor(private readonly adminSecurityService: AdminSecurityService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  selectRole(role: AdminRoleListItem): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService.getRoleById(role.id).subscribe({
      next: (detail) => {
        this.selectedRole.set(detail);
        this.resetPermissions(detail);
      },
      error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
    });
  }

  isPermissionSelected(permissionId: string): boolean {
    return this.selectedPermissionIds().includes(permissionId);
  }

  togglePermission(permissionId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;

    this.selectedPermissionIds.update((current) => {
      const next = new Set(current);

      if (checked) {
        next.add(permissionId);
      } else {
        next.delete(permissionId);
      }

      return [...next];
    });
  }

  resetPermissions(role: AdminRoleDetail): void {
    this.selectedPermissionIds.set(role.permissions.map((permission) => permission.id));
  }

  savePermissions(role: AdminRoleDetail): void {
    if (role.isPermissionEditingLocked) {
      return;
    }

    const confirmation = role.userCount === 0
      ? `Guardar la configuracion de permisos del rol ${role.name}?`
      : `Guardar permisos de ${role.name}? El cambio afectara a ${role.userCount} usuario(s).`;

    if (!window.confirm(confirmation)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateRolePermissions(role.id, this.selectedPermissionIds())
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.selectedRole.set(updated);
          this.resetPermissions(updated);
          this.roles.update((roles) =>
            roles.map((current) =>
              current.id === updated.id
                ? {
                    ...current,
                    permissionCount: updated.permissions.length,
                    permissions: updated.permissions,
                    isPermissionEditingLocked: updated.isPermissionEditingLocked
                  }
                : current
            )
          );
          this.successMessage.set(`Permisos del rol ${updated.name} actualizados correctamente.`);
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  permissionGroups(permissions: AdminPermission[]): PermissionGroup[] {
    const groups = new Map<string, AdminPermission[]>();

    for (const permission of permissions) {
      const key = permission.key.split('.')[0] || 'other';
      const group = groups.get(key) ?? [];
      group.push(permission);
      groups.set(key, group);
    }

    return [...groups.entries()]
      .sort(([left], [right]) => this.groupLabel(left).localeCompare(this.groupLabel(right), 'es'))
      .map(([key, groupPermissions]) => ({
        key,
        label: this.groupLabel(key),
        permissions: [...groupPermissions].sort((left, right) => left.key.localeCompare(right.key))
      }));
  }

  permissionLabel(permissionKey: string): string {
    const action = permissionKey.split('.')[1] ?? permissionKey;
    const labels: Record<string, string> = {
      adjust: 'Ajustar',
      assign: 'Asignar',
      cancel: 'Cancelar',
      changeStatus: 'Cambiar estado',
      complete: 'Completar',
      create: 'Crear',
      delete: 'Eliminar',
      edit: 'Editar',
      manage: 'Administrar',
      update: 'Actualizar',
      view: 'Ver'
    };

    return labels[action] ?? action;
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
      roles: 'Roles',
      suppliers: 'Proveedores',
      users: 'Usuarios'
    };

    return labels[key] ?? key;
  }

  private loadRoles(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.adminSecurityService
      .listRoles()
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (roles) => {
          this.roles.set(roles);

          const driverRole = roles.find((role) => role.name === 'Repartidor');
          const firstRole = driverRole ?? roles[0];

          if (firstRole) {
            this.selectRole(firstRole);
          }
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar roles.';
    }

    if (error.status === 409) {
      return this.getProblemTitle(error) ?? 'El cambio entra en conflicto con la configuracion de seguridad.';
    }

    if (error.status === 400) {
      return 'Revisa la seleccion de permisos.';
    }

    return 'No fue posible cargar o actualizar roles y permisos.';
  }

  private getProblemTitle(error: HttpErrorResponse): string | null {
    if (error.error && typeof error.error === 'object' && 'title' in error.error) {
      const title = (error.error as { title?: unknown }).title;
      return typeof title === 'string' ? title : null;
    }

    return null;
  }
}
