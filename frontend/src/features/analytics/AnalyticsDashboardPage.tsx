import React, { useEffect, useState, useCallback } from 'react';
import { useAnalytics } from './hooks/useAnalytics';
import { SalesChannel } from '../orders/types/order.types';
import { FinancialTrendChart } from './components/FinancialTrendChart';
import { DrilldownOrdersModal } from './components/DrilldownOrdersModal';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const AnalyticsDashboardPage: React.FC = () => {
  const { kpis, channels, topSkus, trend, loading, error, fetchDashboard, exportCsv } = useAnalytics();

  // Filters
  const [channel, setChannel] = useState<string>('ALL');
  const [fromDate, setFromDate] = useState<string>('');
  const [toDate, setToDate] = useState<string>('');

  // Drilldown modal state
  const [isDrilldownOpen, setIsDrilldownOpen] = useState(false);
  const [drilldownChannel, setDrilldownChannel] = useState<SalesChannel | undefined>(undefined);

  const handleApplyFilter = useCallback(() => {
    const channelParam = channel === 'ALL' ? undefined : (channel as SalesChannel);
    const fromParam = fromDate || undefined;
    const toParam = toDate || undefined;
    fetchDashboard({
      channel: channelParam,
      from: fromParam,
      to: toParam,
    });
  }, [channel, fromDate, toDate, fetchDashboard]);

  useEffect(() => {
    handleApplyFilter();
  }, [handleApplyFilter]);

  const handleExportCsv = async () => {
    try {
      const fromParam = fromDate || undefined;
      const toParam = toDate || undefined;
      const blob = await exportCsv({ from: fromParam, to: toParam });
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

  const handleOpenDrilldown = (selectedCh?: SalesChannel) => {
    setDrilldownChannel(selectedCh || (channel === 'ALL' ? undefined : (channel as SalesChannel)));
    setIsDrilldownOpen(true);
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
          <Button variant="secondary" onClick={() => handleOpenDrilldown()}>
            🔍 Chi Tiết Đơn Nguồn (Drilldown)
          </Button>
          <Button variant="secondary" onClick={handleExportCsv}>
            📥 Xuất Báo Cáo CSV
          </Button>
          <Button variant="primary" onClick={handleApplyFilter}>
            Làm mới
          </Button>
        </div>
      </div>

      {/* Filter Toolbar */}
      <div
        style={{
          background: '#ffffff',
          borderRadius: '8px',
          border: '1px solid var(--color-border-hairline)',
          padding: '14px 18px',
          marginBottom: '20px',
          display: 'flex',
          gap: '16px',
          alignItems: 'center',
          flexWrap: 'wrap',
        }}
      >
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '13px', fontWeight: 600, color: '#475569' }}>Kênh bán:</label>
          <select
            value={channel}
            onChange={(e) => setChannel(e.target.value)}
            style={{ padding: '6px 10px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px' }}
          >
            <option value="ALL">Tất cả kênh</option>
            <option value="TIKTOK">TikTok Shop</option>
            <option value="SHOPEE">Shopee</option>
            <option value="POS">In-Store POS</option>
          </select>
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '13px', fontWeight: 600, color: '#475569' }}>Từ ngày:</label>
          <input
            type="date"
            value={fromDate}
            onChange={(e) => setFromDate(e.target.value)}
            style={{ padding: '6px 10px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px' }}
          />
        </div>

        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '13px', fontWeight: 600, color: '#475569' }}>Đến ngày:</label>
          <input
            type="date"
            value={toDate}
            onChange={(e) => setToDate(e.target.value)}
            style={{ padding: '6px 10px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px' }}
          />
        </div>

        <button
          type="button"
          onClick={() => {
            setChannel('ALL');
            setFromDate('');
            setToDate('');
          }}
          style={{
            padding: '6px 12px',
            borderRadius: '6px',
            border: '1px solid #e2e8f0',
            background: '#f8fafc',
            color: '#64748b',
            fontSize: '12px',
            cursor: 'pointer',
          }}
        >
          Đặt lại bộ lọc
        </button>
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

      {/* Financial Trend Visualizer */}
      <FinancialTrendChart points={trend} loading={loading} />

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
        <div style={{ padding: '16px', borderBottom: '1px solid var(--color-border-hairline)', fontWeight: 700, fontSize: '15px', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span>Phân Tích Hiệu Quả Kênh Bán Hàng</span>
          <span style={{ fontSize: '12px', color: 'var(--color-text-secondary)', fontWeight: 'normal' }}>
            Nhấp vào hàng kênh để lọc chi tiết đơn nguồn
          </span>
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
              <th style={{ padding: '12px 16px', textAlign: 'right' }}>THAO TÁC</th>
            </tr>
          </thead>
          <tbody>
            {channels.length === 0 ? (
              <tr>
                <td colSpan={7} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
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
                  <td style={{ padding: '12px 16px', textAlign: 'right' }}>
                    <button
                      type="button"
                      onClick={() => handleOpenDrilldown(ch.channel)}
                      style={{
                        padding: '4px 8px',
                        fontSize: '11px',
                        fontWeight: 600,
                        borderRadius: '4px',
                        border: '1px solid #cbd5e1',
                        background: '#f8fafc',
                        color: '#334155',
                        cursor: 'pointer',
                      }}
                    >
                      Xem đơn nguồn
                    </button>
                  </td>
                </tr>
              ))
            )}
          </tbody>
        </table>
      </div>

      {/* Drilldown Modal */}
      <DrilldownOrdersModal
        isOpen={isDrilldownOpen}
        channel={drilldownChannel}
        from={fromDate || undefined}
        to={toDate || undefined}
        onClose={() => setIsDrilldownOpen(false)}
      />
    </div>
  );
};
