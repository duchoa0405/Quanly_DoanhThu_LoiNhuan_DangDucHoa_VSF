import { httpClient } from '../../../shared/api/httpClient';

export interface FeePreviewPayload {
  channelCode: string;
  subtotal: number;
  shopVoucher: number;
}

export const ordersApi = {
  previewFee: async (payload: FeePreviewPayload) => {
    const res = await httpClient.post('/orders/preview-fee', payload);
    return res.data;
  },
  listOrders: async (params?: Record<string, any>) => {
    const res = await httpClient.get('/orders', { params });
    return res.data;
  },
};
