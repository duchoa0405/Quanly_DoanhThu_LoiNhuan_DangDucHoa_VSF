import { httpClient } from '../../../shared/api/httpClient';
import { ChannelBreakdownItem, FinancialKpiResponse, TopSkuItem } from '../types/analytics.types';

export const analyticsApi = {
  getKpis: async (params?: { from?: string; to?: string; channel?: string }): Promise<FinancialKpiResponse> => {
    const res = await httpClient.get<FinancialKpiResponse>('/analytics/kpis', { params });
    return res.data;
  },

  getChannels: async (params?: { from?: string; to?: string }): Promise<ChannelBreakdownItem[]> => {
    const res = await httpClient.get<{ channels: ChannelBreakdownItem[] }>('/analytics/channels', { params });
    return res.data.channels;
  },

  getTopSkus: async (params?: { from?: string; to?: string; limit?: number; sortBy?: string }): Promise<TopSkuItem[]> => {
    const res = await httpClient.get<TopSkuItem[]>('/analytics/top-skus', { params });
    return res.data;
  },

  exportCsv: async (params?: { from?: string; to?: string }): Promise<Blob> => {
    const res = await httpClient.get('/analytics/export', {
      params,
      responseType: 'blob',
    });
    return res.data;
  },
};
