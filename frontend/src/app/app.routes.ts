import { Routes } from '@angular/router';
import { authGuard, adminGuard, guestGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/home/home').then(m => m.HomeComponent)
  },
  {
    path: 'products',
    loadComponent: () => import('./features/catalog/product-list/product-list').then(m => m.ProductListComponent)
  },
  {
    path: 'products/:slug',
    loadComponent: () => import('./features/catalog/product-detail/product-detail').then(m => m.ProductDetailComponent)
  },
  {
    path: 'cart',
    loadComponent: () => import('./features/cart/cart-page').then(m => m.CartPageComponent)
  },
  {
    path: 'wishlist',
    canActivate: [authGuard],
    loadComponent: () => import('./features/wishlist/wishlist-page').then(m => m.WishlistPageComponent)
  },
  {
    path: 'checkout',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/checkout-page/checkout-page').then(m => m.CheckoutPageComponent)
  },
  {
    path: 'checkout/payment-result',
    canActivate: [authGuard],
    loadComponent: () => import('./features/checkout/payment-result/payment-result').then(m => m.PaymentResultComponent)
  },
  {
    path: 'orders',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/order-list/order-list').then(m => m.OrderListComponent)
  },
  {
    path: 'orders/:id',
    canActivate: [authGuard],
    loadComponent: () => import('./features/orders/order-detail/order-detail').then(m => m.OrderDetailComponent)
  },
  {
    path: 'account/profile',
    canActivate: [authGuard],
    loadComponent: () => import('./features/account/profile/profile').then(m => m.ProfileComponent)
  },
  {
    path: 'account/addresses',
    canActivate: [authGuard],
    loadComponent: () => import('./features/account/addresses/addresses').then(m => m.AddressesComponent)
  },
  {
    path: 'login',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/login/login').then(m => m.LoginComponent)
  },
  {
    path: 'register',
    canActivate: [guestGuard],
    loadComponent: () => import('./features/auth/register/register').then(m => m.RegisterComponent)
  },
  {
    path: 'admin',
    canActivate: [adminGuard],
    loadComponent: () => import('./features/admin/admin-layout/admin-layout').then(m => m.AdminLayoutComponent),
    children: [
      { path: '', loadComponent: () => import('./features/admin/dashboard/dashboard').then(m => m.AdminDashboardComponent) },
      { path: 'products', loadComponent: () => import('./features/admin/products/product-list-admin').then(m => m.AdminProductListComponent) },
      { path: 'products/new', loadComponent: () => import('./features/admin/products/product-form').then(m => m.AdminProductFormComponent) },
      { path: 'products/:id/edit', loadComponent: () => import('./features/admin/products/product-form').then(m => m.AdminProductFormComponent) },
      { path: 'categories', loadComponent: () => import('./features/admin/categories/category-list-admin').then(m => m.AdminCategoryListComponent) },
      { path: 'orders', loadComponent: () => import('./features/admin/orders/order-list-admin').then(m => m.AdminOrderListComponent) },
      { path: 'orders/:id', loadComponent: () => import('./features/admin/orders/order-detail-admin').then(m => m.AdminOrderDetailComponent) },
      { path: 'customers', loadComponent: () => import('./features/admin/customers/customer-list').then(m => m.AdminCustomerListComponent) },
      { path: 'coupons', loadComponent: () => import('./features/admin/coupons/coupon-list').then(m => m.AdminCouponListComponent) }
    ]
  },
  {
    path: 'field-service-demo',
    loadComponent: () => import('./features/field-service-demo/field-service-demo').then(m => m.FieldServiceDemoComponent)
  },
  { path: '**', redirectTo: '' }
];
