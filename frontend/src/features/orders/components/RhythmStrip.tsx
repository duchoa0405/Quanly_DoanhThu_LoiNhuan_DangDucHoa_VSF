import React from 'react';
import { MoneyText } from '../../../shared/ui/MoneyText';

export const RhythmStrip: React.FC = () => {
  const metrics = [
    { label: 'Chờ xử lý', count: 12, value: 5400000 },
    { label: 'Đang vận chuyển', count: 45, value: 18200000 },
    { label: 'Đã giao (Doanh thu)', count: 1240, value: 184500000, highlight: true },
    { label: 'Đã hủy', count: 8, value: 3100000 },
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
            <MoneyText amount={m.value} />
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)', marginTop: '4px' }}>{m.count} đơn hàng</div>
        </div>
      ))}
    </div>
  );
};
