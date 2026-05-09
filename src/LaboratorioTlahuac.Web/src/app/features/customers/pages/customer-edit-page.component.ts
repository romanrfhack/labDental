import { Component, OnInit } from '@angular/core';
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
          @if (customer) {
            <p>{{ customer.displayName }}</p>
          }
        </div>
        <a class="ghost-button" [routerLink]="customer ? ['/app/clientes', customer.id] : '/app/clientes'">
          Volver
        </a>
      </header>

      @if (isLoading) {
        <p class="loading-state">Cargando cliente...</p>
      } @else if (loadErrorMessage) {
        <p class="alert-error" role="alert">{{ loadErrorMessage }}</p>
      } @else if (customer) {
        <app-customer-form
          submitLabel="Guardar cambios"
          [customer]="customer"
          [isSubmitting]="isSubmitting"
          [errorMessage]="errorMessage"
          (save)="update($event)"
          (cancel)="cancel()"
        />
      }
    </section>
  `
})
export class CustomerEditPageComponent implements OnInit {
  customer: CustomerDetail | null = null;
  isLoading = false;
  isSubmitting = false;
  loadErrorMessage = '';
  errorMessage = '';

  constructor(
    private readonly customerService: CustomerService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  update(request: CustomerUpsertRequest): void {
    if (!this.customer) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.customerService
      .update(this.customer.id, request)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (customer) => this.router.navigate(['/app/clientes', customer.id]),
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.toSaveErrorMessage(error);
        }
      });
  }

  cancel(): void {
    if (this.customer) {
      this.router.navigate(['/app/clientes', this.customer.id]);
      return;
    }

    this.router.navigateByUrl('/app/clientes');
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.loadErrorMessage = 'Cliente no encontrado.';
      return;
    }

    this.isLoading = true;
    this.loadErrorMessage = '';

    this.customerService
      .getById(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (customer) => {
          this.customer = customer;
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage =
            error.status === 404 ? 'Cliente no encontrado.' : 'No fue posible cargar el cliente.';
        }
      });
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
