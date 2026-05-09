import { Component, OnInit } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { WorkOrderFormComponent } from '../components/work-order-form.component';
import { WorkOrderDetail, WorkOrderUpsertRequest } from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-edit-page',
  imports: [RouterLink, WorkOrderFormComponent],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Editar orden</h1>
          @if (order) {
            <p>{{ order.orderNumber }}</p>
          }
        </div>
        <a class="ghost-button" [routerLink]="order ? ['/app/ordenes', order.id] : '/app/ordenes'">
          Volver
        </a>
      </header>

      @if (isLoading) {
        <p class="loading-state">Cargando orden...</p>
      } @else if (loadErrorMessage) {
        <p class="alert-error" role="alert">{{ loadErrorMessage }}</p>
      } @else if (order) {
        <app-work-order-form
          submitLabel="Guardar cambios"
          [order]="order"
          [isSubmitting]="isSubmitting"
          [errorMessage]="errorMessage"
          (save)="update($event)"
          (cancel)="cancel()"
        />
      }
    </section>
  `
})
export class WorkOrderEditPageComponent implements OnInit {
  order: WorkOrderDetail | null = null;
  isLoading = false;
  isSubmitting = false;
  loadErrorMessage = '';
  errorMessage = '';

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  update(request: WorkOrderUpsertRequest): void {
    if (!this.order) {
      return;
    }

    this.isSubmitting = true;
    this.errorMessage = '';

    this.workOrderService
      .update(this.order.id, request)
      .pipe(finalize(() => (this.isSubmitting = false)))
      .subscribe({
        next: (order) => this.router.navigate(['/app/ordenes', order.id]),
        error: (error: HttpErrorResponse) => {
          this.errorMessage = this.toSaveErrorMessage(error);
        }
      });
  }

  cancel(): void {
    if (this.order) {
      this.router.navigate(['/app/ordenes', this.order.id]);
      return;
    }

    this.router.navigateByUrl('/app/ordenes');
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.loadErrorMessage = 'Orden no encontrada.';
      return;
    }

    this.isLoading = true;
    this.loadErrorMessage = '';

    this.workOrderService
      .getById(id)
      .pipe(finalize(() => (this.isLoading = false)))
      .subscribe({
        next: (order) => {
          if (order.isCancelled) {
            this.loadErrorMessage = 'Las ordenes canceladas no se editan en el MVP.';
            return;
          }

          this.order = order;
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage =
            error.status === 404 ? 'Orden no encontrada.' : 'No fue posible cargar la orden.';
        }
      });
  }

  private toSaveErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 409) {
      return 'La orden no puede editarse con esa combinacion de cliente y doctor interno.';
    }

    if (error.status === 400) {
      return 'Revisa los campos capturados.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para editar ordenes.';
    }

    return 'No fue posible guardar la orden.';
  }
}
