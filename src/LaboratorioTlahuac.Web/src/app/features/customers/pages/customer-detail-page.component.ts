import { DatePipe } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { AuthService } from '../../../core/auth/auth.service';
import { InternalDoctorsSectionComponent } from '../components/internal-doctors-section.component';
import { CustomerDetail, CustomerType } from '../customer.models';
import { CustomerService } from '../customer.service';

type CustomerDetailNavigationInfo = {
  successMessage?: unknown;
};

function isCustomerDetailNavigationInfo(value: unknown): value is CustomerDetailNavigationInfo {
  return typeof value === 'object' && value !== null && 'successMessage' in value;
}

@Component({
  selector: 'app-customer-detail-page',
  imports: [DatePipe, InternalDoctorsSectionComponent, RouterLink],
  template: `
    <section class="feature-page">
      @if (successMessage(); as successMessage) {
        <p class="alert-success" role="status">{{ successMessage }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando cliente...</p>
      } @else if (errorMessage(); as errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      } @else if (customer(); as customer) {
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
  readonly customer = signal<CustomerDetail | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly successMessage = signal<string | null>(null);

  constructor(
    private readonly customerService: CustomerService,
    private readonly authService: AuthService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {
    this.readSuccessMessageFromNavigationInfo();
  }

  get canEdit(): boolean {
    return this.authService.hasPermission('customers.edit');
  }

  ngOnInit(): void {
    this.load();
  }

  toggleStatus(): void {
    const currentCustomer = this.customer();

    if (!currentCustomer) {
      return;
    }

    if (currentCustomer.isActive && !window.confirm(`Desactivar a ${currentCustomer.displayName}?`)) {
      return;
    }

    this.customerService.updateStatus(currentCustomer.id, !currentCustomer.isActive).subscribe({
      next: (customer) => {
        this.customer.set(customer);
      },
      error: (error: HttpErrorResponse) => {
        this.errorMessage.set(this.toErrorMessage(error));
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

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.customerService
      .getById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (customer) => {
          this.customer.set(customer);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toErrorMessage(error));
          this.customer.set(null);
        }
      });
  }

  private readSuccessMessageFromNavigationInfo(): void {
    const info = this.router.currentNavigation()?.extras.info;

    if (
      isCustomerDetailNavigationInfo(info) &&
      typeof info.successMessage === 'string' &&
      info.successMessage.trim().length > 0
    ) {
      this.successMessage.set(info.successMessage);
    }
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
