import { Component, Input } from '@angular/core';

import { WorkOrderStatusSummary } from '../dashboard.models';

@Component({
  selector: 'app-dashboard-status-breakdown',
  template: `
    <section class="dashboard-panel">
      <h3>Estados</h3>
      <div class="status-grid">
        @for (item of items; track item.status) {
          <div class="status-row">
            <span class="status-pill work-status {{ item.status }}">{{ item.label }}</span>
            <strong>{{ item.count }}</strong>
          </div>
        }
      </div>
    </section>
  `,
  styles: [
    `
      h3 {
        margin: 0;
      }

      .status-grid {
        display: grid;
        gap: var(--space-3);
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }

      .status-row {
        align-items: center;
        background: rgba(243, 248, 252, 0.78);
        border: 1px solid rgba(215, 227, 239, 0.88);
        border-radius: var(--radius-sm);
        display: flex;
        gap: var(--space-3);
        justify-content: space-between;
        padding: 10px 12px;
      }

      strong {
        color: var(--color-neutral-900);
      }

      @media (max-width: 700px) {
        .status-grid {
          grid-template-columns: 1fr;
        }
      }
    `
  ]
})
export class DashboardStatusBreakdownComponent {
  @Input({ required: true }) items: WorkOrderStatusSummary[] = [];
}
