import React, { useEffect, useState } from 'react';
import { analyticsApi } from '../api/analyticsApi';
import { DrilldownOrderItem, PagedDrilldownOrderResponse } from '../types/analytics.types';
import { SalesChannel } from '../../orders/types/order.types';
import { MoneyText } from '../../../shared/ui/MoneyText';
import { Badge } from '../../../shared/ui/Badge';
import { Button } from '../../../shared/ui/Button';

interface DrilldownOrdersModalProps {
  isOpen: boolean;
  channel?: SalesChannel;
  from?: string;
  to?: string;
  onClose: () => void;
}

export const DrilldownOrdersModal: React.FC<DrilldownOrdersModalProps> = ({
  isOpen,
  channel,
  from,
  to,
  onClose,
}) => {
  const [data, setData] = useState<PagedDrilldownOrderResponse | null>(null);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      setLoading(true);
      setError(null);
      analyticsApi
        .getDrilldown({
          channel,
          from,
          to,
          page,
          pageSize: 15,
        })
        .then((res) => setData(res))
        .catch((err) => {
          console.error('Failed to load drilldown orders:', err);
          setError('Không thể tải danh sách đơn hàng chi tiết.');
        })
        .finally(() => setLoading(false));
    }
  }, [isOpen, channel, from, to, page]);

  if (!isOpen) return null;

  return (
    <div
      style={{
        position: 'fixed',
        inset: 0,
        backgroundColor: 'rgba(0, 0, 0, 0.5)',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        zIndex: 1000,
        padding: '20px',
      }}
    >
      <div
        style={{
          background: '#ffffff',
          borderRadius: '12px',
          width: '100%',
          maxWidth: '960px',
          maxHeight: '90vh',
          display: 'flex',
          flexDirection: 'column',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
        }}
      >
        <div
          style={{
            padding: '16px 24px',
            borderBottom: '1px solid #e2e8f0',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <div>
            <h2 style={{ fontSize: '17px', fontWeight: 700, margin: 0 }}>
              Chi Tiết Đơn Hàng Ghi Nhận Tài Chính (Drilldown)
            </h2>
            <p style={{ fontSize: '12px', color: '#64748b', margin: '4px 0 0 0' }}>
              Danh sách các đơn hàng DELIVERED tạo nên doanh thu, chi phí phí sàn và lợi nhuận đóng góp.
              {channel ? ` Kênh: ${channel}` : ' Tất cả kênh'}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: 'none', fontSize: '20px', cursor: 'pointer', color: '#94a3b8' }}
          >
            ✕
          </button>
        </div>

        <div style={{ padding: '16px 24px', flex: 1, overflowY: 'auto' }}>
          {error && (
            <div style={{ padding: '10px 14px', marginBottom: '14px', borderRadius: '6px', background: '#fef2f2', color: '#b91c1c', fontSize: '13px' }}>
              {error}
            </div>
          )}

          {loading ? (
            <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>Đang tải danh sách đơn...</div>
          ) : !data || data.items.length === 0 ? (
            <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>Không có đơn hàng nào phù hợp với điều kiện.</div>
          ) : (
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '12px' }}>
              <thead>
                <tr style={{ background: '#f8fafc', borderBottom: '1px solid #e2e8f0', color: '#64748b' }}>
                  <th style={{ padding: '10px 12px' }}>MÃ ĐƠN</th>
                  <th style={{ padding: '10px 12px' }}>KÊNH</th>
                  <th style={{ padding: '10px 12px' }}>NGÀY GIAO</th>
                  <th style={{ padding: '10px 12px' }}>DOANH THU</th>
                  <th style={{ padding: '10px 12px' }}>PHÍ SÀN</th>
                  <th style={{ padding: '10px 12px' }}>DỰ KIẾN VÍ</th>
                  <th style={{ padding: '10px 12px' }}>GIÁ VỐN (COGS)</th>
                  <th style={{ padding: '10px 12px' }}>LỢI NHUẬN</th>
                </tr>
              </thead>
              <tbody>
                {data.items.map((item: DrilldownOrderItem) => (
                  <tr key={item.id} style={{ borderBottom: '1px solid #f1f5f9' }}>
                    <td style={{ padding: '10px 12px', fontWeight: 600 }}>{item.externalOrderId}</td>
                    <td style={{ padding: '10px 12px' }}>
                      <Badge label={item.channel} />
                    </td>
                    <td style={{ padding: '10px 12px', color: '#64748b' }}>
                      {new Date(item.deliveredAt).toLocaleDateString('vi-VN')}
                    </td>
                    <td style={{ padding: '10px 12px', fontWeight: 600 }}>
                      <MoneyText amount={item.grossRevenue} />
                    </td>
                    <td style={{ padding: '10px 12px', color: '#ef4444' }}>
                      <MoneyText amount={item.totalPlatformFees} />
                    </td>
                    <td style={{ padding: '10px 12px', color: '#10b981', fontWeight: 600 }}>
                      <MoneyText amount={item.projectedSettlement} />
                    </td>
                    <td style={{ padding: '10px 12px', color: '#64748b' }}>
                      <MoneyText amount={item.cogs} />
                    </td>
                    <td style={{ padding: '10px 12px', color: item.contributionProfit >= 0 ? '#10b981' : '#ef4444', fontWeight: 700 }}>
                      <MoneyText amount={item.contributionProfit} />
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          )}
        </div>

        {data && data.totalPages > 1 && (
          <div style={{ padding: '12px 24px', borderTop: '1px solid #e2e8f0', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
            <span style={{ fontSize: '12px', color: '#64748b' }}>
              Trang {data.page} / {data.totalPages} ({data.totalItems} đơn)
            </span>
            <div style={{ display: 'flex', gap: '8px' }}>
              <Button variant="secondary" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page <= 1}>
                Trước
              </Button>
              <Button variant="secondary" onClick={() => setPage((p) => Math.min(data.totalPages, p + 1))} disabled={page >= data.totalPages}>
                Tiếp
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
