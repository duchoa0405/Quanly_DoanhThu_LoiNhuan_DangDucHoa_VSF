import { useState, useCallback } from 'react';
import { analyticsApi } from '../api/analyticsApi';
import { SalesChannel } from '../../orders/types/order.types';
import {
  ChannelBreakdownItem,
  FinancialKpiResponse,
  FinancialTrendPoint,
  PagedDrilldownOrderResponse,
  TopSkuItem,
} from '../types/analytics.types';

export const useAnalytics = () => {
  const [kpis, setKpis] = useState<FinancialKpiResponse | null>(null);
  const [channels, setChannels] = useState<ChannelBreakdownItem[]>([]);
  const [topSkus, setTopSkus] = useState<TopSkuItem[]>([]);
  const [trend, setTrend] = useState<FinancialTrendPoint[]>([]);
  const [drilldown, setDrilldown] = useState<PagedDrilldownOrderResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchDashboard = useCallback(async (params?: { from?: string; to?: string; channel?: SalesChannel }) => {
    setLoading(true);
    setError(null);
    try {
      const [kpiRes, channelRes, skuRes, trendRes] = await Promise.all([
        analyticsApi.getKpis(params),
        analyticsApi.getChannels({ from: params?.from, to: params?.to }).catch(() => []),
        analyticsApi.getTopSkus({ from: params?.from, to: params?.to, limit: 10 }).catch(() => []),
        analyticsApi.getTrend(params).catch(() => []),
      ]);

      setKpis(kpiRes);
      setChannels(channelRes || []);
      setTopSkus(skuRes || []);
      setTrend(trendRes || []);
    } catch (err: unknown) {
      console.error('Failed to load dashboard:', err);
      setError('Không thể tải dữ liệu báo cáo từ máy chủ.');
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchDrilldown = useCallback(async (params?: { from?: string; to?: string; channel?: SalesChannel; page?: number; pageSize?: number }) => {
    try {
      const res = await analyticsApi.getDrilldown(params);
      setDrilldown(res);
      return res;
    } catch (err: unknown) {
      console.error('Failed to load drilldown:', err);
      return null;
    }
  }, []);

  const exportCsv = useCallback(async (params?: { from?: string; to?: string }) => {
    return await analyticsApi.exportCsv(params);
  }, []);

  return {
    kpis,
    channels,
    topSkus,
    trend,
    drilldown,
    loading,
    error,
    fetchDashboard,
    fetchDrilldown,
    exportCsv,
  };
};
