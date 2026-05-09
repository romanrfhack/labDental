import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { CustomerListItem, CustomerType } from '../customer.models';
import { CustomerService } from '../customer.service';

type ActiveFilter = 'active' | 'inactive';

@Component({
  selector: 'app-customer-list-page',
  imports: [FormsModule, RouterLink],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Clientes</h1>
          <p>Doctores, clinicas y otros clientes del laboratorio.</p>
        </div>
        @if (canCreate) {
          <a class="primary-button" routerLink="/app/clientes/nuevo">Nuevo cliente</a>
        }
      </header>

      <form class="toolbar" (ngSubmit)="applyFilters()">
        <label class="filter-field">
          <span>Busqueda</span>
          <input type="search" name="search" [(ngModel)]="search" />
        </label>
        <label class="filter-field">
          <span>Tipo</span>
          <select name="type" [(ngModel)]="type">
            <option value="">Todos</option>
            <option value="Doctor">Doctor</option>
            <option value="Clinic">Clinica</option>
            <option value="Other">Otro</option>
          </select>
        </label>
        <label class="filter-field">
          <span>Estado</span>
          <select name="activeFilter" [(ngModel)]="activeFilter">
            <option value="active">Activos</option>
            <option value="inactive">Inactivos</option>
          </select>
        </label>
        <button class="secondary-button" type="submit">Filtrar</button>
      </form>

      @if (errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }

      @if (isLoading) {
        <p class="loading-state">Cargando clientes...</p>
      } @else if (items.length === 0) {
        <p class="empty-state">No hay clientes con los filtros actuales.</p>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Tipo</th>
              <th>Contacto</th>
              <th>Email</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            @for (customer of items; track customer.id) {
              <tr>
                <td>
                  <a [routerLink]="['/app/clientes', customer.id]">{{ customer.displayName }}</a>
                </td>
                <td>{{ formatType(customer.type) }}</td>
                <td>{{ customer.contactName || customer.phone || customer.whatsApp || '-' }}</td>
                <td>{{ customer.email || '-' }}</td>
                <td>
                  <span class="status-pill" [class.active]="customer.isActive" [class.inactive]="!customer.isActive">
                    {{ customer.isActive ? 'Activo' : 'Inactivo' }}
                  </span>
                </td>
                <td>
                  <div class="page-actions">
                    <a class="ghost-button" [routerLink]="['/app/clientes', customer.id]">Ver</a>
                    @if (canEdit) {
                      <a class="secondary-button" [routerLink]="['/app/clientes', customer.id, 'editar']">
                        Editar
                      </a>
                      <button
                        type="button"
                        [class.danger-button]="customer.isActive"
                        [class.secondary-button]="!customer.isActive"
                        (click)="toggleStatus(customer)"
                      >
                        {{ customer.isActive ? 'Desactivar' : 'Activar' }}
                      </button>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
      }

      <div class="page-actions">
        <button class="ghost-button" type="button" [disabled]="page <= 1 || isLoading" (click)="changePage(page - 1)">
          Anterior
        </button>
        <span>Pagina {{ page }} de {{ totalPages }}</span>
        <button
          class="ghost-button"
          type="button"
          [disabled]="page >= totalPages || isLoading"
          (click)="changePage(page + 1)"
        >
          Siguiente
        </button>
      </div>
    </section>
  `
})
export class CustomerListPageComponent implements OnInit {
  items: CustomerListItem[] = [];
  search = '';
  type: CustomerType | '' = '';
  activeFilter: ActiveFilter = 'active';
  page = 1;
  pageSize = 20;
  totalCount = 0;
  isLoading = false;
  errorMessage = '';

  constructor(
    private readonly customerService: CustomerService,
    private readonly authService: AuthService
  ) {}

  get canCreate(): boolean {
    return this.authService.hasPermission('customers.create');
  }

  get canEdit(): boolean {
    return this.authService.hasPermission('customers.edit');
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.totalCount / this.pageSize));
  }

  ngOnInit(): void {
    this.load();
  }

  applyFilters(): void {
    this.page = 1;
    this.load();
  }

  changePage(page: number): void {
    if (page < 1 || page > this.totalPages || page === this.page) {
      return;
    }

    this.page = page;
    this.load();
  }

  toggleStatus(customer: CustomerListItem): void {
    if (customer.isActive && !window.confirm(`Desactivar a ${customer.displayName}?`)) {
      return;
    }

    this.customerService.updateStatus(customer.id, !customer.isActive).subscribe({
      next: () => this.load(),
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.toErrorMessage(error);
      }
    });
  }

  formatType(type: CustomerType): string {
    if (type === 'Clinic') {
      return 'Clinica';
    }

    if (type === 'Other') {
      return 'Otro';
    }

    return 'Doctor';
  }

  private load(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.customerService
      .list({
        search: this.search.trim() || undefined,
        type: this.type || undefined,
        isActive: this.activeFilter === 'active',
        page: this.page,
        pageSize: this.pageSize
      })
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (response) => {
          this.items = response.items;
          this.page = response.page;
          this.pageSize = response.pageSize;
          this.totalCount = response.totalCount;
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.toErrorMessage(error);
        }
      });
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 403) {
      return 'No tienes permiso para consultar clientes.';
    }

    return 'No fue posible cargar clientes.';
  }
}
