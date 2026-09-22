export type SalesChannel = 'TIKTOK' | 'SHOPEE' | 'POS';
export type PaymentMethod = 'CASH' | 'POS_CARD_QR' | 'MARKETPLACE_WALLET';
export type OrderStatus = 'PENDING' | 'SHIPPED' | 'DELIVERED' | 'CANCELLED';
export type OrderProgressStatus = 'SHIPPED' | 'DELIVERED';

export interface OrderItemSummary {
  skuCode: string;
  quantity: number;
}

export interface OrderListItemResponse {
  id: string;
  externalOrderId: string;
  channel: SalesChannel;
  paymentMethod: PaymentMethod;
  status: OrderStatus;
  orderDate: string;
  customerName?: string | null;
  customerPhone?: string | null;
  subtotal: number;
  shopVoucher: number;
  grossRevenue: number;
  itemCount: number;
  itemsSummary: OrderItemSummary[];
  deliveredAt?: string | null;
  cancelledAt?: string | null;
  createdAt: string;
}

export interface PagedOrderListResponse {
  items: OrderListItemResponse[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface OrderItemResponse {
  id: string;
  productVariantId: string;
  skuCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
  unitCostSnapshot?: number | null;
  totalCost?: number | null;
}

export interface OrderStatusHistoryResponse {
  id: string;
  fromStatus?: OrderStatus | null;
  toStatus: OrderStatus;
  reason?: string | null;
  changedBy: string;
  changedAt: string;
}

export interface FeeSnapshotResponse {
  commissionFee: number;
  paymentFee: number;
  serviceFee: number;
  fixedFee: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  snapshottedAt: string;
}

export interface OrderDetailResponse {
  id: string;
  externalOrderId: string;
  channel: SalesChannel;
  paymentMethod: PaymentMethod;
  status: OrderStatus;
  subtotal: number;
  shopVoucher: number;
  grossRevenue: number;
  customerName?: string | null;
  customerPhone?: string | null;
  orderDate: string;
  deliveredAt?: string | null;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  cogs?: number | null;
  contributionProfit?: number | null;
  items: OrderItemResponse[];
  statusHistory: OrderStatusHistoryResponse[];
  feeSnapshot?: FeeSnapshotResponse | null;
}

export interface OrderSummaryResponse {
  totalOrders: number;
  deliveredOrders: number;
  grossRevenue: number;
  inTransitOrders: number;
  cancelledOrders: number;
}

export interface FeePreviewPayload {
  channel: SalesChannel;
  paymentMethod: PaymentMethod;
  subtotal: number;
  shopVoucher: number;
}

export interface FeeBreakdownResponse {
  subtotal: number;
  shopVoucher: number;
  grossRevenue: number;
  commissionFee: number;
  paymentFee: number;
  serviceFee: number;
  fixedFee: number;
  totalPlatformFees: number;
  projectedSettlement: number;
}

export interface CreateOrderItemPayload {
  productVariantId: string;
  quantity: number;
  unitPrice: number;
}

export interface CreateOrderPayload {
  externalOrderId: string;
  channel: SalesChannel;
  paymentMethod: PaymentMethod;
  shopVoucher: number;
  customerName?: string;
  customerPhone?: string;
  items: CreateOrderItemPayload[];
}

export interface OrderFilterParams {
  channel?: SalesChannel;
  status?: OrderStatus;
  from?: string;
  to?: string;
  search?: string;
  page?: number;
  pageSize?: number;
}
