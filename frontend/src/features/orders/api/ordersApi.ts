import { httpClient } from '../../../shared/api/httpClient';
import {
  OrderDetailResponse,
  OrderSummaryResponse,
  FeePreviewPayload,
  FeeBreakdownResponse,
  CreateOrderPayload,
  OrderFilterParams,
  PagedOrderListResponse,
  OrderProgressStatus,
  SalesChannel,
} from '../types/order.types';

export const ordersApi = {
  getOrders: async (params?: OrderFilterParams): Promise<PagedOrderListResponse> => {
    const response = await httpClient.get<PagedOrderListResponse>('/orders', { params });
    return response.data;
  },

  getOrderById: async (id: string): Promise<OrderDetailResponse> => {
    const response = await httpClient.get<OrderDetailResponse>(`/orders/${id}`);
    return response.data;
  },

  createOrder: async (payload: CreateOrderPayload): Promise<OrderDetailResponse> => {
    const response = await httpClient.post<OrderDetailResponse>('/orders', payload);
    return response.data;
  },

  updateOrderStatus: async (id: string, payload: { toStatus: OrderProgressStatus }): Promise<void> => {
    await httpClient.patch(`/orders/${id}/status`, payload);
  },

  cancelOrder: async (id: string, payload: { cancellationReason: string }): Promise<void> => {
    await httpClient.post(`/orders/${id}/cancel`, payload);
  },

  getOrderSummary: async (params?: { from?: string; to?: string; channel?: SalesChannel }): Promise<OrderSummaryResponse> => {
    const response = await httpClient.get<OrderSummaryResponse>('/orders/summary', { params });
    return response.data;
  },

  previewFee: async (payload: FeePreviewPayload): Promise<FeeBreakdownResponse> => {
    const response = await httpClient.post<FeeBreakdownResponse>('/orders/preview-fee', payload);
    return response.data;
  },
};
