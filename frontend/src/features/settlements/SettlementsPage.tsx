import React, { useEffect, useState, useCallback } from 'react';
import { settlementsApi } from './api/settlementsApi';
import { DiscrepancyType, SettlementLedgerItem, SettlementSummaryResponse } from './types/settlement.types';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const SettlementsPage: React.FC = () => {
  const [items, setItems] = useState<SettlementLedgerItem[]>([]);
  const [summary, setSummary] = useState<SettlementSummaryResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Reconcile Modal state
  const [reconcileModalOrder, setReconcileModalOrder] = useState<SettlementLedgerItem | null>(null);
  const [actualAmount, setActualAmount] = useState<string>('');
  const [reconcileNote, setReconcileNote] = useState<string>('');
  const [discrepancyType, setDiscrepancyType] = useState<DiscrepancyType>('COMMISSION_RATE_MISMATCH');
  const [submitting, setSubmitting] = useState(false);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [ledgerRes, summaryRes] = await Promise.all([
        settlementsApi.getLedger({ pageSize: 50 }),
        settlementsApi.getSummary().catch(() => null),
      ]);
      setItems(ledgerRes.items || []);
      setSummary(summaryRes);
    } catch (err: unknown) {
      console.error('Failed to load settlements:', err);
      setError('Không thể kết nối đến sổ cái đối soát.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const handleReconcileSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!reconcileModalOrder) return;

    const parsedActual = parseFloat(actualAmount);
    if (isNaN(parsedActual) || parsedActual < 0) {
      alert('Vui lòng nhập số tiền thực nhận hợp lệ.');
      return;
    }

    const variance = reconcileModalOrder.projectedSettlement - parsedActual;
    if (variance !== 0 && !reconcileNote.trim()) {
      alert('Ghi chú là bắt buộc khi phát hiện sai lệch số tiền.');
      return;
    }

    setSubmitting(true);
    try {
      await settlementsApi.reconcileOrder(reconcileModalOrder.orderId, {
        actualSettlement: parsedActual,
        notes: reconcileNote.trim() || undefined,
        discrepancyType: variance !== 0 ? discrepancyType : undefined,
      });
      setReconcileModalOrder(null);
      setActualAmount('');
      setReconcileNote('');
      await loadData();
    } catch (err: unknown) {
      console.error('Failed to reconcile order:', err);
      alert('Đối soát thất bại. Vui lòng kiểm tra lại dữ liệu.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Phí Sàn & Đối Soát Ví Thực Nhận (SCR-02)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Theo dõi sổ cái đối soát, ghi nhận thực tế ví sàn và quản lý biên bản sai lệch phí.
          </p>
        </div>
        <Button variant="primary" onClick={loadData}>
          Làm mới sổ cái
        </Button>
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '16px', marginBottom: '24px' }}>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid var(--color-border-hairline)' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>Chờ đối soát</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0' }}>
            {summary?.pendingSettlementCount ?? items.filter((i) => i.reconciliationStatus === 'PENDING_SETTLEMENT').length} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Cần xác nhận thực nhận ví</div>
        </div>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid #86efac' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-positive)' }}>Đã khớp 100%</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-positive)' }}>
            {summary?.reconciledCount ?? items.filter((i) => i.reconciliationStatus === 'RECONCILED').length} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Không có chênh lệch</div>
        </div>
        <div style={{ background: '#ffffff', padding: '16px', borderRadius: '8px', border: '1px solid #fca5a5' }}>
          <div style={{ fontSize: '12px', color: 'var(--color-danger)' }}>Phát hiện sai lệch</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-danger)' }}>
            {summary?.discrepancyCount ?? items.filter((i) => i.reconciliationStatus === 'DISCREPANCY').length} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Đã lập biên bản lệch</div>
        </div>
      </div>

      {error && (
        <div style={{ padding: '12px 16px', marginBottom: '16px', borderRadius: '6px', background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', fontSize: '13px' }}>
          {error}
        </div>
      )}

      {loading ? (
        <div style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
          Đang tải sổ cái đối soát...
        </div>
      ) : (
        <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
            <thead>
              <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
                <th style={{ padding: '12px 16px' }}>MÃ ĐƠN</th>
                <th style={{ padding: '12px 16px' }}>KÊNH</th>
                <th style={{ padding: '12px 16px' }}>DOANH THU GỘP</th>
                <th style={{ padding: '12px 16px' }}>TỔNG PHÍ SÀN</th>
                <th style={{ padding: '12px 16px' }}>DỰ KIẾN NHẬN</th>
                <th style={{ padding: '12px 16px' }}>THỰC NHẬN</th>
                <th style={{ padding: '12px 16px' }}>CHÊNH LỆCH</th>
                <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
                <th style={{ padding: '12px 16px' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 ? (
                <tr>
                  <td colSpan={9} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                    Chưa có dữ liệu đối soát đơn hàng nào.
                  </td>
                </tr>
              ) : (
                items.map((item) => (
                  <tr key={item.id} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                    <td style={{ padding: '12px 16px', fontWeight: 600 }}>{item.externalOrderId}</td>
                    <td style={{ padding: '12px 16px' }}>
                      <Badge label={item.channel} />
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      <MoneyText amount={item.grossRevenue} />
                    </td>
                    <td style={{ padding: '12px 16px', color: 'var(--color-danger)' }}>
                      <MoneyText amount={item.totalPlatformFees} />
                    </td>
                    <td style={{ padding: '12px 16px', fontWeight: 500 }}>
                      <MoneyText amount={item.projectedSettlement} />
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      {item.actualSettlement != null ? <MoneyText amount={item.actualSettlement} /> : '-'}
                    </td>
                    <td style={{ padding: '12px 16px', color: (item.varianceAmount ?? 0) !== 0 ? 'var(--color-danger)' : 'inherit' }}>
                      {item.varianceAmount != null ? <MoneyText amount={item.varianceAmount} /> : '-'}
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      <Badge
                        label={item.reconciliationStatus}
                        variant={
                          item.reconciliationStatus === 'RECONCILED'
                            ? 'success'
                            : item.reconciliationStatus === 'DISCREPANCY'
                            ? 'danger'
                            : 'warning'
                        }
                      />
                    </td>
                    <td style={{ padding: '12px 16px' }}>
                      {item.reconciliationStatus === 'PENDING_SETTLEMENT' ? (
                        <button
                          type="button"
                          onClick={() => {
                            setReconcileModalOrder(item);
                            setActualAmount(item.projectedSettlement.toString());
                          }}
                          style={{
                            padding: '4px 10px',
                            background: 'var(--color-primary, #6366f1)',
                            color: '#ffffff',
                            border: 'none',
                            borderRadius: '4px',
                            fontSize: '12px',
                            cursor: 'pointer',
                          }}
                        >
                          Đối soát
                        </button>
                      ) : (
                        <span style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>Hoàn tất</span>
                      )}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Modal Dialog for Recording Settlement */}
      {reconcileModalOrder && (
        <div style={{ position: 'fixed', inset: 0, background: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 100 }}>
          <div style={{ background: '#ffffff', borderRadius: '8px', padding: '24px', width: '440px', maxWidth: '90%' }}>
            <h2 style={{ fontSize: '16px', fontWeight: 700, marginBottom: '16px' }}>
              Xác Nhận Đối Soát: {reconcileModalOrder.externalOrderId}
            </h2>
            <form onSubmit={handleReconcileSubmit}>
              <div style={{ marginBottom: '12px' }}>
                <label style={{ display: 'block', fontSize: '12px', marginBottom: '4px', color: 'var(--color-text-secondary)' }}>
                  Dự kiến nhận (Projected Settlement):
                </label>
                <div style={{ fontWeight: 600, fontSize: '15px' }}>
                  <MoneyText amount={reconcileModalOrder.projectedSettlement} />
                </div>
              </div>

              <div style={{ marginBottom: '12px' }}>
                <label style={{ display: 'block', fontSize: '12px', marginBottom: '4px', fontWeight: 600 }}>
                  Thực nhận từ ví sàn (VND):
                </label>
                <input
                  type="number"
                  step="1"
                  required
                  value={actualAmount}
                  onChange={(e) => setActualAmount(e.target.value)}
                  style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }}
                />
              </div>

              {parseFloat(actualAmount) !== reconcileModalOrder.projectedSettlement && (
                <>
                  <div style={{ marginBottom: '12px' }}>
                    <label style={{ display: 'block', fontSize: '12px', marginBottom: '4px', fontWeight: 600, color: 'var(--color-danger)' }}>
                      Loại sai lệch (Bắt buộc khi có chênh lệch):
                    </label>
                    <select
                      value={discrepancyType}
                      onChange={(e) => setDiscrepancyType(e.target.value as DiscrepancyType)}
                      style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }}
                    >
                      <option value="COMMISSION_RATE_MISMATCH">Sai tỷ lệ hoa hồng (COMMISSION_RATE_MISMATCH)</option>
                      <option value="PAYMENT_FEE_MISMATCH">Sai phí thanh toán (PAYMENT_FEE_MISMATCH)</option>
                      <option value="SERVICE_FEE_MISMATCH">Sai phí dịch vụ (SERVICE_FEE_MISMATCH)</option>
                      <option value="UNEXPECTED_PLATFORM_CHARGE">Thu phí sàn bất thường (UNEXPECTED_PLATFORM_CHARGE)</option>
                      <option value="OTHER">Lý do khác (OTHER)</option>
                    </select>
                  </div>

                  <div style={{ marginBottom: '16px' }}>
                    <label style={{ display: 'block', fontSize: '12px', marginBottom: '4px', fontWeight: 600, color: 'var(--color-danger)' }}>
                      Ghi chú giải trình sai lệch:
                    </label>
                    <textarea
                      required
                      rows={3}
                      value={reconcileNote}
                      onChange={(e) => setReconcileNote(e.target.value)}
                      placeholder="Mô tả nguyên nhân sai lệch hoặc số tiền chênh lệch..."
                      style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #cbd5e1' }}
                    />
                  </div>
                </>
              )}

              <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '8px' }}>
                <Button variant="secondary" type="button" onClick={() => setReconcileModalOrder(null)}>
                  Hủy
                </Button>
                <Button variant="primary" type="submit" disabled={submitting}>
                  {submitting ? 'Đang lưu...' : 'Xác nhận đối soát'}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};
