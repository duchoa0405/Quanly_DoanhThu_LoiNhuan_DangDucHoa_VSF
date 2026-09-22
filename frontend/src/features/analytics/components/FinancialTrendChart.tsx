import React from 'react';
import { FinancialTrendPoint } from '../types/analytics.types';

interface FinancialTrendChartProps {
  points: FinancialTrendPoint[];
  loading?: boolean;
}

export const FinancialTrendChart: React.FC<FinancialTrendChartProps> = ({ points, loading }) => {
  if (loading) {
    return (
      <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>
        Đang tải xu hướng tài chính...
      </div>
    );
  }

  if (points.length === 0) {
    return (
      <div style={{ padding: '32px', textAlign: 'center', color: '#64748b' }}>
        Chưa có dữ liệu xu hướng trong khoảng thời gian này.
      </div>
    );
  }

  const maxVal = Math.max(...points.map((p) => Math.max(p.grossRevenue, p.contributionProfit, 1)));

  return (
    <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', padding: '20px', marginBottom: '24px' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '16px' }}>
        <h3 style={{ fontSize: '15px', fontWeight: 700, margin: 0 }}>
          Biểu Đồ Xu Hướng Doanh Thu & Lợi Nhuận Đóng Góp (Theo DeliveredAt)
        </h3>
        <div style={{ display: 'flex', gap: '16px', fontSize: '12px' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <span style={{ width: '12px', height: '12px', borderRadius: '2px', background: '#3b82f6', display: 'inline-block' }} />
            <span>Doanh thu gộp</span>
          </div>
          <div style={{ display: 'flex', alignItems: 'center', gap: '6px' }}>
            <span style={{ width: '12px', height: '12px', borderRadius: '2px', background: '#10b981', display: 'inline-block' }} />
            <span>Lợi nhuận đóng góp</span>
          </div>
        </div>
      </div>

      <div style={{ display: 'flex', alignItems: 'flex-end', gap: '12px', height: '180px', paddingTop: '20px' }}>
        {points.map((p) => {
          const revHeight = Math.round((p.grossRevenue / maxVal) * 140);
          const profHeight = Math.max(0, Math.round((p.contributionProfit / maxVal) * 140));

          return (
            <div key={p.date} style={{ flex: 1, display: 'flex', flexDirection: 'column', alignItems: 'center', height: '100%', justifyContent: 'flex-end' }}>
              <div style={{ display: 'flex', gap: '4px', alignItems: 'flex-end', width: '100%', justifyContent: 'center' }}>
                {/* Revenue Bar */}
                <div
                  title={`Doanh thu: ${p.grossRevenue.toLocaleString()} ₫`}
                  style={{
                    width: '45%',
                    maxWidth: '24px',
                    height: `${revHeight}px`,
                    background: '#3b82f6',
                    borderRadius: '4px 4px 0 0',
                    transition: 'height 0.3s ease',
                  }}
                />
                {/* Profit Bar */}
                <div
                  title={`Lợi nhuận: ${p.contributionProfit.toLocaleString()} ₫`}
                  style={{
                    width: '45%',
                    maxWidth: '24px',
                    height: `${profHeight}px`,
                    background: '#10b981',
                    borderRadius: '4px 4px 0 0',
                    transition: 'height 0.3s ease',
                  }}
                />
              </div>
              <span style={{ fontSize: '10px', color: '#64748b', marginTop: '8px', transform: 'rotate(-25deg)', whiteSpace: 'nowrap' }}>
                {p.date.slice(5)}
              </span>
            </div>
          );
        })}
      </div>
    </div>
  );
};
