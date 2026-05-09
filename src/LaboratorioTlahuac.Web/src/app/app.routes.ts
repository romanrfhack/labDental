import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';
import { PublicLayoutComponent } from './public/layout/public-layout.component';
import { HomePageComponent } from './public/pages/home/home-page.component';
import { ServicesPageComponent } from './public/pages/services/services-page.component';
import { ContactPageComponent } from './public/pages/contact/contact-page.component';
import { LoginPageComponent } from './auth/pages/login/login-page.component';
import { PrivateLayoutComponent } from './admin/layout/private-layout.component';
import { DashboardPageComponent } from './features/dashboard/dashboard-page.component';
import { OrdersPageComponent } from './features/orders/orders-page.component';
import { CustomersPageComponent } from './features/customers/customers-page.component';
import { PaymentsPageComponent } from './features/payments/payments-page.component';
import { InventoryPageComponent } from './features/inventory/inventory-page.component';
import { SuppliersPageComponent } from './features/suppliers/suppliers-page.component';
import { UsersPageComponent } from './admin/pages/users/users-page.component';
import { RolesPageComponent } from './admin/pages/roles/roles-page.component';
import { AccessDeniedPageComponent } from './admin/pages/access-denied/access-denied-page.component';

export const routes: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '', component: HomePageComponent, title: 'Laboratorio Dental Tlahuac' },
      { path: 'servicios', component: ServicesPageComponent, title: 'Servicios' },
      { path: 'contacto', component: ContactPageComponent, title: 'Contacto' }
    ]
  },
  { path: 'login', component: LoginPageComponent, title: 'Login' },
  {
    path: 'app',
    component: PrivateLayoutComponent,
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'access-denied',
        component: AccessDeniedPageComponent,
        title: 'Acceso denegado'
      },
      {
        path: 'dashboard',
        component: DashboardPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'reports.view' }
      },
      {
        path: 'ordenes',
        component: OrdersPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' }
      },
      {
        path: 'clientes',
        component: CustomersPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'customers.view' }
      },
      {
        path: 'pagos',
        component: PaymentsPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'payments.view' }
      },
      {
        path: 'inventario',
        component: InventoryPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'inventory.view' }
      },
      {
        path: 'proveedores',
        component: SuppliersPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'suppliers.view' }
      },
      {
        path: 'admin/usuarios',
        component: UsersPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'users.manage' }
      },
      {
        path: 'admin/roles',
        component: RolesPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'roles.manage' }
      }
    ]
  },
  { path: '**', redirectTo: '' }
];
