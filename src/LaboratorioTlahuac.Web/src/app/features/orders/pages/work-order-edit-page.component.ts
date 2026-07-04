import { Component, OnInit, computed, signal } from '@angular/core';
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
          @if (order(); as order) {
            <p>{{ order.orderNumber }}</p>
          }
        </div>
        <a class="ghost-button" [routerLink]="backRoute()">
          Volver
        </a>
      </header>

      @if (isLoading()) {
        <p class="loading-state">Cargando orden...</p>
      } @else if (loadErrorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
      } @else if (order(); as order) {
        <app-work-order-form
          submitLabel="Guardar cambios"
          [order]="order"
          [isSubmitting]="isSubmitting()"
          [errorMessage]="errorMessage()"
          (save)="update($event)"
          (cancel)="cancel()"
        />
      }
    </section>
  `
})
export class WorkOrderEditPageComponent implements OnInit {
  readonly order = signal<WorkOrderDetail | null>(null);
  readonly isLoading = signal(false);
  readonly isSubmitting = signal(false);
  readonly loadErrorMessage = signal<string | null>(null);
  readonly errorMessage = signal<string | null>(null);
  readonly backRoute = computed(() => {
    const currentOrder = this.order();

    return currentOrder ? ['/app/ordenes', currentOrder.id] : '/app/ordenes';
  });

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly route: ActivatedRoute,
    private readonly router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  update(request: WorkOrderUpsertRequest): void {
    const currentOrder = this.order();

    if (!currentOrder) {
      return;
    }

    this.isSubmitting.set(true);
    this.errorMessage.set(null);

    this.workOrderService
      .update(currentOrder.id, request)
      .pipe(finalize(() => this.isSubmitting.set(false)))
      .subscribe({
        next: (order) => {
          this.router.navigate(['/app/ordenes', order.id], {
            info: { successMessage: 'Orden actualizada correctamente.' }
          });
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toSaveErrorMessage(error));
        }
      });
  }

  cancel(): void {
    const currentOrder = this.order();

    if (currentOrder) {
      this.router.navigate(['/app/ordenes', currentOrder.id]);
      return;
    }

    this.router.navigateByUrl('/app/ordenes');
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.loadErrorMessage.set('Orden no encontrada.');
      return;
    }

    this.isLoading.set(true);
    this.loadErrorMessage.set(null);
    this.order.set(null);

    this.workOrderService
      .getById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (order) => {
          if (order.isCancelled) {
            this.loadErrorMessage.set('Las ordenes canceladas no se editan en el MVP.');
            this.order.set(null);
            return;
          }

          this.order.set(order);
        },
        error: (error: HttpErrorResponse) => {
          this.loadErrorMessage.set(
            error.status === 404 ? 'Orden no encontrada.' : 'No fue posible cargar la orden.');
          this.order.set(null);
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
