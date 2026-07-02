import { Component, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { CustomerFormComponent } from '../components/customer-form.component';
import { CustomerUpsertRequest } from '../customer.models';
import { CustomerService } from '../customer.service';

@Component({
  selector: 'app-customer-create-page',
  imports: [CustomerFormComponent, RouterLink],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Nuevo cliente</h1>
          <p>Alta de doctor, clinica u otro cliente.</p>
        </div>
        <a class="ghost-button" routerLink="/app/clientes">Volver</a>
      </header>

      <app-customer-form
        submitLabel="Crear cliente"
        [isSubmitting]="isSubmitting()"
        [errorMessage]="errorMessage()"
        (save)="create($event)"
        (cancel)="cancel()"
      />
    </section>
  `
})
export class CustomerCreatePageComponent {
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly customerService: CustomerService,
    private readonly router: Router
  ) {}

  create(request: CustomerUpsertRequest): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.customerService
      .create(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (customer) =>
          this.router.navigate(['/app/clientes', customer.id], {
            info: { successMessage: 'Cliente creado correctamente.' }
          }),
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toErrorMessage(error));
        }
      });
  }

  cancel(): void {
    this.router.navigateByUrl('/app/clientes');
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Revisa los campos capturados.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para crear clientes.';
    }

    return 'No fue posible crear el cliente.';
  }
}
