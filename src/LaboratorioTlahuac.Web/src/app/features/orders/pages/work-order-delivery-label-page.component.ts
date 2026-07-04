import { DOCUMENT } from '@angular/common';
import { Component, Inject, OnDestroy, OnInit, signal } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';

import { WorkOrderDetail } from '../work-order.models';
import { WorkOrderService } from '../work-order.service';

@Component({
  selector: 'app-work-order-delivery-label-page',
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
            <h1>Etiqueta entrega</h1>
            <p>{{ order.orderNumber }}</p>
          </div>
          <div class="label-screen-actions">
            <button class="primary-button" type="button" (click)="print()">Imprimir</button>
            <a class="ghost-button" [routerLink]="['/app/ordenes', order.id]">Volver a la orden</a>
          </div>
        </header>

        <div class="label-stage" aria-label="Vista previa de etiqueta de entrega 102 x 51 milimetros">
          <article class="thermal-label delivery-label">
            <header class="label-top">
              <div class="brand-line">
                <span class="brand-mark">LDT</span>
                <span class="label-kind">Entrega</span>
              </div>
              <strong class="order-number">{{ order.orderNumber }}</strong>
            </header>

            <div class="delivery-grid">
              <div class="label-column">
                <div class="label-row">
                  <strong>Cliente</strong>
                  <span>{{ compact(order.customerDisplayName, 48) }}</span>
                </div>
                <div class="label-row">
                  <strong>Paciente/ref</strong>
                  <span>{{ compact(patientReference(order), 42) }}</span>
                </div>
                <div class="label-row">
                  <strong>Entrega</strong>
                  <span>{{ formatDateOnly(order.deliveryDate) }}</span>
                </div>
                <div class="label-row long-row">
                  <strong>Trabajo</strong>
                  <span>{{ compact(order.workDescription, 70) }}</span>
                </div>
              </div>

              <div class="label-column">
                <div class="label-row long-row">
                  <strong>Dirección</strong>
                  <span>Dirección pendiente</span>
                </div>
                <div class="label-row long-row">
                  <strong>Contacto</strong>
                  <span>Contacto pendiente</span>
                </div>
                <div class="label-row">
                  <strong>Estado</strong>
                  <span>{{ order.statusLabel }}</span>
                </div>
              </div>
            </div>

            <footer class="signature-area">
              <span>Recibe: __________________</span>
              <span>Firma: __________________</span>
            </footer>
          </article>
        </div>
      }
    </section>
  `,
  styleUrl: './work-order-delivery-label-page.component.scss'
})
export class WorkOrderDeliveryLabelPageComponent implements OnInit, OnDestroy {
  readonly order = signal<WorkOrderDetail | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);
  readonly fallbackRoute = signal('/app/ordenes');

  private readonly pageStyleId = 'ldt-work-order-delivery-label-page-size';

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

  patientReference(order: WorkOrderDetail): string {
    return order.patientName || order.referenceNumber || '-';
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
      @page { size: 102mm 51mm; margin: 0; }
      @media print {
        html,
        body {
          background: #ffffff !important;
          height: 51mm;
          margin: 0 !important;
          width: 102mm;
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
