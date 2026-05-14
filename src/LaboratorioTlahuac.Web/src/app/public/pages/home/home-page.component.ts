import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  template: `
    <section class="hero-section">
      <div class="hero-copy">
        <p class="eyebrow">Laboratorio Dental Tláhuac</p>
        <h1>Soluciones dentales confiables para tu consulta</h1>
        <p class="hero-text">
          Sitio institucional para presentar el laboratorio y entrada al sistema administrativo para seguimiento
          operativo interno.
        </p>
        <div class="hero-actions" aria-label="Acciones principales">
          <a class="primary-action" routerLink="/catalogo">Ver catálogo</a>
          <a class="secondary-action" routerLink="/login">Iniciar sesión</a>
        </div>
      </div>
      <div class="hero-panel" aria-label="Información resumida">
        <span class="panel-label">Para revisión</span>
        <strong>Doctores, consultorios y clínicas dentales</strong>
        <p>Presencia pública separada de la operación privada bajo /app.</p>
      </div>
    </section>
    <section class="section-block" id="servicios">
      <div class="section-heading">
        <p class="eyebrow">Capacidades</p>
        <h2>Catálogo público con precios</h2>
        <p>
          La nueva ruta /catalogo muestra productos organizados por categoría, precios e imágenes locales
          cuando están disponibles.
        </p>
      </div>
      <div class="card-grid">
        <article>
          <span aria-hidden="true">01</span>
          <h3>Catálogo de servicios</h3>
          <p>Espacio reservado para describir los servicios exactos cuando el cliente confirme la lista final.</p>
        </article>
        <article>
          <span aria-hidden="true">02</span>
          <h3>Seguimiento interno</h3>
          <p>Entrada al sistema administrativo para revisar clientes, órdenes, pagos y estado operativo.</p>
        </article>
        <article>
          <span aria-hidden="true">03</span>
          <h3>Comunicación profesional</h3>
          <p>Mensaje orientado a doctores, consultorios y clínicas, pendiente de textos comerciales finales.</p>
        </article>
      </div>
    </section>
    <section class="section-block process-section" id="proceso">
      <div class="section-heading">
        <p class="eyebrow">Proceso</p>
        <h2>Una ruta simple para trabajar con el laboratorio</h2>
      </div>
      <ol class="process-list">
        <li>
          <strong>Contacto</strong>
          <span>El doctor, consultorio o clínica consulta el canal oficial cuando el cliente lo confirme.</span>
        </li>
        <li>
          <strong>Coordinación</strong>
          <span>Se revisan indicaciones, tiempos y datos necesarios para cada trabajo.</span>
        </li>
        <li>
          <strong>Seguimiento</strong>
          <span>El sistema privado concentra órdenes, estados, pagos y saldos para operación interna.</span>
        </li>
      </ol>
    </section>
    <section class="section-block benefits-section">
      <div class="section-heading">
        <p class="eyebrow">Beneficios</p>
        <h2>Diseñado para una operación dental más clara</h2>
      </div>
      <div class="benefit-list">
        <div>
          <strong>Presencia digital formal</strong>
          <span>Información institucional disponible en el dominio del laboratorio.</span>
        </div>
        <div>
          <strong>Acceso ordenado</strong>
          <span>Entrada visible al sistema administrativo mediante /login.</span>
        </div>
        <div>
          <strong>Separación segura</strong>
          <span>El sitio público no expone la aplicación privada bajo /app.</span>
        </div>
      </div>
    </section>
    <section class="contact-band" id="contacto">
      <div>
        <p class="eyebrow">Contacto</p>
        <h2>Datos listos para completarse con información confirmada</h2>
        <p>
          WhatsApp, dirección y horarios siguen pendientes de confirmación. No se publica ningún dato de contacto
          como definitivo hasta recibirlo del cliente.
        </p>
      </div>
      <div class="contact-actions">
        <a class="primary-action" routerLink="/contacto">Ver contacto</a>
        <a class="secondary-action" routerLink="/login">Iniciar sesión</a>
      </div>
    </section>
  `,
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {}
