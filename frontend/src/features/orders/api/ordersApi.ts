import { httpClient } from '../../../shared/api/httpClient';
import {
  OrderResponse,
  OrderSummaryResponse,
  FeePreviewPayload,
  FeePreviewResponse,
  CreateOrderPayload,
  OrderFilterParams,
} from '../types/order.types';

export const ordersApi = {
  getOrders: async (params?: OrderFilterParams): Promise<OrderResponse[]> => {
    const response = await httpClient.get<OrderResponse[]>('/orders', { params });
    return response.data;
  },

  getOrderById: async (id: string): Promise<OrderResponse> => {
    const response = await httpClient.get<OrderResponse>(`/orders/${id}`);
    return response.data;
  },

  createOrder: async (payload: CreateOrderPayload): Promise<OrderResponse> => {
    const response = await httpClient.post<OrderResponse>('/orders', payload);
    return response.data;
  },

  updateOrderStatus: async (id: string, payload: { toStatus: string; reason?: string }): Promise<void> => {
    await httpClient.patch(`/orders/${id}/status`, payload);
  },

  getOrderSummary: async (params?: { fromDate?: string; toDate?: string; channel?: string }): Promise<OrderSummaryResponse> => {
    const response = await httpClient.get<OrderSummaryResponse>('/orders/summary', { params });
    return response.data;
  },

  previewFee: async (payload: FeePreviewPayload): Promise<FeePreviewResponse> => {
    const response = await httpClient.post<FeePreviewResponse>('/orders/fee-preview', payload);
    return response.data;
  },
};
