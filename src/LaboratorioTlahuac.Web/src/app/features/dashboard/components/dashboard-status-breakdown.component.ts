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
      .dashboard-panel {
        background: #ffffff;
        border: 1px solid #d8dee4;
        border-radius: 6px;
        padding: 16px;
      }

      h3 {
        font-size: 1rem;
        margin: 0 0 12px;
      }

      .status-grid {
        display: grid;
        gap: 10px;
        grid-template-columns: repeat(2, minmax(0, 1fr));
      }

      .status-row {
        align-items: center;
        display: flex;
        gap: 10px;
        justify-content: space-between;
      }

      strong {
        color: #111827;
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
