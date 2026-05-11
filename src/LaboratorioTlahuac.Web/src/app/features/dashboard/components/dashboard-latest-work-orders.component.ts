import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { WorkOrderStatusBadgeComponent } from '../../orders/components/work-order-status-badge.component';
import { DashboardWorkOrder } from '../dashboard.models';

@Component({
  selector: 'app-dashboard-latest-work-orders',
  imports: [RouterLink, WorkOrderStatusBadgeComponent],
  template: `
    <section class="dashboard-panel">
      <h3>Ultimas ordenes</h3>

      @if (items.length === 0) {
        <p class="empty-state">No hay ordenes registradas.</p>
      } @else {
        <table class="data-table compact-table">
          <thead>
            <tr>
              <th>Orden</th>
              <th>Cliente</th>
              <th>Paciente</th>
              <th>Entrega</th>
              <th>Estado</th>
            </tr>
          </thead>
          <tbody>
            @for (order of items; track order.id) {
              <tr>
                <td>
                  <a [routerLink]="['/app/ordenes', order.id]">{{ order.orderNumber }}</a>
                </td>
                <td>{{ order.customerDisplayName }}</td>
                <td>{{ order.patientName }}</td>
                <td>{{ formatDateOnly(order.deliveryDate) }}</td>
                <td>
                  <app-work-order-status-badge [status]="order.status" [label]="order.statusLabel" />
                </td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `,
  styles: [
    `
      .dashboard-panel {
        background: #ffffff;
        border: 1px solid #d8dee4;
        border-radius: 6px;
        overflow-x: auto;
        padding: 16px;
      }

      h3 {
        font-size: 1rem;
        margin: 0 0 12px;
      }

      .compact-table {
        min-width: 660px;
      }
    `
  ]
})
export class DashboardLatestWorkOrdersComponent {
  @Input({ required: true }) items: DashboardWorkOrder[] = [];

  formatDateOnly(value: string | null): string {
    if (!value) {
      return '-';
    }

    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }
}
