import { Component, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CustomerFormComponent } from '../components/customer-form.component';
import { CustomerDetail, CustomerUpsertRequest } from '../customer.models';
import { CustomerService } from '../customer.service';

@Component({
  selector: 'app-customer-edit-page',
  imports: [CustomerFormComponent, RouterLink],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Editar cliente</h1>
          @if (customer(); as customer) {
            <p>{{ customer.displayName }}</p>
          }
        </div>
        @if (customer(); as customer) {
          <a class="ghost-button" [routerLink]="['/app/clientes', customer.id]">Volver</a>
        } @else {
          <a class="ghost-button" routerLink="/app/clientes">Volver</a>
        }
      </header>

      @if (isLoading()) {
        <p class="loading-state">Cargando cliente...</p>
      } @else if (customer(); as customer) {
        <app-customer-form
          submitLabel="Guardar cambios"
          [customer]="customer"
          [isSubmitting]="isSubmitting()"
          [errorMessage]="errorMessage()"
          (save)="update($event)"
          (cancel)="cancel()"
        />
      } @else if (errorMessage(); as errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }
    </section>
  `
})
export class CustomerEditPageComponent implements OnInit {
  readonly customer = signal<CustomerDetail | null>(null);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly customerService: CustomerService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  update(request: CustomerUpsertRequest): void {
    const currentCustomer = this.customer();

    if (!currentCustomer || this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.customerService
      .update(currentCustomer.id, request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (customer) =>
          this.router.navigate(['/app/clientes', customer.id], {
            info: { successMessage: 'Cliente actualizado correctamente.' }
          }),
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toSaveErrorMessage(error));
        }
      });
  }

  cancel(): void {
    const currentCustomer = this.customer();

    if (currentCustomer) {
      this.router.navigate(['/app/clientes', currentCustomer.id]);
      return;
    }

    this.router.navigateByUrl('/app/clientes');
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.errorMessage.set('Cliente no encontrado.');
      this.customer.set(null);
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
          this.errorMessage.set(this.toLoadErrorMessage(error));
          this.customer.set(null);
        }
      });
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 404) {
      return 'Cliente no encontrado.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para ver clientes.';
    }

    return 'No fue posible cargar el cliente.';
  }

  private toSaveErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'No puedes cambiar una clinica con doctores internos activos a Doctor u Otro.';
    }

    if (error.status === 400) {
      return 'Revisa los campos capturados.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para editar clientes.';
    }

    return 'No fue posible guardar el cliente.';
  }
}
