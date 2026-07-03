import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, signal } from '@angular/core';
import { finalize, TimeoutError, timeout } from 'rxjs';

import { AuthService } from '../../core/auth/auth.service';
import { DashboardDueSoonWorkOrdersComponent } from './components/dashboard-due-soon-work-orders.component';
import { DashboardFinancialSummaryComponent } from './components/dashboard-financial-summary.component';
import { DashboardLatestPaymentsComponent } from './components/dashboard-latest-payments.component';
import { DashboardLatestWorkOrdersComponent } from './components/dashboard-latest-work-orders.component';
import { DashboardMetricCardComponent } from './components/dashboard-metric-card.component';
import { DashboardStatusBreakdownComponent } from './components/dashboard-status-breakdown.component';
import { DashboardSummary } from './dashboard.models';
import { DashboardService } from './dashboard.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [
    DatePipe,
    DashboardDueSoonWorkOrdersComponent,
    DashboardFinancialSummaryComponent,
    DashboardLatestPaymentsComponent,
    DashboardLatestWorkOrdersComponent,
    DashboardMetricCardComponent,
    DashboardStatusBreakdownComponent
  ],
  template: `
    <section class="feature-page dashboard-page">
      <header class="page-header">
        <div>
          <h1>Dashboard</h1>
          <p>Resumen operativo básico del laboratorio.</p>
        </div>
        @if (summary(); as summary) {
          <span class="generated-at">
            Actualizado {{ summary.generatedAtUtc | date: 'short':'UTC' }}
          </span>
        }
      </header>

      @if (errorMessage(); as errorMessage) {
        <p class="alert-error" role="alert">{{ errorMessage }}</p>
      }

      @if (isLoading()) {
        <p class="loading-state">Cargando dashboard...</p>
      } @else if (summary(); as summary) {
        <section class="dashboard-section">
          <header class="section-header">
            <h2>Operacion</h2>
          </header>

          @if (summary.operationalSummary; as operational) {
            <div class="dashboard-metrics">
              <app-dashboard-metric-card
                label="Ordenes activas"
                [value]="operational.activeWorkOrdersCount"
              />
              <app-dashboard-metric-card
                label="Entregadas"
                [value]="operational.deliveredWorkOrdersCount"
              />
              <app-dashboard-metric-card
                label="Canceladas"
                [value]="operational.cancelledWorkOrdersCount"
                tone="danger"
              />
              <app-dashboard-metric-card
                label="Para hoy"
                [value]="operational.dueTodayCount"
                tone="warning"
              />
              <app-dashboard-metric-card
                label="Vencidas"
                [value]="operational.overdueCount"
                tone="danger"
              />
              <app-dashboard-metric-card
                label="Proximos 7 dias"
                [value]="operational.upcomingDueCount"
              />
            </div>

            <div class="dashboard-grid">
              <app-dashboard-status-breakdown [items]="operational.byStatus" />
              <app-dashboard-due-soon-work-orders [items]="operational.dueSoonWorkOrders" />
            </div>

            <app-dashboard-latest-work-orders [items]="operational.latestWorkOrders" />
          } @else {
            <p class="empty-state">No tienes permiso para ver metricas operativas.</p>
          }
        </section>

        <section class="dashboard-section">
          <header class="section-header">
            <h2>Cobranza</h2>
          </header>

          @if (summary.financialSummary; as financial) {
            <app-dashboard-financial-summary [summary]="financial" />
            <app-dashboard-latest-payments
              [items]="financial.latestPayments"
              [canViewOrders]="canViewOrders"
            />
          } @else {
            <p class="empty-state">No tienes permiso para ver metricas financieras.</p>
          }
        </section>

        <section class="dashboard-section">
          <header class="section-header">
            <h2>Clientes</h2>
          </header>

          @if (summary.customerSummary; as customers) {
            <div class="dashboard-metrics">
              <app-dashboard-metric-card
                label="Clientes activos"
                [value]="customers.activeCustomersCount"
              />
              <app-dashboard-metric-card
                label="Doctores activos"
                [value]="customers.activeDoctorsCount"
              />
              <app-dashboard-metric-card
                label="Clinicas activas"
                [value]="customers.activeClinicsCount"
              />
              <app-dashboard-metric-card
                label="Clientes inactivos"
                [value]="customers.inactiveCustomersCount"
                tone="warning"
              />
            </div>
          } @else {
            <p class="empty-state">No tienes permiso para ver metricas de clientes.</p>
          }
        </section>
      }
    </section>
  `,
  styles: [
    `
      .dashboard-page {
        gap: 24px;
      }

      .generated-at {
        align-self: center;
        color: #4b5563;
        font-size: 0.9rem;
        font-weight: 700;
      }

      .dashboard-section {
        display: grid;
        gap: 16px;
      }

      .section-header {
        align-items: center;
        display: flex;
        justify-content: space-between;
      }

      h2 {
        font-size: 1.25rem;
        margin: 0;
      }

      .dashboard-metrics {
        display: grid;
        gap: 14px;
        grid-template-columns: repeat(4, minmax(0, 1fr));
      }

      .dashboard-grid {
        display: grid;
        gap: 16px;
        grid-template-columns: minmax(280px, 0.85fr) minmax(0, 1.15fr);
      }

      @media (max-width: 1100px) {
        .dashboard-metrics,
        .dashboard-grid {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }

      @media (max-width: 700px) {
        .dashboard-metrics,
        .dashboard-grid {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class DashboardPageComponent implements OnInit {
  private static readonly SummaryTimeoutMs = 15000;

  readonly summary = signal<DashboardSummary | null>(null);
  readonly isLoading = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor(
    private readonly dashboardService: DashboardService,
    private readonly authService: AuthService
  ) {}

  get canViewOrders(): boolean {
    return this.authService.hasPermission('orders.view');
  }

  ngOnInit(): void {
    this.load();
  }

  private load(): void {
    this.isLoading.set(true);
    this.errorMessage.set(null);
    this.summary.set(null);

    this.dashboardService
      .getSummary()
      .pipe(
        timeout(DashboardPageComponent.SummaryTimeoutMs),
        finalize(() => this.isLoading.set(false))
      )
      .subscribe({
        next: (summary) => {
          this.summary.set(summary);
        },
        error: (error: unknown) => {
          this.summary.set(null);
          this.errorMessage.set(this.toErrorMessage(error));
        }
      });
  }

  private toErrorMessage(error: unknown): string {
    if (error instanceof TimeoutError) {
      return 'La consulta del dashboard tardo demasiado. Reintenta en unos segundos.';
    }

    if (!(error instanceof HttpErrorResponse)) {
      return 'No fue posible cargar el dashboard.';
    }

    if (error.status === 403) {
      return 'No tienes permiso para consultar el dashboard.';
    }

    if (error.status === 401) {
      return 'Inicia sesion para consultar el dashboard.';
    }

    return 'No fue posible cargar el dashboard.';
  }
}
