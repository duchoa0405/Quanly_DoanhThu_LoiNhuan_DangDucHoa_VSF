import { useState, useCallback } from 'react';
import { ordersApi } from '../api/ordersApi';
import {
  OrderListItemResponse,
  OrderSummaryResponse,
  OrderFilterParams,
  CreateOrderPayload,
  OrderProgressStatus,
  FeePreviewPayload,
  FeeBreakdownResponse,
  OrderDetailResponse,
} from '../types/order.types';

export const useOrders = () => {
  const [orders, setOrders] = useState<OrderListItemResponse[]>([]);
  const [summary, setSummary] = useState<OrderSummaryResponse | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchOrders = useCallback(async (params?: OrderFilterParams) => {
    setLoading(true);
    setError(null);
    try {
      const [paged, sum] = await Promise.all([
        ordersApi.getOrders(params),
        ordersApi.getOrderSummary({
          channel: params?.channel,
          from: params?.from,
          to: params?.to,
        }).catch(() => null),
      ]);
      setOrders(paged.items || []);
      setSummary(sum);
    } catch (err: unknown) {
      console.error('Failed to load orders:', err);
      setError('Không thể tải danh sách đơn hàng từ máy chủ.');
    } finally {
      setLoading(false);
    }
  }, []);

  const createOrder = useCallback(async (payload: CreateOrderPayload): Promise<OrderDetailResponse> => {
    const created = await ordersApi.createOrder(payload);
    await fetchOrders();
    return created;
  }, [fetchOrders]);

  const updateStatus = useCallback(async (id: string, toStatus: OrderProgressStatus) => {
    await ordersApi.updateOrderStatus(id, { toStatus });
    await fetchOrders();
  }, [fetchOrders]);

  const cancelOrder = useCallback(async (id: string, cancellationReason: string) => {
    await ordersApi.cancelOrder(id, { cancellationReason });
    await fetchOrders();
  }, [fetchOrders]);

  const previewFee = useCallback(async (payload: FeePreviewPayload): Promise<FeeBreakdownResponse> => {
    return await ordersApi.previewFee(payload);
  }, []);

  return {
    orders,
    summary,
    loading,
    error,
    fetchOrders,
    createOrder,
    updateStatus,
    cancelOrder,
    previewFee,
  };
};
