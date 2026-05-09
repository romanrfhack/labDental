import { CurrencyPipe } from '@angular/common';
import { Component, Input } from '@angular/core';

import { PaymentSummary } from '../payment.models';
import { PaymentStatusBadgeComponent } from './payment-status-badge.component';

@Component({
  selector: 'app-payment-summary-card',
  imports: [CurrencyPipe, PaymentStatusBadgeComponent],
  template: `
    <section class="payment-summary" aria-label="Resumen financiero">
      <div class="payment-summary-grid">
        <div class="detail-item">
          <strong>Total</strong>
          <span>{{ summary.totalAmount === null ? '-' : (summary.totalAmount | currency: 'MXN':'symbol-narrow') }}</span>
        </div>
        <div class="detail-item">
          <strong>Pagado</strong>
          <span>{{ summary.paidAmount | currency: 'MXN':'symbol-narrow' }}</span>
        </div>
        <div class="detail-item">
          <strong>Saldo</strong>
          <span>{{ summary.balance === null ? '-' : (summary.balance | currency: 'MXN':'symbol-narrow') }}</span>
        </div>
        <div class="detail-item">
          <strong>Estado financiero</strong>
          <app-payment-status-badge
            [status]="summary.paymentStatus"
            [label]="summary.paymentStatusLabel"
          />
        </div>
      </div>
    </section>
  `
})
export class PaymentSummaryCardComponent {
  @Input({ required: true }) summary!: PaymentSummary;
}
