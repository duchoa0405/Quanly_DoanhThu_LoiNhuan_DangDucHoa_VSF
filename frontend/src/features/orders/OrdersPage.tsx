import React, { useState, useEffect, useCallback } from 'react';
import { RhythmStrip } from './components/RhythmStrip';
import { OrderFilterPills } from './components/OrderFilterPills';
import { OrderTable } from './components/OrderTable';
import { CreateOrderModal } from './components/CreateOrderModal';
import { CancelOrderModal } from './components/CancelOrderModal';
import { Button } from '../../shared/ui/Button';
import { ordersApi } from './api/ordersApi';
import { OrderListItemResponse, OrderSummaryResponse, SalesChannel } from './types/order.types';

export const OrdersPage: React.FC = () => {
  const [selectedChannel, setSelectedChannel] = useState<string>('ALL');
  const [orders, setOrders] = useState<OrderListItemResponse[]>([]);
  const [summary, setSummary] = useState<OrderSummaryResponse | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  // Modals
  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [cancellingOrder, setCancellingOrder] = useState<OrderListItemResponse | null>(null);

  const fetchOrderData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const channelParam = selectedChannel === 'ALL' ? undefined : (selectedChannel as SalesChannel);
      const [pagedList, summaryData] = await Promise.all([
        ordersApi.getOrders({ channel: channelParam }),
        ordersApi.getOrderSummary({ channel: channelParam }),
      ]);
      setOrders(pagedList.items || []);
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

  const handleUpdateStatus = async (orderId: string, toStatus: 'SHIPPED' | 'DELIVERED') => {
    try {
      await ordersApi.updateOrderStatus(orderId, { toStatus });
      await fetchOrderData();
    } catch (err: unknown) {
      console.error('Failed to update order status:', err);
      alert('Cập nhật trạng thái đơn hàng thất bại.');
    }
  };

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
          <Button variant="primary" onClick={() => setIsCreateOpen(true)}>
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
        onUpdateStatus={handleUpdateStatus}
        onCancelOrder={(order) => setCancellingOrder(order)}
      />

      {/* Modals */}
      <CreateOrderModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={fetchOrderData}
      />

      <CancelOrderModal
        isOpen={cancellingOrder !== null}
        order={cancellingOrder}
        onClose={() => setCancellingOrder(null)}
        onSuccess={fetchOrderData}
      />
    </div>
  );
};
