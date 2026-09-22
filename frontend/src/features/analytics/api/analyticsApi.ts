import { httpClient } from '../../../shared/api/httpClient';
import { SalesChannel } from '../../orders/types/order.types';
import {
  ChannelBreakdownItem,
  ChannelBreakdownListResponse,
  FinancialKpiResponse,
  FinancialTrendPoint,
  FinancialTrendResponse,
  PagedDrilldownOrderResponse,
  TopSkuItem,
  TopSkuListResponse,
} from '../types/analytics.types';

export const analyticsApi = {
  getKpis: async (params?: { from?: string; to?: string; channel?: SalesChannel }): Promise<FinancialKpiResponse> => {
    const res = await httpClient.get<FinancialKpiResponse>('/analytics/kpis', { params });
    return res.data;
  },

  getTrend: async (params?: { from?: string; to?: string; channel?: SalesChannel }): Promise<FinancialTrendPoint[]> => {
    const res = await httpClient.get<FinancialTrendResponse>('/analytics/trend', { params });
    return res.data.points || [];
  },

  getChannels: async (params?: { from?: string; to?: string }): Promise<ChannelBreakdownItem[]> => {
    const res = await httpClient.get<ChannelBreakdownListResponse>('/analytics/channel-breakdown', { params });
    return res.data.channels || [];
  },

  getTopSkus: async (params?: { from?: string; to?: string; limit?: number; sortBy?: string }): Promise<TopSkuItem[]> => {
    const res = await httpClient.get<TopSkuListResponse>('/analytics/top-skus', { params });
    return res.data.items || [];
  },

  getDrilldown: async (params?: { from?: string; to?: string; channel?: SalesChannel; page?: number; pageSize?: number }): Promise<PagedDrilldownOrderResponse> => {
    const res = await httpClient.get<PagedDrilldownOrderResponse>('/analytics/drilldown', { params });
    return res.data;
  },

  exportCsv: async (params?: { from?: string; to?: string }): Promise<Blob> => {
    const res = await httpClient.get('/analytics/export-csv', {
      params,
      responseType: 'blob',
    });
    return res.data;
  },
};
