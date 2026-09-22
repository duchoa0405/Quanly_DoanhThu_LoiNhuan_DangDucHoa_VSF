import { SalesChannel } from '../../orders/types/order.types';
import { DiscrepancyType } from '../../settlements/types/settlement.types';

export interface DiscrepancyItemResponse {
  id: string;
  reconciliationId: string;
  orderId: string;
  externalOrderId: string;
  channel: SalesChannel;
  discrepancyType: DiscrepancyType;
  explanationNote?: string | null;
  varianceAmount: number;
  isResolved: boolean;
  createdAt: string;
}

export interface PagedDiscrepancyListResponse {
  items: DiscrepancyItemResponse[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}

export interface ResolveDiscrepancyRequest {
  resolutionNotes: string;
}

export interface DiscrepancyDetailResponse {
  id: string;
  reconciliationId: string;
  orderId: string;
  externalOrderId: string;
  channel: SalesChannel;
  discrepancyType: DiscrepancyType;
  explanationNote?: string | null;
  varianceAmount: number;
  isResolved: boolean;
  createdAt: string;
  resolvedAt?: string | null;
  resolvedBy?: string | null;
  resolutionNotes?: string | null;
}
