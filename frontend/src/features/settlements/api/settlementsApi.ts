import { httpClient } from '../../../shared/api/httpClient';
import { PagedSettlementLedgerResponse, ReconcileSettlementPayload, SettlementSummaryResponse } from '../types/settlement.types';

export const settlementsApi = {
  getLedger: async (params?: { channel?: string; status?: string; page?: number; pageSize?: number }): Promise<PagedSettlementLedgerResponse> => {
    const res = await httpClient.get<PagedSettlementLedgerResponse>('/settlements/ledger', { params });
    return res.data;
  },

  getSummary: async (): Promise<SettlementSummaryResponse> => {
    const res = await httpClient.get<SettlementSummaryResponse>('/settlements/summary');
    return res.data;
  },

  reconcileOrder: async (orderId: string, payload: ReconcileSettlementPayload): Promise<void> => {
    await httpClient.post(`/settlements/orders/${orderId}/reconcile`, payload);
  },
};
