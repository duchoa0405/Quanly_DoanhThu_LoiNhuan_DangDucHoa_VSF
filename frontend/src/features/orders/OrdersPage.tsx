import React, { useState, useEffect, useCallback } from 'react';
import { RhythmStrip } from './components/RhythmStrip';
import { OrderFilterPills } from './components/OrderFilterPills';
import { OrderTable } from './components/OrderTable';
import { Button } from '../../shared/ui/Button';
import { ordersApi } from './api/ordersApi';
import { OrderResponse, OrderSummaryResponse } from './types/order.types';

export const OrdersPage: React.FC = () => {
  const [selectedChannel, setSelectedChannel] = useState<string>('ALL');
  const [orders, setOrders] = useState<OrderResponse[]>([]);
  const [summary, setSummary] = useState<OrderSummaryResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchOrderData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const channelParam = selectedChannel === 'ALL' ? undefined : selectedChannel;
      const [orderList, summaryData] = await Promise.all([
        ordersApi.getOrders({ channel: channelParam }),
        ordersApi.getOrderSummary({ channel: channelParam }),
      ]);
      setOrders(orderList || []);
      setSummary(summaryData || null);
    } catch (err: unknown) {
      console.error('Failed to load orders:', err);
      setError('Không thể kết nối đến máy chủ API. Vui lòng kiểm tra backend và thử lại.');
    } finally {
      setLoading(false);
    }
  }, [selectedChannel]);

  useEffect(() => {
    fetchOrderData();
  }, [fetchOrderData]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Quản Lý Đơn Hàng Đa Kênh (SCR-01)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Theo dõi tiến trình đơn, bóc tách phí sàn real-time & kiểm soát doanh thu ghi nhận.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="secondary" onClick={fetchOrderData}>
            Làm mới
          </Button>
          <Button variant="primary">
            + Tạo Đơn Mới (Live Fee Preview)
          </Button>
        </div>
      </div>

      <RhythmStrip summary={summary} />

      <OrderFilterPills selectedChannel={selectedChannel} onSelectChannel={setSelectedChannel} />

      {error && (
        <div
          style={{
            padding: '12px 16px',
            marginBottom: '16px',
            borderRadius: '6px',
            background: '#fef2f2',
            border: '1px solid #fecaca',
            color: '#b91c1c',
            fontSize: '13px',
          }}
        >
          {error}
        </div>
      )}

      <OrderTable
        orders={orders}
        loading={loading}
      />
    </div>
  );
};
