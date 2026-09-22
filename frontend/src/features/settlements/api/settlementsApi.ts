import { httpClient } from '../../../shared/api/httpClient';
import { SalesChannel } from '../../orders/types/order.types';
import {
  PagedSettlementLedgerResponse,
  ReconcileSettlementPayload,
  ReconciliationResponse,
  ReconciliationStatus,
  SettlementSummaryResponse,
} from '../types/settlement.types';

export const settlementsApi = {
  getLedger: async (params?: {
    channel?: SalesChannel;
    status?: ReconciliationStatus;
    page?: number;
    pageSize?: number;
  }): Promise<PagedSettlementLedgerResponse> => {
    const res = await httpClient.get<PagedSettlementLedgerResponse>('/settlements', { params });
    return res.data;
  },

  getSummary: async (): Promise<SettlementSummaryResponse> => {
    const res = await httpClient.get<SettlementSummaryResponse>('/settlements/summary');
    return res.data;
  },

  reconcileOrder: async (orderId: string, payload: ReconcileSettlementPayload): Promise<ReconciliationResponse> => {
    const res = await httpClient.post<ReconciliationResponse>(`/settlements/${orderId}/reconcile`, payload);
    return res.data;
  },
};
