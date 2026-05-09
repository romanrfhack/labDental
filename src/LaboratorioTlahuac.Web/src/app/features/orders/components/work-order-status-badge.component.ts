import { Component, Input } from '@angular/core';

import { WorkOrderStatus } from '../work-order.models';

@Component({
  selector: 'app-work-order-status-badge',
  template: `
    <span class="status-pill work-status" [class]="statusClass">
      {{ label || status }}
    </span>
  `
})
export class WorkOrderStatusBadgeComponent {
  @Input({ required: true }) status!: WorkOrderStatus;
  @Input() label = '';

  get statusClass(): string {
    return `status-pill work-status ${this.status}`;
  }
}
