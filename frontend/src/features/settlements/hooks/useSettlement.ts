import { useState, useCallback } from 'react';
import { settlementsApi } from '../api/settlementsApi';
import { SalesChannel } from '../../orders/types/order.types';
import {
  ReconcileSettlementPayload,
  ReconciliationStatus,
  SettlementLedgerItem,
  SettlementSummaryResponse,
} from '../types/settlement.types';

export const useSettlement = () => {
  const [items, setItems] = useState<SettlementLedgerItem[]>([]);
  const [summary, setSummary] = useState<SettlementSummaryResponse | null>(null);
  const [pageInfo, setPageInfo] = useState<{ page: number; pageSize: number; totalItems: number; totalPages: number }>({
    page: 1,
    pageSize: 50,
    totalItems: 0,
    totalPages: 0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchLedger = useCallback(async (params?: {
    channel?: SalesChannel;
    status?: ReconciliationStatus;
    page?: number;
    pageSize?: number;
  }) => {
    setLoading(true);
    setError(null);
    try {
      const [ledgerRes, summaryRes] = await Promise.all([
        settlementsApi.getLedger(params),
        settlementsApi.getSummary().catch(() => null),
      ]);
      setItems(ledgerRes.items || []);
      setPageInfo({
        page: ledgerRes.page,
        pageSize: ledgerRes.pageSize,
        totalItems: ledgerRes.totalItems,
        totalPages: ledgerRes.totalPages,
      });
      setSummary(summaryRes);
    } catch (err: unknown) {
      console.error('Failed to load settlements:', err);
      setError('Không thể kết nối đến sổ cái đối soát.');
    } finally {
      setLoading(false);
    }
  }, []);

  const reconcileOrder = useCallback(async (orderId: string, payload: ReconcileSettlementPayload) => {
    const res = await settlementsApi.reconcileOrder(orderId, payload);
    await fetchLedger();
    return res;
  }, [fetchLedger]);

  return {
    items,
    summary,
    pageInfo,
    loading,
    error,
    fetchLedger,
    reconcileOrder,
  };
};
