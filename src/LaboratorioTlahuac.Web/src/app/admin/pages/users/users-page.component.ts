import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, computed, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';

import {
  AdminPermissionOverrideEffect,
  AdminRoleListItem,
  AdminRoleSummary,
  AdminUserDetail,
  AdminUserListItem,
  AdminUserPermissionState
} from '../../admin-security.models';
import { AdminSecurityService } from '../../admin-security.service';

type ActiveFilter = 'all' | 'active' | 'inactive';
type PermissionOverrideSelection = 'Inherited' | AdminPermissionOverrideEffect;

interface CreateUserForm {
  email: string;
  fullName: string;
  temporaryPassword: string;
  roleIds: string[];
}

interface EditUserForm {
  email: string;
  fullName: string;
  temporaryPassword: string;
  roleIds: string[];
}

interface PermissionGroup {
  key: string;
  label: string;
  permissions: AdminUserPermissionState[];
}

@Component({
  selector: 'app-users-page',
  imports: [FormsModule],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Usuarios</h1>
          <p>Alta, estado, roles y excepciones individuales de acceso para la app privada.</p>
        </div>
        <button class="primary-button" type="button" (click)="toggleCreateForm()">
          {{ showCreateForm() ? 'Cerrar alta' : 'Nuevo usuario' }}
        </button>
      </header>

      <form class="toolbar admin-users-toolbar" (ngSubmit)="applyFilters()">
        <label class="filter-field">
          <span>Busqueda</span>
          <input type="search" name="search" [(ngModel)]="search" />
        </label>
        <label class="filter-field">
          <span>Estado</span>
          <select name="activeFilter" [(ngModel)]="activeFilter">
            <option value="all">Todos</option>
            <option value="active">Activos</option>
            <option value="inactive">Inactivos</option>
          </select>
        </label>
        <label class="filter-field">
          <span>Rol</span>
          <select name="roleId" [(ngModel)]="roleId">
            <option value="">Todos</option>
            @for (role of roles(); track role.id) {
              <option [value]="role.id">{{ role.name }}</option>
            }
          </select>
        </label>
        <button class="secondary-button" type="submit">Filtrar</button>
      </form>

      @if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      }

      @if (successMessage(); as message) {
        <p class="alert-success" role="status">{{ message }}</p>
      }

      @if (showCreateForm()) {
        <form class="admin-panel" (ngSubmit)="createUser()">
          <header>
            <h2>Nuevo usuario</h2>
            <p>El usuario hereda automaticamente los permisos de los roles seleccionados.</p>
          </header>

          <div class="field-grid">
            <label class="form-field">
              <span>Nombre completo</span>
              <input name="newFullName" [(ngModel)]="createForm.fullName" autocomplete="name" required />
            </label>
            <label class="form-field">
              <span>Email</span>
              <input name="newEmail" type="email" [(ngModel)]="createForm.email" autocomplete="email" required />
            </label>
            <label class="form-field full-field">
              <span>Contrasena temporal</span>
              <input
                name="newTemporaryPassword"
                type="password"
                [(ngModel)]="createForm.temporaryPassword"
                autocomplete="new-password"
                required
              />
            </label>
          </div>

          <fieldset class="admin-fieldset">
            <legend>Roles</legend>
            <p class="permission-guidance">Los permisos se heredan del rol; no se copian al usuario.</p>
            <div class="admin-checkbox-grid">
              @for (role of roles(); track role.id) {
                <label class="check-field">
                  <input
                    type="checkbox"
                    [checked]="isRoleSelected(createForm.roleIds, role.id)"
                    (change)="toggleRole(createForm.roleIds, role.id, $event)"
                  />
                  <span>{{ role.name }}</span>
                </label>
              }
            </div>
          </fieldset>

          <div class="page-actions">
            <button class="primary-button" type="submit" [disabled]="isSaving()">Crear usuario</button>
            <button class="ghost-button" type="button" (click)="resetCreateForm()">Limpiar</button>
          </div>
        </form>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando usuarios...</p>
      } @else if (users().length === 0) {
        <p class="empty-state">No hay usuarios con los filtros actuales.</p>
      } @else {
        <div class="table-scroll admin-users-table-scroll">
          <table class="data-table">
            <thead>
              <tr>
                <th>Usuario</th>
                <th>Email</th>
                <th>Roles</th>
                <th>Estado</th>
                <th>Ultimo acceso</th>
                <th>Acciones</th>
              </tr>
            </thead>
            <tbody>
              @for (user of users(); track user.id) {
                <tr>
                  <td>{{ user.fullName }}</td>
                  <td>{{ user.email }}</td>
                  <td>{{ formatRoles(user.roles) }}</td>
                  <td>
                    <span class="status-pill" [class.active]="user.isActive" [class.inactive]="!user.isActive">
                      {{ user.isActive ? 'Activo' : 'Inactivo' }}
                    </span>
                  </td>
                  <td>{{ formatDateTime(user.lastLoginAtUtc) }}</td>
                  <td>
                    <div class="page-actions">
                      <button class="secondary-button" type="button" (click)="startEdit(user)">Editar</button>
                      <button
                        type="button"
                        [class.danger-button]="user.isActive"
                        [class.secondary-button]="!user.isActive"
                        [disabled]="isSaving()"
                        (click)="toggleUserStatus(user)"
                      >
                        {{ user.isActive ? 'Desactivar' : 'Activar' }}
                      </button>
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>

        <div class="admin-mobile-list">
          @for (user of users(); track user.id) {
            <article class="admin-card">
              <header>
                <div>
                  <strong>{{ user.fullName }}</strong>
                  <span>{{ user.email }}</span>
                </div>
                <span class="status-pill" [class.active]="user.isActive" [class.inactive]="!user.isActive">
                  {{ user.isActive ? 'Activo' : 'Inactivo' }}
                </span>
              </header>
              <dl>
                <div>
                  <dt>Roles</dt>
                  <dd>{{ formatRoles(user.roles) }}</dd>
                </div>
                <div>
                  <dt>Ultimo acceso</dt>
                  <dd>{{ formatDateTime(user.lastLoginAtUtc) }}</dd>
                </div>
              </dl>
              <div class="page-actions">
                <button class="secondary-button" type="button" (click)="startEdit(user)">Editar</button>
                <button
                  type="button"
                  [class.danger-button]="user.isActive"
                  [class.secondary-button]="!user.isActive"
                  [disabled]="isSaving()"
                  (click)="toggleUserStatus(user)"
                >
                  {{ user.isActive ? 'Desactivar' : 'Activar' }}
                </button>
              </div>
            </article>
          }
        </div>
      }

      <div class="page-actions pagination-actions">
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

      @if (selectedUser(); as user) {
        <form class="admin-panel" (ngSubmit)="saveBasicUser(user)">
          <header>
            <h2>Editar usuario</h2>
            <p>{{ user.fullName }}</p>
          </header>

          <div class="field-grid">
            <label class="form-field">
              <span>Nombre completo</span>
              <input name="editFullName" [(ngModel)]="editForm.fullName" required />
            </label>
            <label class="form-field">
              <span>Email</span>
              <input name="editEmail" type="email" [(ngModel)]="editForm.email" required />
            </label>
          </div>

          <div class="page-actions">
            <button class="primary-button" type="submit" [disabled]="isSaving()">Guardar datos</button>
            <button class="ghost-button" type="button" (click)="clearSelection()">Cerrar</button>
          </div>
        </form>

        <section class="admin-panel">
          <header>
            <h2>Roles asignados</h2>
            <p>Los permisos base del usuario se heredan de estos roles.</p>
          </header>

          <div class="admin-checkbox-grid">
            @for (role of roles(); track role.id) {
              <label class="check-field">
                <input
                  type="checkbox"
                  [checked]="isRoleSelected(editForm.roleIds, role.id)"
                  (change)="toggleRole(editForm.roleIds, role.id, $event)"
                />
                <span>{{ role.name }}</span>
              </label>
            }
          </div>

          <div class="page-actions">
            <button class="secondary-button" type="button" [disabled]="isSaving()" (click)="saveRoles(user)">
              Guardar roles
            </button>
          </div>
        </section>

        <section class="admin-panel user-permission-panel">
          <header class="permission-editor-header">
            <div>
              <h2>Permisos efectivos</h2>
              <p>Usa excepciones solo cuando este usuario deba apartarse de los permisos normales de su rol.</p>
            </div>
            @if (user.isPermissionOverrideEditingLocked) {
              <span class="permission-lock-badge">Admin protegido</span>
            } @else {
              <span class="permission-edit-badge">Excepciones por usuario</span>
            }
          </header>

          @if (user.isPermissionOverrideEditingLocked) {
            <p class="permission-guidance">
              Este usuario pertenece al rol Admin. Conserva todos los permisos del sistema y no admite excepciones
              individuales.
            </p>
          } @else {
            <p class="permission-guidance">
              Heredado mantiene la configuracion del rol. Permitir agrega una excepcion y Denegar prevalece sobre lo
              heredado. Los cambios se hacen efectivos en la siguiente solicitud autenticada.
            </p>
          }

          <div class="permission-groups">
            @for (group of permissionGroups(user.permissions); track group.key) {
              <fieldset class="admin-fieldset permission-group">
                <legend>{{ group.label }}</legend>
                <div class="user-permission-list">
                  @for (permission of group.permissions; track permission.id) {
                    <div class="user-permission-row">
                      <div class="user-permission-copy">
                        <div class="permission-title-line">
                          <strong>{{ permissionLabel(permission.key) }}</strong>
                          <span
                            class="status-pill"
                            [class.active]="permission.effective"
                            [class.inactive]="!permission.effective"
                          >
                            {{ permission.effective ? 'Permitido' : 'Denegado' }}
                          </span>
                        </div>
                        <span>{{ permission.description }}</span>
                        <small>
                          {{ permission.inherited ? 'Heredado de: ' + permission.sourceRoles.join(', ') : 'Sin rol de origen' }}
                          · {{ permission.key }}
                        </small>
                      </div>

                      <label class="permission-override-field">
                        <span>Configuracion</span>
                        <select
                          [value]="permissionOverrideSelection(permission.id)"
                          [disabled]="user.isPermissionOverrideEditingLocked || isSaving()"
                          (change)="setPermissionOverride(permission.id, $event)"
                        >
                          <option value="Inherited">Heredado</option>
                          <option value="Allow">Permitir</option>
                          <option value="Deny">Denegar</option>
                        </select>
                      </label>
                    </div>
                  }
                </div>
              </fieldset>
            }
          </div>

          @if (!user.isPermissionOverrideEditingLocked) {
            <div class="page-actions">
              <button class="primary-button" type="button" [disabled]="isSaving()" (click)="savePermissionOverrides(user)">
                {{ isSaving() ? 'Guardando...' : 'Guardar excepciones' }}
              </button>
              <button class="ghost-button" type="button" [disabled]="isSaving()" (click)="resetPermissionOverrides(user)">
                Descartar cambios
              </button>
            </div>
          }
        </section>

        <form class="admin-panel" (ngSubmit)="setTemporaryPassword(user)">
          <header>
            <h2>Contrasena temporal</h2>
            <p>No se envia correo. Compartela solo por un canal seguro.</p>
          </header>

          <label class="form-field">
            <span>Nueva contrasena temporal</span>
            <input
              name="editTemporaryPassword"
              type="password"
              [(ngModel)]="editForm.temporaryPassword"
              autocomplete="new-password"
              required
            />
          </label>

          <div class="page-actions">
            <button class="secondary-button" type="submit" [disabled]="isSaving()">
              Actualizar contrasena
            </button>
          </div>
        </form>
      }
    </section>
  `
})
export class UsersPageComponent implements OnInit {
  readonly users = signal<AdminUserListItem[]>([]);
  readonly roles = signal<AdminRoleListItem[]>([]);
  readonly selectedUser = signal<AdminUserDetail | null>(null);
  readonly showCreateForm = signal(false);
  readonly page = signal(1);
  readonly pageSize = signal(20);
  readonly totalCount = signal(0);
  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);
  readonly totalPages = computed(() => Math.max(1, Math.ceil(this.totalCount() / this.pageSize())));

  search = '';
  activeFilter: ActiveFilter = 'all';
  roleId = '';
  createForm: CreateUserForm = this.emptyCreateForm();
  editForm: EditUserForm = this.emptyEditForm();
  permissionOverrideEdits: Record<string, PermissionOverrideSelection> = {};

  constructor(private readonly adminSecurityService: AdminSecurityService) {}

  ngOnInit(): void {
    this.loadRoles();
    this.loadUsers();
  }

  toggleCreateForm(): void {
    this.showCreateForm.update((visible) => !visible);
    this.errorMessage.set(null);
    this.successMessage.set(null);
  }

  applyFilters(): void {
    this.page.set(1);
    this.loadUsers();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages() || page === this.page()) {
      return;
    }

    this.page.set(page);
    this.loadUsers();
  }

  createUser(): void {
    const validationMessage = this.validateUserForm(
      this.createForm.email,
      this.createForm.fullName,
      this.createForm.temporaryPassword,
      this.createForm.roleIds,
      true
    );

    if (validationMessage) {
      this.errorMessage.set(validationMessage);
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .createUser({
        email: this.createForm.email.trim(),
        fullName: this.createForm.fullName.trim(),
        temporaryPassword: this.createForm.temporaryPassword,
        roleIds: this.createForm.roleIds
      })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: () => {
          this.successMessage.set('Usuario creado correctamente con los permisos heredados de sus roles.');
          this.resetCreateForm();
          this.showCreateForm.set(false);
          this.loadUsers();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  resetCreateForm(): void {
    this.createForm = this.emptyCreateForm();
  }

  startEdit(user: AdminUserListItem): void {
    this.errorMessage.set(null);
    this.successMessage.set(null);
    this.isSaving.set(true);

    this.adminSecurityService
      .getUserById(user.id)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (detail) => this.applySelectedUser(detail),
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  saveBasicUser(user: AdminUserDetail): void {
    const validationMessage = this.validateUserForm(
      this.editForm.email,
      this.editForm.fullName,
      '',
      this.editForm.roleIds,
      false
    );

    if (validationMessage) {
      this.errorMessage.set(validationMessage);
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateUser(user.id, {
        email: this.editForm.email.trim(),
        fullName: this.editForm.fullName.trim()
      })
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.applySelectedUser(updated);
          this.successMessage.set('Usuario actualizado correctamente.');
          this.loadUsers();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  saveRoles(user: AdminUserDetail): void {
    if (this.editForm.roleIds.length === 0) {
      this.errorMessage.set('Selecciona al menos un rol.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .assignUserRoles(user.id, this.editForm.roleIds)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.applySelectedUser(updated);
          this.successMessage.set('Roles y permisos heredados actualizados correctamente.');
          this.loadUsers();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  savePermissionOverrides(user: AdminUserDetail): void {
    if (user.isPermissionOverrideEditingLocked) {
      return;
    }

    const overrides = user.permissions
      .map((permission) => ({
        permissionId: permission.id,
        effect: this.permissionOverrideSelection(permission.id)
      }))
      .filter((item): item is { permissionId: string; effect: AdminPermissionOverrideEffect } =>
        item.effect === 'Allow' || item.effect === 'Deny'
      );

    if (!window.confirm(`Guardar ${overrides.length} excepcion(es) de permisos para ${user.fullName}?`)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateUserPermissions(user.id, overrides)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.applySelectedUser(updated);
          this.successMessage.set('Excepciones de permisos actualizadas correctamente.');
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  resetPermissionOverrides(user: AdminUserDetail): void {
    this.permissionOverrideEdits = Object.fromEntries(
      user.permissions.map((permission) => [permission.id, permission.overrideEffect ?? 'Inherited'])
    );
  }

  permissionOverrideSelection(permissionId: string): PermissionOverrideSelection {
    return this.permissionOverrideEdits[permissionId] ?? 'Inherited';
  }

  setPermissionOverride(permissionId: string, event: Event): void {
    const value = (event.target as HTMLSelectElement).value as PermissionOverrideSelection;
    this.permissionOverrideEdits = { ...this.permissionOverrideEdits, [permissionId]: value };
  }

  setTemporaryPassword(user: AdminUserDetail): void {
    if (this.editForm.temporaryPassword.trim().length < 10) {
      this.errorMessage.set('La contrasena temporal debe tener al menos 10 caracteres.');
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .setTemporaryPassword(user.id, this.editForm.temporaryPassword)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          this.applySelectedUser(updated);
          this.editForm.temporaryPassword = '';
          this.successMessage.set('Contrasena temporal actualizada.');
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  toggleUserStatus(user: AdminUserListItem): void {
    const nextState = !user.isActive;

    if (!nextState && !window.confirm(`Desactivar a ${user.fullName}?`)) {
      return;
    }

    this.isSaving.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    this.adminSecurityService
      .updateUserStatus(user.id, nextState)
      .pipe(finalize(() => this.isSaving.set(false)))
      .subscribe({
        next: (updated) => {
          if (this.selectedUser()?.id === updated.id) {
            this.applySelectedUser(updated);
          }

          this.successMessage.set(nextState ? 'Usuario activado.' : 'Usuario desactivado.');
          this.loadUsers();
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  clearSelection(): void {
    this.selectedUser.set(null);
    this.editForm = this.emptyEditForm();
    this.permissionOverrideEdits = {};
  }

  isRoleSelected(roleIds: string[], roleId: string): boolean {
    return roleIds.includes(roleId);
  }

  toggleRole(roleIds: string[], roleId: string, event: Event): void {
    const checked = (event.target as HTMLInputElement).checked;
    const index = roleIds.indexOf(roleId);

    if (checked && index === -1) {
      roleIds.push(roleId);
      return;
    }

    if (!checked && index !== -1) {
      roleIds.splice(index, 1);
    }
  }

  formatRoles(roles: AdminRoleSummary[]): string {
    return roles.length > 0 ? roles.map((role) => role.name).join(', ') : '-';
  }

  formatDateTime(value: string | null): string {
    if (!value) {
      return '-';
    }

    return new Intl.DateTimeFormat('es-MX', {
      dateStyle: 'short',
      timeStyle: 'short'
    }).format(new Date(value));
  }

  permissionGroups(permissions: AdminUserPermissionState[]): PermissionGroup[] {
    const groups = new Map<string, AdminUserPermissionState[]>();

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

  private applySelectedUser(detail: AdminUserDetail): void {
    this.selectedUser.set(detail);
    this.editForm = {
      email: detail.email,
      fullName: detail.fullName,
      temporaryPassword: this.editForm.temporaryPassword,
      roleIds: detail.roles.map((role) => role.id)
    };
    this.resetPermissionOverrides(detail);
  }

  private loadUsers(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.adminSecurityService
      .listUsers({
        search: this.search.trim() || undefined,
        isActive: this.toIsActiveFilter(),
        roleId: this.roleId || undefined,
        page: this.page(),
        pageSize: this.pageSize()
      })
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (response) => {
          this.users.set(response.items);
          this.page.set(response.page);
          this.pageSize.set(response.pageSize);
          this.totalCount.set(response.totalCount);
        },
        error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
      });
  }

  private loadRoles(): void {
    this.adminSecurityService.listRoles().subscribe({
      next: (roles) => this.roles.set(roles),
      error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
    });
  }

  private toIsActiveFilter(): boolean | undefined {
    if (this.activeFilter === 'active') {
      return true;
    }

    if (this.activeFilter === 'inactive') {
      return false;
    }

    return undefined;
  }

  private validateUserForm(
    email: string,
    fullName: string,
    temporaryPassword: string,
    roleIds: string[],
    requirePassword: boolean
  ): string | null {
    if (!fullName.trim()) {
      return 'Captura el nombre completo.';
    }

    if (!email.trim() || !email.includes('@')) {
      return 'Captura un email valido.';
    }

    if (requirePassword && temporaryPassword.trim().length < 10) {
      return 'La contrasena temporal debe tener al menos 10 caracteres.';
    }

    if (roleIds.length === 0) {
      return 'Selecciona al menos un rol.';
    }

    return null;
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para administrar usuarios.';
    }

    if (error.status === 409) {
      return this.getProblemTitle(error) ?? 'El cambio entra en conflicto con el estado actual.';
    }

    if (error.status === 400) {
      return 'Revisa los datos o permisos seleccionados.';
    }

    return 'No fue posible completar la operacion.';
  }

  private getProblemTitle(error: HttpErrorResponse): string | null {
    if (error.error && typeof error.error === 'object' && 'title' in error.error) {
      const title = (error.error as { title?: unknown }).title;
      return typeof title === 'string' ? title : null;
    }

    return null;
  }

  private emptyCreateForm(): CreateUserForm {
    return {
      email: '',
      fullName: '',
      temporaryPassword: '',
      roleIds: []
    };
  }

  private emptyEditForm(): EditUserForm {
    return {
      email: '',
      fullName: '',
      temporaryPassword: '',
      roleIds: []
    };
  }
}
