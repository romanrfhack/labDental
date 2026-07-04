import { CurrencyPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DashboardMetricCardComponent } from './dashboard-metric-card.component';
import { FinancialSummary } from '../dashboard.models';

@Component({
  selector: 'app-dashboard-financial-summary',
  imports: [CurrencyPipe, DashboardMetricCardComponent, RouterLink],
  template: `
    <div class="section-toolbar">
      <a class="ghost-button" routerLink="/app/pagos">Ver pagos</a>
    </div>

    <div class="dashboard-metrics">
      <app-dashboard-metric-card
        label="Por cobrar"
        [value]="summary.totalReceivable | currency: 'MXN':'symbol-narrow'"
        hint="Solo saldos positivos"
      />
      <app-dashboard-metric-card
        label="Ordenes con saldo"
        [value]="summary.ordersWithPendingBalanceCount"
        tone="warning"
      />
      <app-dashboard-metric-card label="Pagadas" [value]="summary.paidOrdersCount" />
      <app-dashboard-metric-card
        label="Parciales"
        [value]="summary.partialPaymentOrdersCount"
        tone="warning"
      />
      <app-dashboard-metric-card label="Sin pago" [value]="summary.unpaidOrdersCount" tone="danger" />
      <app-dashboard-metric-card label="Sobrepagadas" [value]="summary.overpaidOrdersCount" />
      <app-dashboard-metric-card
        label="Pagos cancelados"
        [value]="summary.cancelledPaymentsCount"
        tone="danger"
      />
    </div>
  `,
  styles: [
    `
      .section-toolbar {
        margin-bottom: var(--space-4);
      }

      .dashboard-metrics {
        display: grid;
        gap: var(--space-4);
        grid-template-columns: repeat(4, minmax(0, 1fr));
      }

      @media (max-width: 1100px) {
        .dashboard-metrics {
          grid-template-columns: repeat(2, minmax(0, 1fr));
        }
      }

      @media (max-width: 620px) {
        .dashboard-metrics {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class DashboardFinancialSummaryComponent {
  @Input({ required: true }) summary!: FinancialSummary;
}
