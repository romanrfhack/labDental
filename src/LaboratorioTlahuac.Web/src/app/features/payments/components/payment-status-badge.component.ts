import { Component, Input } from '@angular/core';

import { PaymentStatus } from '../payment.models';

@Component({
  selector: 'app-payment-status-badge',
  template: `
    <span
      class="status-pill payment-status"
      [class.TotalNotSet]="status === 'TotalNotSet'"
      [class.Unpaid]="status === 'Unpaid'"
      [class.Partial]="status === 'Partial'"
      [class.Paid]="status === 'Paid'"
      [class.Overpaid]="status === 'Overpaid'"
    >
      {{ label || status }}
    </span>
  `
})
export class PaymentStatusBadgeComponent {
  @Input({ required: true }) status!: PaymentStatus;
  @Input() label = '';
}
