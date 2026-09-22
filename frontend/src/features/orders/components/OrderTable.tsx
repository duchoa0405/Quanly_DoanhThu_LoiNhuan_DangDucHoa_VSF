import React from 'react';
import { OrderListItemResponse, OrderStatus } from '../types/order.types';
import { MoneyText } from '../../../shared/ui/MoneyText';
import { Badge } from '../../../shared/ui/Badge';

interface OrderTableProps {
  orders: OrderListItemResponse[];
  loading?: boolean;
  onSelectOrder?: (order: OrderListItemResponse) => void;
  onUpdateStatus?: (orderId: string, toStatus: 'SHIPPED' | 'DELIVERED') => void;
  onCancelOrder?: (order: OrderListItemResponse) => void;
}

export const OrderTable: React.FC<OrderTableProps> = ({
  orders,
  loading,
  onSelectOrder,
  onUpdateStatus,
  onCancelOrder,
}) => {
  if (loading) {
    return (
      <div style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
        Đang tải dữ liệu đơn hàng...
      </div>
    );
  }

  if (orders.length === 0) {
    return (
      <div style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
        Không tìm thấy đơn hàng nào phù hợp với bộ lọc.
      </div>
    );
  }

  const getStatusColor = (status: OrderStatus) => {
    switch (status) {
      case 'DELIVERED':
        return 'var(--color-positive, #10b981)';
      case 'CANCELLED':
        return 'var(--color-danger, #ef4444)';
      case 'SHIPPED':
        return '#3b82f6';
      case 'PENDING':
      default:
        return 'var(--color-warning, #f59e0b)';
    }
  };

  return (
    <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
      <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
        <thead>
          <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
            <th style={{ padding: '12px 16px' }}>MÃ ĐƠN HÀNG</th>
            <th style={{ padding: '12px 16px' }}>KÊNH</th>
            <th style={{ padding: '12px 16px' }}>KHÁCH HÀNG</th>
            <th style={{ padding: '12px 16px' }}>SẢN PHẨM</th>
            <th style={{ padding: '12px 16px' }}>DOANH THU GỘP</th>
            <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
            <th style={{ padding: '12px 16px', textAlign: 'right' }}>THAO TÁC</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => {
            const statusColor = getStatusColor(order.status);
            const itemsText = order.itemsSummary && order.itemsSummary.length > 0
              ? order.itemsSummary.map(i => `${i.skuCode} (x${i.quantity})`).join(', ')
              : `${order.itemCount} sản phẩm`;

            return (
              <tr
                key={order.id}
                style={{
                  borderBottom: '1px solid var(--color-border-hairline)',
                  transition: 'background-color 0.15s ease',
                }}
              >
                <td
                  style={{ padding: '12px 16px', fontWeight: 600, cursor: onSelectOrder ? 'pointer' : 'default' }}
                  onClick={() => onSelectOrder?.(order)}
                >
                  {order.externalOrderId}
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <Badge label={order.channel} />
                </td>
                <td style={{ padding: '12px 16px' }}>{order.customerName || 'Khách vãng lai'}</td>
                <td style={{ padding: '12px 16px', color: 'var(--color-text-secondary)' }}>{itemsText}</td>
                <td style={{ padding: '12px 16px', fontWeight: 600 }}>
                  <MoneyText amount={order.grossRevenue} />
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{ color: statusColor, fontWeight: 600 }}>
                    ● {order.status}
                  </span>
                </td>
                <td style={{ padding: '12px 16px', textAlign: 'right' }}>
                  <div style={{ display: 'inline-flex', gap: '6px' }}>
                    {order.status === 'PENDING' && (
                      <>
                        <button
                          type="button"
                          onClick={() => onUpdateStatus?.(order.id, 'SHIPPED')}
                          style={{
                            padding: '4px 8px',
                            fontSize: '11px',
                            fontWeight: 600,
                            borderRadius: '4px',
                            border: '1px solid #3b82f6',
                            background: '#eff6ff',
                            color: '#1d4ed8',
                            cursor: 'pointer',
                          }}
                        >
                          Giao hàng
                        </button>
                        <button
                          type="button"
                          onClick={() => onCancelOrder?.(order)}
                          style={{
                            padding: '4px 8px',
                            fontSize: '11px',
                            fontWeight: 600,
                            borderRadius: '4px',
                            border: '1px solid #fca5a5',
                            background: '#fef2f2',
                            color: '#b91c1c',
                            cursor: 'pointer',
                          }}
                        >
                          Hủy
                        </button>
                      </>
                    )}
                    {order.status === 'SHIPPED' && (
                      <>
                        <button
                          type="button"
                          onClick={() => onUpdateStatus?.(order.id, 'DELIVERED')}
                          style={{
                            padding: '4px 8px',
                            fontSize: '11px',
                            fontWeight: 600,
                            borderRadius: '4px',
                            border: '1px solid #10b981',
                            background: '#ecfdf5',
                            color: '#047857',
                            cursor: 'pointer',
                          }}
                        >
                          Đã giao
                        </button>
                        <button
                          type="button"
                          onClick={() => onCancelOrder?.(order)}
                          style={{
                            padding: '4px 8px',
                            fontSize: '11px',
                            fontWeight: 600,
                            borderRadius: '4px',
                            border: '1px solid #fca5a5',
                            background: '#fef2f2',
                            color: '#b91c1c',
                            cursor: 'pointer',
                          }}
                        >
                          Hủy
                        </button>
                      </>
                    )}
                    {(order.status === 'DELIVERED' || order.status === 'CANCELLED') && (
                      <span style={{ fontSize: '12px', color: '#94a3b8' }}>
                        {order.status === 'DELIVERED' ? 'Đã hoàn tất' : 'Đã hủy'}
                      </span>
                    )}
                  </div>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
