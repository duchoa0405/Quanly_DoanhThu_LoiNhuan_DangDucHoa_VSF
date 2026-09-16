import React from 'react';
import { Button } from '../../shared/ui/Button';
import { MoneyText } from '../../shared/ui/MoneyText';

export const AnalyticsDashboardPage: React.FC = () => {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Dashboard Doanh Thu & Báo Cáo Điều Hành (SCR-03)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Dữ liệu tài chính chỉ tính trên đơn DELIVERED - Kiểm soát chặt chẽ dòng tiền thực nhận.
          </p>
        </div>
        <Button variant="secondary">📥 Xuất Báo Cáo CSV</Button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '24px' }}>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>DOANH THU ĐÃ GIAO (GROSS)</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0' }}><MoneyText amount={184500000} /></div>
          <div style={{ fontSize: '11px', color: 'var(--color-positive)' }}>+14.2% so với tuần trước</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>TỔNG PHÍ SÀN CÁC KÊNH</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-danger)' }}><MoneyText amount={17500000} /></div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)' }}>Chiếm 9.5% tổng doanh số</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid #818cf8' }}>
          <div style={{ fontSize: '12px', color: '#4338ca' }}>DÒNG TIỀN THỰC NHẬN VỀ VÍ</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: '#4338ca' }}><MoneyText amount={167000000} /></div>
          <div style={{ fontSize: '11px', color: 'var(--color-positive)' }}>Biên lợi nhuận gộp 90.5%</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>ĐƠN GIAO THÀNH CÔNG</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0' }}>1,240 đơn</div>
          <div style={{ fontSize: '11px', color: 'var(--color-positive)' }}>Tỷ lệ giao đạt 99.4%</div>
        </div>
      </div>
    </div>
  );
};
