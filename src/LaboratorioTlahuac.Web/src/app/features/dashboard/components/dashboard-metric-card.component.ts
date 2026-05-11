import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-dashboard-metric-card',
  template: `
    <article class="metric-card" [class.warning]="tone === 'warning'" [class.danger]="tone === 'danger'">
      <span>{{ label }}</span>
      <strong>{{ value }}</strong>
      @if (hint) {
        <small>{{ hint }}</small>
      }
    </article>
  `,
  styles: [
    `
      .metric-card {
        background: #ffffff;
        border: 1px solid #d8dee4;
        border-left: 4px solid #0f766e;
        border-radius: 6px;
        display: grid;
        gap: 6px;
        min-height: 104px;
        padding: 14px;
      }

      .metric-card.warning {
        border-left-color: #b45309;
      }

      .metric-card.danger {
        border-left-color: #b91c1c;
      }

      span {
        color: #4b5563;
        font-size: 0.86rem;
        font-weight: 700;
      }

      strong {
        color: #111827;
        font-size: 1.6rem;
        line-height: 1.1;
      }

      small {
        color: #6b7280;
        line-height: 1.35;
      }
    `
  ]
})
export class DashboardMetricCardComponent {
  @Input({ required: true }) label!: string;
  @Input({ required: true }) value!: string | number | null;
  @Input() hint = '';
  @Input() tone: 'default' | 'warning' | 'danger' = 'default';
}
