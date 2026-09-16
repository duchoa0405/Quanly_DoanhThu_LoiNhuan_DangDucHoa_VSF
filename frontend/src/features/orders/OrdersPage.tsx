import React, { useState } from 'react';
import { RhythmStrip } from './components/RhythmStrip';
import { OrderFilterPills } from './components/OrderFilterPills';
import { Button } from '../../shared/ui/Button';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';

export const OrdersPage: React.FC = () => {
  const [selectedChannel, setSelectedChannel] = useState('ALL');

  const sampleOrders = [
    { id: '1', code: 'TTS-882103', channel: 'TikTok', customer: 'Nguyễn Thị Mai', amount: 450000, fee: 33500, net: 416500, status: 'Delivered' },
    { id: '2', code: 'SHP-992014', channel: 'Shopee', customer: 'Trần Văn Hưng', amount: 620000, fee: 52700, net: 567300, status: 'Delivered' },
    { id: '3', code: 'POS-100293', channel: 'POS', customer: 'Lê Thanh Thảo', amount: 1250000, fee: 12500, net: 1237500, status: 'Delivered' },
  ];

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Quản Lý Đơn Hàng Đa Kênh (SCR-01)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Theo dõi tiến trình đơn, bóc tách phí sàn real-time & kiểm soát doanh thu ghi nhận.
          </p>
        </div>
        <Button variant="primary">+ Tạo Đơn Mới (Live Fee Preview)</Button>
      </div>

      <RhythmStrip />
      <OrderFilterPills selectedChannel={selectedChannel} onSelectChannel={setSelectedChannel} />

      <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
          <thead>
            <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
              <th style={{ padding: '12px 16px' }}>MÃ ĐƠN HÀNG</th>
              <th style={{ padding: '12px 16px' }}>KÊNH</th>
              <th style={{ padding: '12px 16px' }}>KHÁCH HÀNG</th>
              <th style={{ padding: '12px 16px' }}>DOANH THU GỘP</th>
              <th style={{ padding: '12px 16px' }}>PHÍ SÀN DỰ KIẾN</th>
              <th style={{ padding: '12px 16px' }}>THỰC NHẬN</th>
              <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
            </tr>
          </thead>
          <tbody>
            {sampleOrders.map((o) => (
              <tr key={o.id} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                <td style={{ padding: '12px 16px', fontWeight: 600 }}>{o.code}</td>
                <td style={{ padding: '12px 16px' }}><Badge label={o.channel} /></td>
                <td style={{ padding: '12px 16px' }}>{o.customer}</td>
                <td style={{ padding: '12px 16px' }}><MoneyText amount={o.amount} /></td>
                <td style={{ padding: '12px 16px', color: 'var(--color-danger)' }}><MoneyText amount={o.fee} /></td>
                <td style={{ padding: '12px 16px', color: 'var(--color-positive)' }}><MoneyText amount={o.net} /></td>
                <td style={{ padding: '12px 16px' }}><span style={{ color: 'var(--color-positive)', fontWeight: 600 }}>✓ {o.status}</span></td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>
    </div>
  );
};
