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
import { WorkOrderCreatePageComponent } from './features/orders/pages/work-order-create-page.component';
import { WorkOrderDetailPageComponent } from './features/orders/pages/work-order-detail-page.component';
import { WorkOrderEditPageComponent } from './features/orders/pages/work-order-edit-page.component';
import { WorkOrderListPageComponent } from './features/orders/pages/work-order-list-page.component';
import { CustomerCreatePageComponent } from './features/customers/pages/customer-create-page.component';
import { CustomerDetailPageComponent } from './features/customers/pages/customer-detail-page.component';
import { CustomerEditPageComponent } from './features/customers/pages/customer-edit-page.component';
import { CustomerListPageComponent } from './features/customers/pages/customer-list-page.component';
import { PaymentListPageComponent } from './features/payments/pages/payment-list-page.component';
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
        component: WorkOrderListPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' }
      },
      {
        path: 'ordenes/nueva',
        component: WorkOrderCreatePageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'orders.create' }
      },
      {
        path: 'ordenes/:id/editar',
        component: WorkOrderEditPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'orders.edit' }
      },
      {
        path: 'ordenes/:id',
        component: WorkOrderDetailPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' }
      },
      {
        path: 'clientes',
        component: CustomerListPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'customers.view' }
      },
      {
        path: 'clientes/nuevo',
        component: CustomerCreatePageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'customers.create' }
      },
      {
        path: 'clientes/:id/editar',
        component: CustomerEditPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'customers.edit' }
      },
      {
        path: 'clientes/:id',
        component: CustomerDetailPageComponent,
        canActivate: [permissionGuard],
        data: { permission: 'customers.view' }
      },
      {
        path: 'pagos',
        component: PaymentListPageComponent,
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
