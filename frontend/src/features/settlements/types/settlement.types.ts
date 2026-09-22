import { SalesChannel } from '../../orders/types/order.types';

export type ReconciliationStatus = 'PENDING_SETTLEMENT' | 'RECONCILED' | 'DISCREPANCY';

export type DiscrepancyType =
  | 'COMMISSION_RATE_MISMATCH'
  | 'PAYMENT_FEE_MISMATCH'
  | 'SERVICE_FEE_MISMATCH'
  | 'UNEXPECTED_PLATFORM_CHARGE'
  | 'OTHER';

export interface SettlementLedgerItem {
  id: string;
  orderId: string;
  externalOrderId: string;
  channel: SalesChannel;
  grossRevenue: number;
  commissionFee: number;
  paymentFee: number;
  serviceFee: number;
  fixedFee: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  actualSettlement?: number | null;
  varianceAmount?: number | null;
  reconciliationStatus: ReconciliationStatus;
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
  discrepancyType?: DiscrepancyType;
}

export interface ReconciliationResponse {
  id: string;
  orderId: string;
  projectedSettlement: number;
  actualSettlement: number;
  varianceAmount: number;
  reconciliationStatus: ReconciliationStatus;
  reconciliationNotes?: string | null;
  reconciledAt: string;
  reconciledBy: string;
}
