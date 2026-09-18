// Mirrors the DTOs exposed by ECommerce.Api. Kept as plain interfaces (no classes) since
// they are pure data shapes coming straight off HTTP responses.

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}

// ----- Auth -----
export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  refreshToken: string;
  userId: string;
  email: string;
  fullName: string;
  roles: string[];
}

export interface UserProfile {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
}

// ----- Categories -----
export interface Category {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  imageUrl: string | null;
  isActive: boolean;
  displayOrder: number;
  parentCategoryId: number | null;
  productCount: number;
  subCategories: Category[];
}

// ----- Products -----
export interface ProductImage {
  id: number;
  url: string;
  displayOrder: number;
  isPrimary: boolean;
}

export interface ProductListItem {
  id: number;
  name: string;
  slug: string;
  price: number;
  compareAtPrice: number | null;
  primaryImageUrl: string | null;
  inStock: boolean;
  averageRating: number;
  reviewCount: number;
  categoryId: number;
  categoryName: string;
}

export interface ProductDetail {
  id: number;
  name: string;
  slug: string;
  description: string | null;
  sku: string;
  price: number;
  compareAtPrice: number | null;
  stockQuantity: number;
  isActive: boolean;
  averageRating: number;
  reviewCount: number;
  categoryId: number;
  categoryName: string;
  images: ProductImage[];
  createdAt: string;
}

export interface ProductQueryParams {
  search?: string;
  categoryId?: number;
  minPrice?: number;
  maxPrice?: number;
  inStockOnly?: boolean;
  sortBy?: 'price_asc' | 'price_desc' | 'newest' | 'rating' | 'name';
  page?: number;
  pageSize?: number;
  includeInactive?: boolean;
}

// ----- Cart -----
export interface CartItem {
  id: number;
  productId: number;
  productName: string;
  productImageUrl: string | null;
  productSlug: string;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
  availableStock: number;
}

export interface Cart {
  id: number;
  items: CartItem[];
  subtotal: number;
  totalItems: number;
}

// ----- Addresses -----
export interface Address {
  id: number;
  fullName: string;
  phone: string;
  line1: string;
  line2: string | null;
  city: string;
  province: string;
  postalCode: string | null;
  country: string;
  isDefault: boolean;
}

export type AddressInput = Omit<Address, 'id'>;

// ----- Orders -----
export type OrderStatus = 'PendingPayment' | 'Paid' | 'Processing' | 'Shipped' | 'Delivered' | 'Cancelled' | 'Refunded';
export type PaymentMethod = 'CashOnDelivery' | 'VnPay' | 'Momo';
export type PaymentStatus = 'Pending' | 'Succeeded' | 'Failed' | 'Refunded';

export interface OrderItem {
  productId: number;
  productName: string;
  productImageUrl: string | null;
  unitPrice: number;
  quantity: number;
  lineTotal: number;
}

export interface Order {
  id: number;
  orderNumber: string;
  status: OrderStatus;
  subtotal: number;
  discountAmount: number;
  shippingFee: number;
  total: number;
  couponCode: string | null;
  shippingFullName: string;
  shippingPhone: string;
  shippingLine1: string;
  shippingLine2: string | null;
  shippingCity: string;
  shippingProvince: string;
  shippingPostalCode: string | null;
  paymentMethod: PaymentMethod;
  paymentStatus: PaymentStatus;
  customerNote: string | null;
  createdAt: string;
  items: OrderItem[];
}

export interface OrderSummary {
  id: number;
  orderNumber: string;
  status: OrderStatus;
  total: number;
  paymentStatus: PaymentStatus;
  createdAt: string;
  itemCount: number;
}

export interface PlaceOrderRequest {
  addressId: number;
  paymentMethod: PaymentMethod;
  couponCode?: string | null;
  customerNote?: string | null;
}

export interface PlaceOrderResult {
  order: Order;
  paymentRedirectUrl: string | null;
}

// ----- Wishlist -----
export interface WishlistItem {
  id: number;
  productId: number;
  productName: string;
  productSlug: string;
  productImageUrl: string | null;
  price: number;
  inStock: boolean;
}

// ----- Reviews -----
export interface Review {
  id: number;
  productId: number;
  userDisplayName: string;
  rating: number;
  comment: string | null;
  createdAt: string;
}

// ----- Coupons -----
export type DiscountType = 'Percentage' | 'FixedAmount';

export interface Coupon {
  id: number;
  code: string;
  discountType: DiscountType;
  discountValue: number;
  minOrderAmount: number | null;
  maxDiscountAmount: number | null;
  maxUsageCount: number | null;
  usageCount: number;
  expiresAt: string | null;
  isActive: boolean;
}

export interface ValidateCouponResponse {
  isValid: boolean;
  discountAmount: number;
  message: string | null;
}

// ----- Admin -----
export interface DashboardSummary {
  totalProducts: number;
  totalCategories: number;
  totalCustomers: number;
  totalOrders: number;
  pendingOrders: number;
  totalRevenue: number;
  revenueLast30Days: number;
  topProducts: { productId: number; name: string; unitsSold: number; revenue: number }[];
  recentOrders: { id: number; orderNumber: string; customerEmail: string; total: number; status: string; createdAt: string }[];
  lowStockProducts: { productId: number; name: string; stockQuantity: number }[];
}

export interface Customer {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  isLocked: boolean;
}

// ----- Error shape from ExceptionHandlingMiddleware -----
export interface ApiProblem {
  status: number;
  title: string;
  errors?: Record<string, string[]>;
  traceId: string;
}
