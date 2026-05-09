import { DatePipe } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

import { AuthService } from '../../../core/auth/auth.service';
import { InternalDoctorsSectionComponent } from '../components/internal-doctors-section.component';
import { CustomerDetail, CustomerType } from '../customer.models';
import { CustomerService } from '../customer.service';

@Component({
  selector: 'app-customer-detail-page',
  imports: [DatePipe, InternalDoctorsSectionComponent, RouterLink],
  template: `
    <section class="feature-page">
      @if (isLoading) {
        <p class="loading-state">Cargando cliente...</p>
      } @else if (errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      } @else if (customer) {
        <header class="page-header">
          <div>
            <h1>{{ customer.displayName }}</h1>
            <p>{{ formatType(customer.type) }}</p>
          </div>
          <div class="page-actions">
            <a class="ghost-button" routerLink="/app/clientes">Volver</a>
            @if (canEdit) {
              <a class="secondary-button" [routerLink]="['/app/clientes', customer.id, 'editar']">Editar</a>
              <button
                type="button"
                [class.danger-button]="customer.isActive"
                [class.secondary-button]="!customer.isActive"
                (click)="toggleStatus()"
              >
                {{ customer.isActive ? 'Desactivar' : 'Activar' }}
              </button>
            }
          </div>
        </header>

        <div class="detail-grid">
          <div class="detail-item">
            <strong>Estado</strong>
            <span class="status-pill" [class.active]="customer.isActive" [class.inactive]="!customer.isActive">
              {{ customer.isActive ? 'Activo' : 'Inactivo' }}
            </span>
          </div>
          <div class="detail-item">
            <strong>Contacto</strong>
            <span>{{ customer.contactName || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Telefono</strong>
            <span>{{ customer.phone || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>WhatsApp</strong>
            <span>{{ customer.whatsApp || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Email</strong>
            <span>{{ customer.email || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Razon social</strong>
            <span>{{ customer.legalName || '-' }}</span>
          </div>
          <div class="detail-item full-field">
            <strong>Direccion</strong>
            <span>{{ customer.address || '-' }}</span>
          </div>
          <div class="detail-item full-field">
            <strong>Notas</strong>
            <span>{{ customer.notes || '-' }}</span>
          </div>
          <div class="detail-item">
            <strong>Creado</strong>
            <span>{{ customer.createdAtUtc | date: 'medium' }}</span>
          </div>
          <div class="detail-item">
            <strong>Actualizado</strong>
            <span>{{ customer.updatedAtUtc | date: 'medium' }}</span>
          </div>
        </div>

        @if (customer.type === 'Clinic') {
          <app-internal-doctors-section [customer]="customer" />
        }
      }
    </section>
  `
})
export class CustomerDetailPageComponent implements OnInit {
  customer: CustomerDetail | null = null;
  isLoading = false;
  errorMessage = '';

  constructor(
    private readonly customerService: CustomerService,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  get canEdit(): boolean {
    return this.authService.hasPermission('customers.edit');
  }

  ngOnInit(): void {
    this.load();
  }

  toggleStatus(): void {
    if (!this.customer) {
      return;
    }

    if (this.customer.isActive && !window.confirm(`Desactivar a ${this.customer.displayName}?`)) {
      return;
    }

    this.customerService.updateStatus(this.customer.id, !this.customer.isActive).subscribe({
      next: (customer) => {
        this.customer = customer;
      },
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
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigateByUrl('/app/clientes');
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    this.customerService.getById(id).subscribe({
      next: (customer) => {
        this.customer = customer;
        this.isLoading = false;
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage = this.toErrorMessage(error);
        this.isLoading = false;
      }
    });
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 404) {
      return 'Cliente no encontrado.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para ver clientes.';
    }

    return 'No fue posible cargar el cliente.';
  }
}
