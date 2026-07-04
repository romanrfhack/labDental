import { CurrencyPipe } from '@angular/common';
import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';

import { DashboardPayment } from '../dashboard.models';

@Component({
  selector: 'app-dashboard-latest-payments',
  imports: [CurrencyPipe, RouterLink],
  template: `
    <section class="dashboard-panel">
      <h3>Ultimos pagos</h3>

      @if (items.length === 0) {
        <p class="empty-state">No hay pagos vigentes registrados.</p>
      } @else {
        <table class="data-table compact-table">
          <thead>
            <tr>
              <th>Fecha</th>
              <th>Orden</th>
              <th>Cliente</th>
              <th>Paciente</th>
              <th>Monto</th>
              <th>Metodo</th>
            </tr>
          </thead>
          <tbody>
            @for (payment of items; track payment.id) {
              <tr>
                <td>{{ formatDateOnly(payment.paymentDate) }}</td>
                <td>
                  @if (canViewOrders) {
                    <a [routerLink]="['/app/ordenes', payment.workOrderId]">{{ payment.orderNumber }}</a>
                  } @else {
                    {{ payment.orderNumber }}
                  }
                </td>
                <td>{{ payment.customerDisplayName }}</td>
                <td>{{ payment.patientName }}</td>
                <td>{{ payment.amount | currency: 'MXN':'symbol-narrow' }}</td>
                <td>{{ payment.methodLabel }}</td>
              </tr>
            }
          </tbody>
        </table>
      }
    </section>
  `,
  styles: [
    `
      h3 {
        margin: 0;
      }

      .compact-table {
        min-width: 720px;
      }
    `
  ]
})
export class DashboardLatestPaymentsComponent {
  @Input({ required: true }) items: DashboardPayment[] = [];
  @Input() canViewOrders = false;

  formatDateOnly(value: string): string {
    const [year, month, day] = value.split('-');

    return `${day}/${month}/${year}`;
  }
}
