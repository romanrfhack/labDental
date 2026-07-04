import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { WorkOrderDetail } from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-job-label-page',
  imports: [RouterLink],
  template: `
    <section class="label-print-page">
      @if (isLoading()) {
        <p class="loading-state">Cargando etiqueta...</p>
      } @else if (errorMessage(); as message) {
        <p class="alert-error" role="alert">{{ message }}</p>
        <div class="label-screen-actions">
          <a class="ghost-button" [routerLink]="fallbackRoute()">Volver</a>
        </div>
      } @else if (order(); as order) {
        <header class="label-page-heading">
          <div>
            <h1>Etiqueta interna</h1>
            <p>{{ order.orderNumber }}</p>
          </div>
          <div class="label-screen-actions">
            <button class="primary-button" type="button" (click)="print()">Imprimir</button>
            <a class="ghost-button" [routerLink]="['/app/ordenes', order.id]">Volver a la orden</a>
          </div>
        </header>

        <div class="label-stage" aria-label="Vista previa de etiqueta interna 76 x 51 milimetros">
          <article class="thermal-label work-label">
            <header class="label-top">
              <div class="brand-line">
                <span class="brand-mark">LDT</span>
                <span class="label-kind">Etiqueta interna</span>
              </div>
              <strong class="order-number">{{ order.orderNumber }}</strong>
            </header>

            <div class="label-body">
              <div class="label-row">
                <strong>Cliente</strong>
                <span>{{ compact(order.customerDisplayName, 44) }}</span>
              </div>
              @if (order.internalDoctorFullName) {
                <div class="label-row">
                  <strong>Dr. interno</strong>
                  <span>{{ compact(order.internalDoctorFullName, 38) }}</span>
                </div>
              }
              <div class="label-row">
                <strong>Paciente</strong>
                <span>{{ compact(order.patientName, 40) }}</span>
              </div>
              <div class="label-row">
                <strong>Recepción</strong>
                <span>{{ formatDateOnly(order.receivedDate) }}</span>
              </div>
              <div class="label-row">
                <strong>Entrega</strong>
                <span>{{ formatDateOnly(order.deliveryDate) }}</span>
              </div>
              <div class="label-row">
                <strong>Estado</strong>
                <span>{{ order.statusLabel }}</span>
              </div>
              @if (order.dentalColor) {
                <div class="label-row">
                  <strong>Color</strong>
                  <span>{{ compact(order.dentalColor, 32) }}</span>
                </div>
              }
              <div class="label-row long-row">
                <strong>Trabajo</strong>
                <span>{{ compact(order.workDescription, 92) }}</span>
              </div>
              @if (order.notes) {
                <div class="label-row long-row">
                  <strong>Obs.</strong>
                  <span>{{ compact(order.notes, 70) }}</span>
                </div>
              }
            </div>
          </article>
        </div>
      }
    </section>
  `,
  styleUrl: './work-order-job-label-page.component.scss'
})
export class WorkOrderJobLabelPageComponent implements OnInit, OnDestroy {
  readonly order = signal<WorkOrderDetail | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly fallbackRoute = signal('/app/ordenes');

  private readonly pageStyleId = 'ldt-work-order-job-label-page-size';

  constructor(
    private readonly workOrderService: WorkOrderService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    @Inject(DOCUMENT) private readonly document: Document
  ) {}

  ngOnInit(): void {
    this.installPrintPageSize();
    this.load();
  }

  ngOnDestroy(): void {
    this.document.getElementById(this.pageStyleId)?.remove();
  }

  print(): void {
    window.print();
  }

  formatDateOnly(value: string | null): string {
    if (!value) {
      return '-';
    }

    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }

  compact(value: string | null, maxLength: number): string {
    if (!value) {
      return '-';
    }

    const normalized = value.trim().replace(/\s+/g, ' ');

    return normalized.length > maxLength ? `${normalized.slice(0, maxLength - 1)}...` : normalized;
  }

  private load(): void {
    const id = this.route.snapshot.paramMap.get('id');

    if (!id) {
      this.router.navigateByUrl('/app/ordenes');
      return;
    }

    this.fallbackRoute.set(`/app/ordenes/${id}`);
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.order.set(null);

    this.workOrderService
      .getById(id)
      .pipe(finalize(() => this.isLoading.set(false)))
      .subscribe({
        next: (order) => {
          this.order.set(order);
        },
        error: (error: HttpErrorResponse) => {
          this.errorMessage.set(this.toLoadErrorMessage(error));
          this.order.set(null);
          if (error.status === 404) {
            this.fallbackRoute.set('/app/ordenes');
          }
        }
      });
  }

  private installPrintPageSize(): void {
    this.document.getElementById(this.pageStyleId)?.remove();

    const style = this.document.createElement('style');
    style.id = this.pageStyleId;
    style.textContent = `
      @page { size: 76mm 51mm; margin: 0; }
      @media print {
        html,
        body {
          background: #ffffff !important;
          height: 51mm;
          margin: 0 !important;
          width: 76mm;
        }

        .private-shell {
          display: block !important;
          min-height: 0 !important;
        }

        .private-shell > aside,
        .private-topbar,
        .label-page-heading,
        .label-screen-actions {
          display: none !important;
        }

        .private-content {
          padding: 0 !important;
        }

        main {
          background: #ffffff !important;
        }
      }
    `;

    this.document.head.appendChild(style);
  }

  private toLoadErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 404) {
      return 'Orden no encontrada.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para ver ordenes.';
    }

    return 'No fue posible cargar la etiqueta.';
  }
}
