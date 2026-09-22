import React, { useEffect, useState, useCallback } from 'react';
import { analyticsApi } from './api/analyticsApi';
import { ChannelBreakdownItem, FinancialKpiResponse, TopSkuItem } from './types/analytics.types';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const AnalyticsDashboardPage: React.FC = () => {
  const [kpis, setKpis] = useState<FinancialKpiResponse | null>(null);
  const [channels, setChannels] = useState<ChannelBreakdownItem[]>([]);
  const [topSkus, setTopSkus] = useState<TopSkuItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadAnalytics = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [kpiData, channelData, skuData] = await Promise.all([
        analyticsApi.getKpis(),
        analyticsApi.getChannels().catch(() => []),
        analyticsApi.getTopSkus({ limit: 10 }).catch(() => []),
      ]);
      setKpis(kpiData);
      setChannels(channelData || []);
      setTopSkus(skuData || []);
    } catch (err: unknown) {
      console.error('Failed to load analytics:', err);
      setError('Không thể kết nối đến máy chủ Analytics.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadAnalytics();
  }, [loadAnalytics]);

  const handleExportCsv = async () => {
    try {
      const blob = await analyticsApi.exportCsv();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `financial_report_${new Date().toISOString().slice(0, 10)}.csv`;
      document.body.appendChild(a);
      a.click();
      a.remove();
    } catch (err) {
      console.error('Failed to export CSV:', err);
      alert('Xuất báo cáo CSV thất bại.');
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Dashboard Doanh Thu & Báo Cáo Điều Hành (SCR-03)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Doanh thu và lợi nhuận tính theo chuẩn DeliveredAt (Giao hàng thành công) - Không dùng OrderDate.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="secondary" onClick={handleExportCsv}>
            📥 Xuất Báo Cáo CSV
          </Button>
          <Button variant="primary" onClick={loadAnalytics}>
            Làm mới
          </Button>
        </div>
      </div>

      {error && (
        <div style={{ padding: '12px 16px', marginBottom: '16px', borderRadius: '6px', background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', fontSize: '13px' }}>
          {error}
        </div>
      )}

      {/* KPI Summary Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(4, 1fr)', gap: '16px', marginBottom: '24px' }}>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>DOANH THU ĐÃ GIAO (GROSS)</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0' }}>
            <MoneyText amount={kpis?.grossRevenue ?? 0} />
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)' }}>{kpis?.deliveredOrderCount ?? 0} đơn giao thành công</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>TỔNG PHÍ SÀN (FEES)</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-danger)' }}>
            <MoneyText amount={kpis?.totalPlatformFees ?? 0} />
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)' }}>Hoa hồng + Phí TT + Dịch vụ</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>GIÁ VỐN HÀNG BÁN (COGS)</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-text-primary)' }}>
            <MoneyText amount={kpis?.cogs ?? 0} />
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-text-muted)' }}>Chi phí giá vốn theo SKU</div>
        </div>
        <div style={{ background: '#ffffff', padding: '18px', borderRadius: '8px', border: '1px solid #818cf8' }}>
          <div style={{ fontSize: '12px', color: '#4338ca' }}>LỢI NHUẬN ĐÓNG GÓP (PROFIT)</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: '#4338ca' }}>
            <MoneyText amount={kpis?.contributionProfit ?? 0} />
          </div>
          <div style={{ fontSize: '11px', color: 'var(--color-positive)', fontWeight: 600 }}>
            Biên lợi nhuận: {kpis?.contributionMarginPct ?? 0}%
          </div>
        </div>
      </div>

      {/* Top SKUs Table */}
      <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden', marginBottom: '24px' }}>
        <div style={{ padding: '16px', borderBottom: '1px solid var(--color-border-hairline)', fontWeight: 700, fontSize: '15px' }}>
          Top SKU Theo Lợi Nhuận Đóng Góp (Đã phân bổ Voucher & Phí sàn)
        </div>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
          <thead>
            <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
              <th style={{ padding: '12px 16px' }}>MÃ SKU</th>
              <th style={{ padding: '12px 16px' }}>TÊN SẢN PHẨM</th>
              <th style={{ padding: '12px 16px' }}>SỐ LƯỢNG GIAO</th>
              <th style={{ padding: '12px 16px' }}>DOANH THU THỰC</th>
              <th style={{ padding: '12px 16px' }}>GIÁ VỐN (COGS)</th>
              <th style={{ padding: '12px 16px' }}>LỢI NHUẬN ĐÓNG GÓP</th>
              <th style={{ padding: '12px 16px' }}>BIÊN LỢI NHUẬN</th>
            </tr>
          </thead>
          <tbody>
            {topSkus.length === 0 ? (
              <tr>
                <td colSpan={7} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                  {loading ? 'Đang tải dữ liệu SKU...' : 'Chưa có dữ liệu SKU giao thành công.'}
                </td>
              </tr>
            ) : (
              topSkus.map((sku) => (
                <tr key={sku.skuCode} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                  <td style={{ padding: '12px 16px', fontWeight: 600 }}>{sku.skuCode}</td>
                  <td style={{ padding: '12px 16px' }}>{sku.productName}</td>
                  <td style={{ padding: '12px 16px' }}>{sku.deliveredUnits}</td>
                  <td style={{ padding: '12px 16px' }}>
                    <MoneyText amount={sku.grossRevenue} />
                  </td>
                  <td style={{ padding: '12px 16px', color: 'var(--color-text-secondary)' }}>
                    <MoneyText amount={sku.cogs} />
                  </td>
                  <td style={{ padding: '12px 16px', color: sku.contributionProfit >= 0 ? 'var(--color-positive)' : 'var(--color-danger)', fontWeight: 600 }}>
                    <MoneyText amount={sku.contributionProfit} />
                  </td>
                  <td style={{ padding: '12px 16px', fontWeight: 600 }}>
                    {sku.contributionMarginPct}%
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Channel Breakdown */}
      <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
        <div style={{ padding: '16px', borderBottom: '1px solid var(--color-border-hairline)', fontWeight: 700, fontSize: '15px' }}>
          Phân Tích Hiệu Quả Kênh Bán Hàng
        </div>
        <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
          <thead>
            <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
              <th style={{ padding: '12px 16px' }}>KÊNH BÁN HÀNG</th>
              <th style={{ padding: '12px 16px' }}>ĐƠN THÀNH CÔNG</th>
              <th style={{ padding: '12px 16px' }}>DOANH THU GỘP</th>
              <th style={{ padding: '12px 16px' }}>TỔNG PHÍ SÀN</th>
              <th style={{ padding: '12px 16px' }}>LỢI NHUẬN ĐÓNG GÓP</th>
              <th style={{ padding: '12px 16px' }}>BIÊN LỢI NHUẬN</th>
            </tr>
          </thead>
          <tbody>
            {channels.length === 0 ? (
              <tr>
                <td colSpan={6} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                  {loading ? 'Đang tải dữ liệu kênh...' : 'Chưa có dữ liệu kênh bán hàng.'}
                </td>
              </tr>
            ) : (
              channels.map((ch) => (
                <tr key={ch.channel} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                  <td style={{ padding: '12px 16px' }}>
                    <Badge label={ch.channel} />
                  </td>
                  <td style={{ padding: '12px 16px' }}>{ch.deliveredOrders}</td>
                  <td style={{ padding: '12px 16px' }}>
                    <MoneyText amount={ch.grossRevenue} />
                  </td>
                  <td style={{ padding: '12px 16px', color: 'var(--color-danger)' }}>
                    <MoneyText amount={ch.totalPlatformFees} />
                  </td>
                  <td style={{ padding: '12px 16px', color: ch.contributionProfit >= 0 ? 'var(--color-positive)' : 'var(--color-danger)', fontWeight: 600 }}>
                    <MoneyText amount={ch.contributionProfit} />
                  </td>
                  <td style={{ padding: '12px 16px', fontWeight: 600 }}>
                    {ch.contributionMarginPct}%
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>
    </div>
  );
};
