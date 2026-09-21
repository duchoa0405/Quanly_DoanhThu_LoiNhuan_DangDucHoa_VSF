export type SalesChannel = 'TikTokShop' | 'Shopee' | 'Pos';
export type PaymentMethod = 'Cod' | 'OnlineBanking' | 'EWallet';
export type OrderStatus = 'Pending' | 'Confirmed' | 'InTransit' | 'Delivered' | 'Cancelled' | 'Returned';

export interface OrderItemResponse {
  id: string;
  productVariantId: string;
  skuCode: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  unitCost?: number | null;
  lineTotal: number;
  totalCost?: number | null;
}

export interface FeeSnapshotResponse {
  id: string;
  commissionRate: number;
  commissionFeeAmount: number;
  paymentFeeRate: number;
  paymentFeeAmount: number;
  serviceFeeRate: number;
  serviceFeeAmount: number;
  serviceFeeCapSnapshot?: number | null;
  fixedFeeAmount: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  snapshotAt: string;
}

export interface ReconciliationSummaryResponse {
  id: string;
  status: string;
  projectedSettlement: number;
  actualSettlement?: number | null;
  varianceAmount?: number | null;
  reconciledAt?: string | null;
}

export interface OrderResponse {
  id: string;
  externalOrderId: string;
  channel: SalesChannel | string;
  paymentMethod: PaymentMethod | string;
  status: OrderStatus | string;
  subtotal: number;
  shopVoucher: number;
  grossRevenue: number;
  customerName?: string | null;
  customerPhone?: string | null;
  orderDate: string;
  deliveredAt?: string | null;
  cancelledAt?: string | null;
  cancellationReason?: string | null;
  totalCost?: number | null;
  profit?: number | null;
  items: OrderItemResponse[];
  feeSnapshot?: FeeSnapshotResponse | null;
  reconciliationRecord?: ReconciliationSummaryResponse | null;
}

export interface OrderSummaryResponse {
  totalOrders: number;
  deliveredOrders: number;
  grossRevenue: number;
  inTransitOrders: number;
  cancelledOrders: number;
}

export interface FeePreviewPayload {
  channel: string;
  paymentMethod: string;
  subtotal: number;
  shopVoucher: number;
}

export interface FeePreviewResponse {
  channel: string;
  paymentMethod: string;
  subtotal: number;
  shopVoucher: number;
  grossRevenue: number;
  commissionFeeAmount: number;
  paymentFeeAmount: number;
  serviceFeeAmount: number;
  fixedFeeAmount: number;
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
  channel: string;
  paymentMethod: string;
  subtotal: number;
  shopVoucher: number;
  customerName?: string;
  customerPhone?: string;
  orderDate: string;
  items: CreateOrderItemPayload[];
}

export interface OrderFilterParams {
  channel?: string;
  status?: string;
  paymentMethod?: string;
  search?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
}
