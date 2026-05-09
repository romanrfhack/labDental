import { DatePipe } from '@angular/common';
import { Component, Input } from '@angular/core';

import { WorkOrderStatusHistory } from '../work-order.models';

@Component({
  selector: 'app-work-order-status-history-section',
  imports: [DatePipe],
  template: `
    <section class="feature-page">
      <h2>Historial de estado</h2>

      @if (history.length === 0) {
        <p class="empty-state">Sin cambios de estado registrados.</p>
      } @else {
        <table class="data-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Cambio</th>
              <th>Notas</th>
            </tr>
          </thead>
          <tbody>
            @for (item of history; track item.id) {
              <tr>
                <td>{{ item.changedAtUtc | date: 'medium' }}</td>
                <td>{{ formatTransition(item) }}</td>
                <td>{{ item.notes || '-' }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `
})
export class WorkOrderStatusHistorySectionComponent {
  @Input() history: WorkOrderStatusHistory[] = [];

  formatTransition(item: WorkOrderStatusHistory): string {
    return item.fromStatusLabel
      ? `${item.fromStatusLabel} -> ${item.toStatusLabel}`
      : item.toStatusLabel;
  }
}
