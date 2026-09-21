import React from 'react';
import { OrderResponse } from '../types/order.types';
import { MoneyText } from '../../../shared/ui/MoneyText';
import { Badge } from '../../../shared/ui/Badge';

interface OrderTableProps {
  orders: OrderResponse[];
  loading?: boolean;
  onSelectOrder?: (order: OrderResponse) => void;
}

export const OrderTable: React.FC<OrderTableProps> = ({ orders, loading, onSelectOrder }) => {
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

  const getStatusColor = (status: string) => {
    switch (status.toLowerCase()) {
      case 'delivered':
        return 'var(--color-positive, #10b981)';
      case 'cancelled':
      case 'returned':
        return 'var(--color-danger, #ef4444)';
      case 'intransit':
      case 'shipped':
        return '#3b82f6';
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
            <th style={{ padding: '12px 16px' }}>DOANH THU GỘP</th>
            <th style={{ padding: '12px 16px' }}>PHÍ SÀN DỰ KIẾN</th>
            <th style={{ padding: '12px 16px' }}>DỰ KIẾN THỰC NHẬN</th>
            <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((order) => {
            const platformFee = order.feeSnapshot?.totalPlatformFees ?? 0;
            const netPayout = order.feeSnapshot?.projectedSettlement ?? (order.grossRevenue - platformFee);
            const statusColor = getStatusColor(order.status);

            return (
              <tr
                key={order.id}
                onClick={() => onSelectOrder?.(order)}
                style={{
                  borderBottom: '1px solid var(--color-border-hairline)',
                  cursor: onSelectOrder ? 'pointer' : 'default',
                  transition: 'background-color 0.15s ease',
                }}
              >
                <td style={{ padding: '12px 16px', fontWeight: 600 }}>{order.externalOrderId}</td>
                <td style={{ padding: '12px 16px' }}>
                  <Badge label={order.channel} />
                </td>
                <td style={{ padding: '12px 16px' }}>{order.customerName || 'Khách vãng lai'}</td>
                <td style={{ padding: '12px 16px' }}>
                  <MoneyText amount={order.grossRevenue} />
                </td>
                <td style={{ padding: '12px 16px', color: 'var(--color-danger)' }}>
                  <MoneyText amount={platformFee} />
                </td>
                <td style={{ padding: '12px 16px', color: 'var(--color-positive)', fontWeight: 600 }}>
                  <MoneyText amount={netPayout} />
                </td>
                <td style={{ padding: '12px 16px' }}>
                  <span style={{ color: statusColor, fontWeight: 600 }}>
                    ● {order.status}
                  </span>
                </td>
              </tr>
            );
          })}
        </tbody>
      </table>
    </div>
  );
};
