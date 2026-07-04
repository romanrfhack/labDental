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
      span {
        color: var(--color-neutral-500);
        font-size: 0.84rem;
        font-weight: 800;
        letter-spacing: 0.01em;
        text-transform: uppercase;
      }

      strong {
        color: var(--color-neutral-900);
        font-size: clamp(1.8rem, 1.4rem + 0.6vw, 2.2rem);
        line-height: 1.05;
      }

      small {
        color: var(--color-neutral-400);
        line-height: 1.4;
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
