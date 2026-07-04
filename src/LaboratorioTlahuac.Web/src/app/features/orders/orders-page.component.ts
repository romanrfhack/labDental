import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-orders-page',
  imports: [RouterLink],
  template: `
    <section class="feature-page">
      <header class="page-header">
        <div>
          <h1>Ordenes de trabajo</h1>
          <p>Centro operativo para seguimiento, captura y consulta de trabajos dentales.</p>
        </div>
        <div class="page-actions">
          <a class="primary-button" routerLink="/app/ordenes/nueva">Nueva orden</a>
          <a class="ghost-button" routerLink="/app/ordenes">Ver listado</a>
        </div>
      </header>

      <section class="admin-panel stack-md">
        <div>
          <h2>Operacion diaria</h2>
          <p class="text-muted">Desde aqui puedes revisar entregas, estados, importes y acceso rapido a cada orden.</p>
        </div>

        <div class="detail-grid">
          <div class="detail-item">
            <strong>Captura</strong>
            <span>Registra nuevas ordenes con cliente, paciente, trabajo y fecha de entrega.</span>
          </div>
          <div class="detail-item">
            <strong>Seguimiento</strong>
            <span>Consulta el estado del trabajo, pagos vinculados y etiquetas operativas.</span>
          </div>
        </div>
      </section>
    </section>
  `
})
export class OrdersPageComponent {}
