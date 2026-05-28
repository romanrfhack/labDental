import { Component } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-private-layout',
  imports: [AsyncPipe, RouterLink, RouterLinkActive, RouterOutlet],
  template: `
    <div class="private-shell">
      <aside>
        <a routerLink="/app/dashboard" class="brand">LDT Admin</a>
        <nav>
          @if (authService.hasPermission('reports.view')) {
            <a
              routerLink="/app/dashboard"
              routerLinkActive="is-active"
              [routerLinkActiveOptions]="{ exact: true }"
              ariaCurrentWhenActive="page"
            >
              Dashboard
            </a>
          }
          @if (authService.hasPermission('orders.view')) {
            <a routerLink="/app/ordenes" routerLinkActive="is-active" ariaCurrentWhenActive="page">Ordenes</a>
          }
          @if (authService.hasPermission('customers.view')) {
            <a routerLink="/app/clientes" routerLinkActive="is-active" ariaCurrentWhenActive="page">Clientes</a>
          }
          @if (authService.hasPermission('payments.view')) {
            <a routerLink="/app/pagos" routerLinkActive="is-active" ariaCurrentWhenActive="page">Pagos</a>
          }
          @if (authService.hasPermission('inventory.view')) {
            <a routerLink="/app/inventario" routerLinkActive="is-active" ariaCurrentWhenActive="page">Inventario</a>
          }
          @if (authService.hasPermission('suppliers.view')) {
            <a routerLink="/app/proveedores" routerLinkActive="is-active" ariaCurrentWhenActive="page"
              >Proveedores</a>
          }
          @if (authService.hasPermission('users.manage')) {
            <a routerLink="/app/admin/usuarios" routerLinkActive="is-active" ariaCurrentWhenActive="page"
              >Usuarios</a>
          }
          @if (authService.hasPermission('roles.manage')) {
            <a routerLink="/app/admin/roles" routerLinkActive="is-active" ariaCurrentWhenActive="page">Roles</a>
          }
        </nav>
      </aside>
      <main>
        <header class="private-topbar">
          @if (authService.currentUser$ | async; as user) {
            <span>{{ user.fullName || user.email }}</span>
          }
          <button type="button" (click)="logout()">Salir</button>
        </header>
        <section class="private-content">
          <router-outlet />
        </section>
      </main>
    </div>
  `,
  styleUrl: './private-layout.component.scss'
})
export class PrivateLayoutComponent {
  constructor(
    readonly authService: AuthService,
    private readonly router: Router
  ) {}

  logout() {
    this.authService.logout().subscribe({
      next: () => this.router.navigateByUrl('/login')
    });
  }
}
