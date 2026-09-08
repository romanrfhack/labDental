import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { finalize, forkJoin } from 'rxjs';

import { AdminSecurityService } from '../../admin-security.service';
import { AdminPermission, AdminRoleDetail, AdminRoleListItem } from '../../admin-security.models';

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
          <p>Configura los permisos base que heredan automaticamente los usuarios de cada rol.</p>
        </div>
      </header>

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando roles...</p>
      } @else if (roles().length === 0) {
        <p class="empty-state">No hay roles configurados.</p>
      } @else {
        <div class="admin-role-layout">
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
                        {{ isProtectedRole(role.name) ? 'Ver permisos' : 'Configurar' }}
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
                  {{ isProtectedRole(role.name) ? 'Ver permisos' : 'Configurar' }}
                </button>
              </article>
            }
          </div>

          <aside class="admin-panel permission-editor">
            @if (selectedRole(); as role) {
              <header>
                <div>
                  <h2>{{ role.name }}</h2>
                  <p>{{ role.description }}</p>
                </div>
                @if (isProtectedRole(role.name)) {
                  <span class="readonly-note">Protegido</span>
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

              @if (isProtectedRole(role.name)) {
                <p class="permission-help">
                  Este rol es tecnico/protegido. Sus permisos pueden consultarse, pero no modificarse desde la UI.
                </p>
              } @else {
                <p class="permission-help">
                  Los cambios afectan a {{ role.userCount }} usuario{{ role.userCount === 1 ? '' : 's' }} con este rol.
                  El backend aplica el nuevo permiso desde el siguiente request.
                </p>
              }

              <div class="permission-groups">
                @for (group of permissionGroups(); track group.key) {
                  <section class="permission-group">
                    <h3>{{ group.label }}</h3>
                    <div class="permission-options">
                      @for (permission of group.permissions; track permission.id) {
                        <label class="permission-option">
                          <input
                            type="checkbox"
                            [checked]="selectedPermissionIds().has(permission.id)"
                            [disabled]="isProtectedRole(role.name) || isSaving()"
                            (change)="togglePermission(permission.id, $any($event.target).checked)"
                          />
                          <span>
                            <strong>{{ permission.description }}</strong>
                            <small>{{ permission.key }}</small>
                          </span>
                        </label>
                      }
                    </div>
                  </section>
                }
              </div>

              @if (!isProtectedRole(role.name)) {
                <div class="editor-actions">
                  <button
                    class="primary-button"
                    type="button"
                    [disabled]="isSaving() || !hasPermissionChanges()"
                    (click)="savePermissions()"
                  >
                    {{ isSaving() ? 'Guardando...' : 'Guardar permisos' }}
                  </button>
                  <button
                    class="secondary-button"
                    type="button"
                    [disabled]="isSaving() || !hasPermissionChanges()"
                    (click)="resetPermissions()"
                  >
                    Descartar cambios
                  </button>
                </div>
              }
            } @else {
              <p class="empty-state">Selecciona un rol para ver sus permisos.</p>
            }
          </aside>
        </div>
      }
    </section>
  `,
  styles: `
    .permission-editor header {
      display: flex;
      align-items: flex-start;
      justify-content: space-between;
      gap: 1rem;
    }

    .permission-help {
      margin: 1rem 0;
      color: var(--text-muted, #52657a);
      line-height: 1.5;
    }

    .permission-groups {
      display: grid;
      gap: 1rem;
    }

    .permission-group {
      border-top: 1px solid var(--border-color, #d7e2ec);
      padding-top: 1rem;
    }

    .permission-group h3 {
      margin: 0 0 0.65rem;
      font-size: 1rem;
    }

    .permission-options {
      display: grid;
      gap: 0.6rem;
    }

    .permission-option {
      display: flex;
      align-items: flex-start;
      gap: 0.7rem;
      min-height: 44px;
      cursor: pointer;
    }

    .permission-option input {
      width: 1.15rem;
      height: 1.15rem;
      margin-top: 0.15rem;
    }

    .permission-option span {
      display: grid;
      gap: 0.15rem;
      min-width: 0;
    }

    .permission-option small {
      color: var(--text-muted, #52657a);
      overflow-wrap: anywhere;
    }

    .editor-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.75rem;
      margin-top: 1.25rem;
    }
  `
})
export class RolesPageComponent implements OnInit {
  readonly roles = signal<AdminRoleListItem[]>([]);
  readonly allPermissions = signal<AdminPermission[]>([]);
  readonly selectedRole = signal<AdminRoleDetail | null>(null);
  readonly selectedPermissionIds = signal<Set<string>>(new Set());
  readonly originalPermissionIds = signal<Set<string>>(new Set());
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  readonly permissionGroups = computed<PermissionGroup[]>(() => {
    const groups = new Map<string, AdminPermission[]>();

    for (const permission of this.allPermissions()) {
      const prefix = permission.key.split('.')[0] || 'other';
      const current = groups.get(prefix) ?? [];
      current.push(permission);
      groups.set(prefix, current);
    }

    return Array.from(groups.entries())
      .sort(([left], [right]) => left.localeCompare(right))
      .map(([key, permissions]) => ({
        key,
        label: this.groupLabel(key),
        permissions: permissions.sort((left, right) => left.key.localeCompare(right.key))
      }));
  });

  readonly hasPermissionChanges = computed(() =>
    !this.setsEqual(this.selectedPermissionIds(), this.originalPermissionIds())
  );

  constructor(private readonly adminSecurityService: AdminSecurityService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  selectRole(role: AdminRoleListItem): void {
    this.loadRoleDetail(role.id);
  }

  togglePermission(permissionId: string, isSelected: boolean): void {
    const next = new Set(this.selectedPermissionIds());

    if (isSelected) {
      next.add(permissionId);
    } else {
      next.delete(permissionId);
    }

    this.selectedPermissionIds.set(next);
    this.successMessage.set(null);
  }

  resetPermissions(): void {
    this.selectedPermissionIds.set(new Set(this.originalPermissionIds()));
    this.successMessage.set(null);
  }

  savePermissions(): void {
    const role = this.selectedRole();

    if (!role || this.isProtectedRole(role.name) || !this.hasPermissionChanges()) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateRolePermissions(role.id, Array.from(this.selectedPermissionIds()))
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updatedRole) => {
          this.applyRoleDetail(updatedRole);
          this.successMessage.set(`Permisos de ${updatedRole.name} actualizados correctamente.`);
          this.loadRoleListOnly();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  isProtectedRole(roleName: string): boolean {
    const normalizedName = roleName.trim().toLocaleLowerCase();
    return normalizedName === 'admin' || normalizedName === 'limited qa';
  }

  private loadRoles(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    forkJoin({
      roles: this.adminSecurityService.listRoles(),
      permissions: this.adminSecurityService.listPermissions()
    })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: ({ roles, permissions }) => {
          this.roles.set(roles);
          this.allPermissions.set(permissions);

          const driverRole = roles.find((role) => role.name === 'Repartidor');
          const firstRole = driverRole ?? roles[0];

          if (firstRole) {
            this.loadRoleDetail(firstRole.id);
          }
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private loadRoleListOnly(): void {
    this.adminSecurityService.listRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
    });
  }

  private loadRoleDetail(roleId: string): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService.getRoleById(roleId).subscribe({
      next: (detail) => this.applyRoleDetail(detail),
      error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
    });
  }

  private applyRoleDetail(detail: AdminRoleDetail): void {
    const permissionIds = new Set(detail.permissions.map((permission) => permission.id));
    this.selectedRole.set(detail);
    this.selectedPermissionIds.set(new Set(permissionIds));
    this.originalPermissionIds.set(new Set(permissionIds));
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

  private setsEqual(left: Set<string>, right: Set<string>): boolean {
    return left.size === right.size && Array.from(left).every((value) => right.has(value));
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar roles.';
    }

    if (error.status === 409) {
      return error.error?.title ?? 'Este rol esta protegido y no puede modificarse.';
    }

    if (error.status === 400) {
      return 'La seleccion contiene uno o mas permisos invalidos.';
    }

    return 'No fue posible cargar o guardar los permisos del rol.';
  }
}
