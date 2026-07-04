import { Component, Input } from '@angular/core';

import { WorkOrderDeliveryStatus } from '../work-order.models';

@Component({
  selector: 'app-work-order-delivery-status-badge',
  template: `
    <span class="status-pill delivery-status" [class]="statusClass">
      {{ label || 'Sin entrega' }}
    </span>
  `
})
export class WorkOrderDeliveryStatusBadgeComponent {
  @Input() status: WorkOrderDeliveryStatus | null = null;
  @Input() label = 'Sin entrega';

  get statusClass(): string {
    return `status-pill delivery-status ${this.status ?? 'NoDelivery'}`;
  }
}
