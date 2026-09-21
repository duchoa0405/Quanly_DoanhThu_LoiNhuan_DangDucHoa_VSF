import React from 'react';
import { MoneyText } from '../../../shared/ui/MoneyText';
import { OrderSummaryResponse } from '../types/order.types';

interface RhythmStripProps {
  summary?: OrderSummaryResponse | null;
}

export const RhythmStrip: React.FC<RhythmStripProps> = ({ summary }) => {
  const metrics = summary
    ? [
        { label: 'Tổng số đơn', count: summary.totalOrders, value: summary.grossRevenue },
        { label: 'Đang vận chuyển', count: summary.inTransitOrders, value: 0 },
        { label: 'Đã giao (Doanh thu ghi nhận)', count: summary.deliveredOrders, value: summary.grossRevenue, highlight: true },
        { label: 'Đã hủy', count: summary.cancelledOrders, value: 0 },
      ]
    : [
        { label: 'Tổng đơn hàng', count: 0, value: 0 },
        { label: 'Đang vận chuyển', count: 0, value: 0 },
        { label: 'Đã giao (Doanh thu ghi nhận)', count: 0, value: 0, highlight: true },
        { label: 'Đã hủy', count: 0, value: 0 },
      ];

  return (
    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '20px' }}>
      {metrics.map((m, i) => (
        <div
          key={i}
          style={{
            background: '#ffffff',
            padding: '16px',
            borderRadius: '8px',
            border: m.highlight ? '1px solid #818cf8' : '1px solid var(--color-border-hairline)',
          }}
        >
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)', marginBottom: '6px' }}>{m.label}</div>
          <div style={{ fontSize: '20px', fontWeight: 700 }}>
            {m.value > 0 ? <MoneyText amount={m.value} /> : `${m.count} đơn`}
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)', marginTop: '4px' }}>
            {m.count} đơn hàng
          </div>
        </div>
      ))}
    </div>
  );
};
