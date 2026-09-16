import React from 'react';
import { Button } from '../../shared/ui/Button';
import { MoneyText } from '../../shared/ui/MoneyText';

export const DiscrepanciesPage: React.FC = () => {
  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Hồ Sơ Kiểm Toán Sai Lệch Dòng Tiền</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Quản lý và giải trình các trường hợp sàn phạt cân nặng (#DIS-002) hoặc tính thừa phí.
          </p>
        </div>
        <Button variant="secondary">Xuất Biên Bản Giải Trình</Button>
      </div>

      <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', padding: '20px' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '13px' }}>
          <thead>
            <tr style={{ background: '#f8fafc', color: 'var(--color-text-secondary)' }}>
              <th style={{ padding: '10px' }}>MÃ HỒ SƠ</th>
              <th style={{ padding: '10px' }}>MÃ ĐƠN HÀNG</th>
              <th style={{ padding: '10px' }}>TIỀN DỰ KIẾN</th>
              <th style={{ padding: '10px' }}>TIỀN VÍ THỰC VỀ</th>
              <th style={{ padding: '10px' }}>CHÊNH LỆCH</th>
              <th style={{ padding: '10px' }}>LÝ DO SÀN PHẠT</th>
              <th style={{ padding: '10px' }}>TRẠNG THÁI</th>
            </tr>
          </thead>
          <tbody>
            <tr style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
              <td style={{ padding: '10px', fontWeight: 600 }}>#DIS-002</td>
              <td style={{ padding: '10px' }}>TTS-882103</td>
              <td style={{ padding: '10px' }}><MoneyText amount={416500} /></td>
              <td style={{ padding: '10px' }}><MoneyText amount={386500} /></td>
              <td style={{ padding: '10px', color: 'var(--color-danger)' }}><MoneyText amount={-30000} /></td>
              <td style={{ padding: '10px' }}>Phạt vượt cân nặng 400g (Lỗi kho)</td>
              <td style={{ padding: '10px' }}><span style={{ background: '#fef3c7', color: '#b45309', padding: '2px 8px', borderRadius: '4px', fontSize: '11px', fontWeight: 600 }}>Đang Khiếu Nại</span></td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  );
};
