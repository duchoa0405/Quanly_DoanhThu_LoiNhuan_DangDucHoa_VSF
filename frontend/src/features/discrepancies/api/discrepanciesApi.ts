import { httpClient } from '../../../shared/api/httpClient';
import { SalesChannel } from '../../orders/types/order.types';
import { DiscrepancyType } from '../../settlements/types/settlement.types';
import {
  DiscrepancyDetailResponse,
  PagedDiscrepancyListResponse,
  ResolveDiscrepancyRequest,
} from '../types/discrepancy.types';

export const discrepanciesApi = {
  getDiscrepancies: async (params?: {
    channel?: SalesChannel;
    discrepancyType?: DiscrepancyType;
    isResolved?: boolean;
    from?: string;
    to?: string;
    search?: string;
    page?: number;
    pageSize?: number;
  }): Promise<PagedDiscrepancyListResponse> => {
    const res = await httpClient.get<PagedDiscrepancyListResponse>('/discrepancies', { params });
    return res.data;
  },

  getDiscrepancyById: async (id: string): Promise<DiscrepancyDetailResponse> => {
    const res = await httpClient.get<DiscrepancyDetailResponse>(`/discrepancies/${id}`);
    return res.data;
  },

  resolveDiscrepancy: async (id: string, payload: ResolveDiscrepancyRequest): Promise<DiscrepancyDetailResponse> => {
    const res = await httpClient.patch<DiscrepancyDetailResponse>(`/discrepancies/${id}/resolve`, payload);
    return res.data;
  },
};
