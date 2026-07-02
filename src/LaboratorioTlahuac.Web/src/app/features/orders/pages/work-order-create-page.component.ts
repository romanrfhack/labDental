import { Component, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { WorkOrderFormComponent } from '../components/work-order-form.component';
import { WorkOrderUpsertRequest } from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-create-page',
  imports: [RouterLink, WorkOrderFormComponent],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Nueva orden</h1>
          <p>Alta de trabajo dental para un cliente.</p>
        </div>
        <a class="ghost-button" routerLink="/app/ordenes">Volver</a>
      </header>

      <app-work-order-form
        submitLabel="Crear orden"
        [isSubmitting]="isSubmitting()"
        [errorMessage]="errorMessage()"
        (save)="create($event)"
        (cancel)="cancel()"
      />
    </section>
  `
})
export class WorkOrderCreatePageComponent {
  readonly isSubmitting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly router: Router
  ) {}

  create(request: WorkOrderUpsertRequest): void {
    if (this.isSubmitting()) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.workOrderService
      .create(request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (order) => this.router.navigate(['/app/ordenes', order.id]),
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toErrorMessage(error));
        }
      });
  }

  cancel(): void {
    this.router.navigateByUrl('/app/ordenes');
  }

  private toErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400) {
      return 'Revisa los campos capturados.';
    }

    if (error.status === 409) {
      return 'La orden no cumple una regla de cliente o doctor interno.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para crear ordenes.';
    }

    return 'No fue posible crear la orden.';
  }
}
