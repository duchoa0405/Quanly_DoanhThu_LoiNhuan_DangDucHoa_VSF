export interface SettlementLedgerItem {
  id: string;
  orderId: string;
  externalOrderId: string;
  channel: 'TIKTOK' | 'SHOPEE' | 'POS';
  grossRevenue: number;
  commissionFee: number;
  paymentFee: number;
  serviceFee: number;
  fixedFee: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  actualSettlement?: number | null;
  varianceAmount?: number | null;
  reconciliationStatus: 'PENDING_SETTLEMENT' | 'RECONCILED' | 'DISCREPANCY';
  deliveredAt: string;
  reconciledAt?: string | null;
}

export interface PagedSettlementLedgerResponse {
  items: SettlementLedgerItem[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface SettlementSummaryResponse {
  pendingSettlementCount: number;
  reconciledCount: number;
  discrepancyCount: number;
}

export interface ReconcileSettlementPayload {
  actualSettlement: number;
  notes?: string;
  discrepancyType?: 'COMMISSION_RATE_MISMATCH' | 'PAYMENT_FEE_MISMATCH' | 'SERVICE_FEE_MISMATCH' | 'PLATFORM_FEE_OVERCHARGE' | 'OTHER';
}
