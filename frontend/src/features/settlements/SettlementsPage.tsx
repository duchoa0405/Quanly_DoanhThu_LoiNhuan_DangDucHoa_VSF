import React from 'react';
import { Button } from '../../shared/ui/Button';
import { MoneyText } from '../../shared/ui/MoneyText';

export const SettlementsPage: React.FC = () => {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Phí Sàn & Đối Soát Ví (SCR-02)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            So khớp 2 chiều giữa đơn hàng và file sao kê Excel tài khoản ngân hàng / ví sàn.
          </p>
        </div>
        <Button variant="primary">📥 Nạp Sao Kê Ví (Excel/CSV)</Button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '16px', marginBottom: '24px' }}>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>Chờ đối soát</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0' }}><MoneyText amount={12400000} /></div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>42 đơn hàng</div>
        </div>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid #86efac' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-positive)' }}>Đã khớp 100%</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-positive)' }}><MoneyText amount={184500000} /></div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>1,238 đơn hàng</div>
        </div>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid #fca5a5' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-danger)' }}>Phát hiện sai lệch</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-danger)' }}><MoneyText amount={68000} /></div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>2 đơn hàng (Cần lập hồ sơ)</div>
        </div>
      </div>
    </div>
  );
};
