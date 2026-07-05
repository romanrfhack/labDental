import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { permissionGuard } from './core/guards/permission.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./public/layout/public-layout.component').then((m) => m.PublicLayoutComponent),
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./public/pages/home/home-page.component').then((m) => m.HomePageComponent),
        title: 'Laboratorio Dental Tláhuac',
      },
      {
        path: 'catalogo',
        loadComponent: () =>
          import('./public/pages/catalog/catalog-page.component').then(
            (m) => m.CatalogPageComponent,
          ),
        title: 'Catálogo | Laboratorio Dental Tláhuac',
      },
      {
        path: 'servicios',
        loadComponent: () =>
          import('./public/pages/services/services-page.component').then(
            (m) => m.ServicesPageComponent,
          ),
        title: 'Servicios | Laboratorio Dental Tláhuac',
      },
      {
        path: 'contacto',
        loadComponent: () =>
          import('./public/pages/contact/contact-page.component').then(
            (m) => m.ContactPageComponent,
          ),
        title: 'Contacto | Laboratorio Dental Tláhuac',
      },
    ],
  },
  {
    path: 'login',
    loadComponent: () =>
      import('./auth/pages/login/login-page.component').then((m) => m.LoginPageComponent),
    title: 'Login',
  },
  {
    path: 'app',
    loadComponent: () =>
      import('./admin/layout/private-layout.component').then((m) => m.PrivateLayoutComponent),
    canActivate: [authGuard],
    children: [
      { path: '', pathMatch: 'full', redirectTo: 'dashboard' },
      {
        path: 'access-denied',
        loadComponent: () =>
          import('./admin/pages/access-denied/access-denied-page.component').then(
            (m) => m.AccessDeniedPageComponent,
          ),
        title: 'Acceso denegado',
      },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./features/dashboard/dashboard-page.component').then(
            (m) => m.DashboardPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'reports.view' },
      },
      {
        path: 'entregas',
        loadComponent: () =>
          import('./features/deliveries/pages/delivery-list-page.component').then(
            (m) => m.DeliveryListPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'deliveries.view' },
      },
      {
        path: 'entregas/:id',
        loadComponent: () =>
          import('./features/deliveries/pages/delivery-detail-page.component').then(
            (m) => m.DeliveryDetailPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'deliveries.view' },
      },
      {
        path: 'ordenes',
        loadComponent: () =>
          import('./features/orders/pages/work-order-list-page.component').then(
            (m) => m.WorkOrderListPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' },
      },
      {
        path: 'ordenes/nueva',
        loadComponent: () =>
          import('./features/orders/pages/work-order-create-page.component').then(
            (m) => m.WorkOrderCreatePageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.create' },
      },
      {
        path: 'ordenes/:id/editar',
        loadComponent: () =>
          import('./features/orders/pages/work-order-edit-page.component').then(
            (m) => m.WorkOrderEditPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.edit' },
      },
      {
        path: 'ordenes/:id/etiqueta-trabajo',
        loadComponent: () =>
          import('./features/orders/pages/work-order-job-label-page.component').then(
            (m) => m.WorkOrderJobLabelPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' },
      },
      {
        path: 'ordenes/:id/etiqueta-entrega',
        loadComponent: () =>
          import('./features/orders/pages/work-order-delivery-label-page.component').then(
            (m) => m.WorkOrderDeliveryLabelPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' },
      },
      {
        path: 'ordenes/:id',
        loadComponent: () =>
          import('./features/orders/pages/work-order-detail-page.component').then(
            (m) => m.WorkOrderDetailPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'orders.view' },
      },
      {
        path: 'clientes',
        loadComponent: () =>
          import('./features/customers/pages/customer-list-page.component').then(
            (m) => m.CustomerListPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'customers.view' },
      },
      {
        path: 'clientes/nuevo',
        loadComponent: () =>
          import('./features/customers/pages/customer-create-page.component').then(
            (m) => m.CustomerCreatePageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'customers.create' },
      },
      {
        path: 'clientes/:id/editar',
        loadComponent: () =>
          import('./features/customers/pages/customer-edit-page.component').then(
            (m) => m.CustomerEditPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'customers.edit' },
      },
      {
        path: 'clientes/:id',
        loadComponent: () =>
          import('./features/customers/pages/customer-detail-page.component').then(
            (m) => m.CustomerDetailPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'customers.view' },
      },
      {
        path: 'pagos',
        loadComponent: () =>
          import('./features/payments/pages/payment-list-page.component').then(
            (m) => m.PaymentListPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'payments.view' },
      },
      {
        path: 'inventario',
        loadComponent: () =>
          import('./features/inventory/inventory-page.component').then(
            (m) => m.InventoryPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'inventory.view' },
      },
      {
        path: 'proveedores',
        loadComponent: () =>
          import('./features/suppliers/suppliers-page.component').then(
            (m) => m.SuppliersPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'suppliers.view' },
      },
      {
        path: 'admin/usuarios',
        loadComponent: () =>
          import('./admin/pages/users/users-page.component').then((m) => m.UsersPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'users.manage' },
      },
      {
        path: 'admin/catalogo',
        loadComponent: () =>
          import('./features/catalog/pages/admin-catalog-page.component').then(
            (m) => m.AdminCatalogPageComponent,
          ),
        canActivate: [permissionGuard],
        data: { permission: 'catalog.view' },
        title: 'Catalogo',
      },
      {
        path: 'admin/roles',
        loadComponent: () =>
          import('./admin/pages/roles/roles-page.component').then((m) => m.RolesPageComponent),
        canActivate: [permissionGuard],
        data: { permission: 'roles.manage' },
      },
    ],
  },
  { path: '**', redirectTo: '' },
];
