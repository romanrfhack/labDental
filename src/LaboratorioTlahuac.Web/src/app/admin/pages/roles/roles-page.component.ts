import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';

import { AdminSecurityService } from '../../admin-security.service';
import { AdminRoleDetail, AdminRoleListItem } from '../../admin-security.models';

@Component({
  selector: 'app-roles-page',
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Roles</h1>
          <p>Permisos actuales por rol. Edicion de permisos queda cerrada en esta fase.</p>
        </div>
        <span class="readonly-note">Solo lectura</span>
      </header>

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
                      <small class="muted-block">{{ role.description }}</small>
                    </td>
                    <td>{{ role.isSystem ? 'Sistema' : 'Operativo' }}</td>
                    <td>{{ role.userCount }}</td>
                    <td>{{ role.permissionCount }}</td>
                    <td>
                      <button class="secondary-button" type="button" (click)="selectRole(role)">
                        Ver permisos
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
                <button class="secondary-button" type="button" (click)="selectRole(role)">Ver permisos</button>
              </article>
            }
          </div>

          <aside class="admin-panel">
            @if (selectedRole(); as role) {
              <header>
                <h2>{{ role.name }}</h2>
                <p>{{ role.description }}</p>
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

              @if (role.permissions.length === 0) {
                <p class="empty-state">Este rol no tiene permisos asignados.</p>
              } @else {
                <ul class="permission-list">
                  @for (permission of role.permissions; track permission.id) {
                    <li>
                      <strong>{{ permission.key }}</strong>
                      <span>{{ permission.description }}</span>
                    </li>
                  }
                </ul>
              }
            } @else {
              <p class="empty-state">Selecciona un rol para ver sus permisos.</p>
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
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(private readonly adminSecurityService: AdminSecurityService) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  selectRole(role: AdminRoleListItem): void {
    this.errorMessage.set(null);

    this.adminSecurityService.getRoleById(role.id).subscribe({
      next: (detail) => this.selectedRole.set(detail),
      error: (error: HttpErrorResponse) => this.errorMessage.set(this.toErrorMessage(error))
    });
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
      return 'No tienes permiso para consultar roles.';
    }

    return 'No fue posible cargar roles y permisos.';
  }
}
