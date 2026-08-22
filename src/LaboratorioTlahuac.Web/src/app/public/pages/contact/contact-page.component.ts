import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';

import { PublicScrollAnimationsDirective } from '../../animations/public-scroll-animations.directive';

@Component({
  selector: 'app-contact-page',
  imports: [RouterLink, PublicScrollAnimationsDirective],
  template: `
    <section class="contact-v2 public-animation-scope" appPublicScrollAnimations>
      <header class="contact-v2-hero" data-animate="fade-up">
        <p class="contact-v2-eyebrow">Contacto</p>
        <h1>Hablemos del trabajo que necesitas coordinar</h1>
        <p>
          Comunícate directamente con el laboratorio para confirmar indicaciones, disponibilidad, tiempos y precio final.
        </p>
      </header>

      <div class="contact-v2-grid">
        <article class="contact-v2-channel" data-animate="stagger-card">
          <span class="contact-v2-label">Llamadas</span>
          <h2>Teléfonos del laboratorio</h2>
          <p>En celular puedes tocar cualquier número para iniciar la llamada.</p>
          <div class="contact-v2-phone-list">
            <a href="tel:+525533319445">55 3331 9445</a>
            <a href="tel:+525521612311">55 2161 2311</a>
            <a href="tel:+525598029816">55 9802 9816</a>
          </div>
        </article>

        <article class="contact-v2-channel" data-animate="stagger-card">
          <span class="contact-v2-label">Correo</span>
          <h2>Contacto por correo electrónico</h2>
          <p>Úsalo cuando necesites enviar información por escrito o dar seguimiento a una consulta.</p>
          <a class="contact-v2-email" href="mailto:contacto@laboratoriodentaltlahuac.com">
            contacto@laboratoriodentaltlahuac.com
          </a>
        </article>
      </div>

      <div class="contact-v2-prep" data-animate="fade-up">
        <div>
          <p class="contact-v2-eyebrow">Para agilizar la consulta</p>
          <h2>Ten a la mano los datos básicos del caso</h2>
          <p>Esto ayuda a que la primera conversación sea más precisa.</p>
        </div>
        <div class="contact-v2-prep-list" aria-label="Información sugerida">
          <span>Tipo de trabajo</span>
          <span>Material</span>
          <span>Indicaciones</span>
          <span>Fecha requerida</span>
        </div>
      </div>

      <p class="contact-v2-note" data-animate="fade-in">
        Dirección, horarios y WhatsApp se incorporarán cuando estén confirmados. Mientras tanto, solo se muestran los canales validados.
      </p>

      <a class="contact-v2-back" routerLink="/catalogo" data-animate="fade-up"><span aria-hidden="true">←</span> Volver al catálogo</a>
    </section>
  `,
  styleUrl: './contact-page.component.scss'
})
export class ContactPageComponent {}
