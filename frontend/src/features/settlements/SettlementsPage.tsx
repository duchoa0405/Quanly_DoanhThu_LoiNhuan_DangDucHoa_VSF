import React, { useEffect, useState, useCallback } from 'react';
import { useSettlement } from './hooks/useSettlement';
import { DiscrepancyType, ReconciliationStatus, SettlementLedgerItem } from './types/settlement.types';
import { SalesChannel } from '../orders/types/order.types';
import { discrepanciesApi } from '../discrepancies/api/discrepanciesApi';
import { DiscrepancyItemResponse } from '../discrepancies/types/discrepancy.types';
import { ResolveDiscrepancyModal } from '../discrepancies/components/ResolveDiscrepancyModal';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const SettlementsPage: React.FC = () => {
  const { items, summary, loading, error, fetchLedger, reconcileOrder } = useSettlement();

  // Tab & Filters
  const [activeTab, setActiveTab] = useState<'ledger' | 'discrepancies'>('ledger');
  const [selectedChannel, setSelectedChannel] = useState<string>('ALL');
  const [selectedStatus, setSelectedStatus] = useState<string>('ALL');

  // Discrepancy list state for Tab 2
  const [discrepancies, setDiscrepancies] = useState<DiscrepancyItemResponse[]>([]);
  const [loadingDiscrepancies, setLoadingDiscrepancies] = useState(false);

  // Modals state
  const [reconcileModalOrder, setReconcileModalOrder] = useState<SettlementLedgerItem | null>(null);
  const [actualAmount, setActualAmount] = useState<string>('');
  const [reconcileNote, setReconcileNote] = useState<string>('');
  const [discrepancyType, setDiscrepancyType] = useState<DiscrepancyType>('COMMISSION_RATE_MISMATCH');
  const [submittingReconcile, setSubmittingReconcile] = useState(false);

  const [activeDiscrepancyToResolve, setActiveDiscrepancyToResolve] = useState<DiscrepancyItemResponse | null>(null);

  const loadLedgerData = useCallback(() => {
    const channelParam = selectedChannel === 'ALL' ? undefined : (selectedChannel as SalesChannel);
    const statusParam = selectedStatus === 'ALL' ? undefined : (selectedStatus as ReconciliationStatus);
    fetchLedger({ channel: channelParam, status: statusParam, pageSize: 50 });
  }, [selectedChannel, selectedStatus, fetchLedger]);

  const loadDiscrepancyData = useCallback(async () => {
    setLoadingDiscrepancies(true);
    try {
      const channelParam = selectedChannel === 'ALL' ? undefined : (selectedChannel as SalesChannel);
      const res = await discrepanciesApi.getDiscrepancies({ channel: channelParam, pageSize: 50 });
      setDiscrepancies(res.items || []);
    } catch (err) {
      console.error('Failed to load discrepancies:', err);
    } finally {
      setLoadingDiscrepancies(false);
    }
  }, [selectedChannel]);

  useEffect(() => {
    loadLedgerData();
  }, [loadLedgerData]);

  useEffect(() => {
    if (activeTab === 'discrepancies') {
      loadDiscrepancyData();
    }
  }, [activeTab, loadDiscrepancyData]);

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

    setSubmittingReconcile(true);
    try {
      await reconcileOrder(reconcileModalOrder.orderId, {
        actualSettlement: parsedActual,
        notes: reconcileNote.trim() || undefined,
        discrepancyType: variance !== 0 ? discrepancyType : undefined,
      });
      setReconcileModalOrder(null);
      setActualAmount('');
      setReconcileNote('');
      loadLedgerData();
      if (activeTab === 'discrepancies') loadDiscrepancyData();
    } catch (err: unknown) {
      console.error('Failed to reconcile order:', err);
      alert('Đối soát thất bại. Vui lòng kiểm tra lại dữ liệu.');
    } finally {
      setSubmittingReconcile(false);
    }
  };

  const handleOpenDiscrepancyFromRow = async (item: SettlementLedgerItem) => {
    try {
      const res = await discrepanciesApi.getDiscrepancies({ search: item.externalOrderId, pageSize: 1 });
      if (res.items && res.items.length > 0) {
        setActiveDiscrepancyToResolve(res.items[0]);
      } else {
        alert('Không tìm thấy biên bản sai lệch cho đơn hàng này.');
      }
    } catch (err) {
      console.error('Failed to get discrepancy:', err);
      alert('Không thể mở biên bản sai lệch.');
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
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="secondary" onClick={() => { loadLedgerData(); if (activeTab === 'discrepancies') loadDiscrepancyData(); }}>
            Làm mới
          </Button>
        </div>
      </div>

      {/* Summary Rhythm Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '16px', marginBottom: '24px' }}>
        <div
          onClick={() => setSelectedStatus(selectedStatus === 'PENDING_SETTLEMENT' ? 'ALL' : 'PENDING_SETTLEMENT')}
          style={{
            background: '#ffffff',
            padding: '16px',
            borderRadius: '8px',
            border: selectedStatus === 'PENDING_SETTLEMENT' ? '2px solid #f59e0b' : '1px solid var(--color-border-hairline)',
            cursor: 'pointer',
          }}
        >
          <div style={{ fontSize: '12px', color: 'var(--color-text-secondary)' }}>Chờ đối soát</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: '#d97706' }}>
            {summary?.pendingSettlementCount ?? 0} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Cần xác nhận thực nhận ví</div>
        </div>

        <div
          onClick={() => setSelectedStatus(selectedStatus === 'RECONCILED' ? 'ALL' : 'RECONCILED')}
          style={{
            background: '#ffffff',
            padding: '16px',
            borderRadius: '8px',
            border: selectedStatus === 'RECONCILED' ? '2px solid #10b981' : '1px solid #86efac',
            cursor: 'pointer',
          }}
        >
          <div style={{ fontSize: '12px', color: 'var(--color-positive)' }}>Đã khớp 100%</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-positive)' }}>
            {summary?.reconciledCount ?? 0} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Không có chênh lệch</div>
        </div>

        <div
          onClick={() => setSelectedStatus(selectedStatus === 'DISCREPANCY' ? 'ALL' : 'DISCREPANCY')}
          style={{
            background: '#ffffff',
            padding: '16px',
            borderRadius: '8px',
            border: selectedStatus === 'DISCREPANCY' ? '2px solid #ef4444' : '1px solid #fca5a5',
            cursor: 'pointer',
          }}
        >
          <div style={{ fontSize: '12px', color: 'var(--color-danger)' }}>Phát hiện sai lệch</div>
          <div style={{ fontSize: '22px', fontWeight: 700, margin: '6px 0', color: 'var(--color-danger)' }}>
            {summary?.discrepancyCount ?? 0} đơn
          </div>
          <div style={{ fontSize: '12px', color: 'var(--color-text-muted)' }}>Đã lập biên bản lệch</div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div style={{ display: 'flex', gap: '8px', borderBottom: '1px solid #e2e8f0', marginBottom: '16px' }}>
        <button
          type="button"
          onClick={() => setActiveTab('ledger')}
          style={{
            padding: '8px 16px',
            fontSize: '13px',
            fontWeight: 600,
            border: 'none',
            borderBottom: activeTab === 'ledger' ? '2px solid #6366f1' : '2px solid transparent',
            background: 'none',
            color: activeTab === 'ledger' ? '#4f46e5' : '#64748b',
            cursor: 'pointer',
          }}
        >
          📋 Sổ Cái Đối Soát Đơn Hàng
        </button>
        <button
          type="button"
          onClick={() => setActiveTab('discrepancies')}
          style={{
            padding: '8px 16px',
            fontSize: '13px',
            fontWeight: 600,
            border: 'none',
            borderBottom: activeTab === 'discrepancies' ? '2px solid #ef4444' : '2px solid transparent',
            background: 'none',
            color: activeTab === 'discrepancies' ? '#dc2626' : '#64748b',
            cursor: 'pointer',
          }}
        >
          ⚠️ Hồ Sơ Sai Lệch & Khiếu Nại Sàn ({summary?.discrepancyCount ?? 0})
        </button>
      </div>

      {/* Filter Bar */}
      <div style={{ display: 'flex', gap: '16px', alignItems: 'center', marginBottom: '16px', background: '#f8fafc', padding: '10px 14px', borderRadius: '6px' }}>
        <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
          <label style={{ fontSize: '12px', fontWeight: 600, color: '#475569' }}>Kênh:</label>
          <select
            value={selectedChannel}
            onChange={(e) => setSelectedChannel(e.target.value)}
            style={{ padding: '4px 8px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '12px' }}
          >
            <option value="ALL">Tất cả kênh</option>
            <option value="TIKTOK">TikTok Shop</option>
            <option value="SHOPEE">Shopee</option>
            <option value="POS">POS</option>
          </select>
        </div>

        {activeTab === 'ledger' && (
          <div style={{ display: 'flex', alignItems: 'center', gap: '8px' }}>
            <label style={{ fontSize: '12px', fontWeight: 600, color: '#475569' }}>Trạng thái đối soát:</label>
            <select
              value={selectedStatus}
              onChange={(e) => setSelectedStatus(e.target.value)}
              style={{ padding: '4px 8px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '12px' }}
            >
              <option value="ALL">Tất cả</option>
              <option value="PENDING_SETTLEMENT">Chờ đối soát (PENDING_SETTLEMENT)</option>
              <option value="RECONCILED">Khớp 100% (RECONCILED)</option>
              <option value="DISCREPANCY">Phát hiện sai lệch (DISCREPANCY)</option>
            </select>
          </div>
        )}
      </div>

      {error && (
        <div style={{ padding: '12px 16px', marginBottom: '16px', borderRadius: '6px', background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', fontSize: '13px' }}>
          {error}
        </div>
      )}

      {/* TAB 1: LEDGER */}
      {activeTab === 'ledger' && (
        loading ? (
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
                  <th style={{ padding: '12px 16px', textAlign: 'right' }}>THAO TÁC</th>
                </tr>
              </thead>
              <tbody>
                {items.length === 0 ? (
                  <tr>
                    <td colSpan={9} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                      Chưa có dữ liệu đối soát đơn hàng nào phù hợp bộ lọc.
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
                      <td style={{ padding: '12px 16px', color: (item.varianceAmount ?? 0) !== 0 ? 'var(--color-danger)' : 'inherit', fontWeight: (item.varianceAmount ?? 0) !== 0 ? 600 : 400 }}>
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
                      <td style={{ padding: '12px 16px', textAlign: 'right' }}>
                        {item.reconciliationStatus === 'PENDING_SETTLEMENT' && (
                          <button
                            type="button"
                            onClick={() => {
                              setReconcileModalOrder(item);
                              setActualAmount(item.projectedSettlement.toString());
                            }}
                            style={{
                              padding: '4px 10px',
                              background: '#6366f1',
                              color: '#ffffff',
                              border: 'none',
                              borderRadius: '4px',
                              fontSize: '12px',
                              cursor: 'pointer',
                            }}
                          >
                            Đối soát
                          </button>
                        )}
                        {item.reconciliationStatus === 'DISCREPANCY' && (
                          <button
                            type="button"
                            onClick={() => handleOpenDiscrepancyFromRow(item)}
                            style={{
                              padding: '4px 10px',
                              background: '#fef2f2',
                              color: '#b91c1c',
                              border: '1px solid #fca5a5',
                              borderRadius: '4px',
                              fontSize: '12px',
                              cursor: 'pointer',
                              fontWeight: 600,
                            }}
                          >
                            Xử lý sai lệch
                          </button>
                        )}
                        {item.reconciliationStatus === 'RECONCILED' && (
                          <span style={{ fontSize: '12px', color: '#10b981', fontWeight: 600 }}>✓ Khớp</span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )
      )}

      {/* TAB 2: DISCREPANCIES */}
      {activeTab === 'discrepancies' && (
        loadingDiscrepancies ? (
          <div style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
            Đang tải hồ sơ sai lệch...
          </div>
        ) : (
          <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
              <thead>
                <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
                  <th style={{ padding: '12px 16px' }}>MÃ ĐƠN HÀNG</th>
                  <th style={{ padding: '12px 16px' }}>KÊNH</th>
                  <th style={{ padding: '12px 16px' }}>LOẠI SAI LỆCH</th>
                  <th style={{ padding: '12px 16px' }}>CHÊNH LỆCH VÍ</th>
                  <th style={{ padding: '12px 16px' }}>LÝ DO / GIẢI TRÌNH</th>
                  <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
                  <th style={{ padding: '12px 16px', textAlign: 'right' }}>THAO TÁC</th>
                </tr>
              </thead>
              <tbody>
                {discrepancies.length === 0 ? (
                  <tr>
                    <td colSpan={7} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                      Không có hồ sơ sai lệch nào cần xử lý.
                    </td>
                  </tr>
                ) : (
                  discrepancies.map((d) => (
                    <tr key={d.id} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                      <td style={{ padding: '12px 16px', fontWeight: 600 }}>{d.externalOrderId}</td>
                      <td style={{ padding: '12px 16px' }}>
                        <Badge label={d.channel} />
                      </td>
                      <td style={{ padding: '12px 16px', fontWeight: 500 }}>{d.discrepancyType}</td>
                      <td style={{ padding: '12px 16px', color: '#ef4444', fontWeight: 600 }}>
                        <MoneyText amount={d.varianceAmount} />
                      </td>
                      <td style={{ padding: '12px 16px', color: '#64748b' }}>
                        {d.explanationNote || '-'}
                      </td>
                      <td style={{ padding: '12px 16px' }}>
                        <Badge
                          label={d.isResolved ? 'Đã giải quyết' : 'Đang khiếu nại'}
                          variant={d.isResolved ? 'success' : 'danger'}
                        />
                      </td>
                      <td style={{ padding: '12px 16px', textAlign: 'right' }}>
                        {!d.isResolved ? (
                          <button
                            type="button"
                            onClick={() => setActiveDiscrepancyToResolve(d)}
                            style={{
                              padding: '4px 10px',
                              background: '#ef4444',
                              color: '#ffffff',
                              border: 'none',
                              borderRadius: '4px',
                              fontSize: '12px',
                              cursor: 'pointer',
                              fontWeight: 600,
                            }}
                          >
                            Giải quyết
                          </button>
                        ) : (
                          <span style={{ fontSize: '12px', color: '#10b981', fontWeight: 600 }}>Đã đóng</span>
                        )}
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        )
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
                <Button variant="primary" type="submit" disabled={submittingReconcile}>
                  {submittingReconcile ? 'Đang lưu...' : 'Xác nhận đối soát'}
                </Button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Discrepancy Resolution Modal */}
      <ResolveDiscrepancyModal
        isOpen={activeDiscrepancyToResolve !== null}
        discrepancy={activeDiscrepancyToResolve}
        onClose={() => setActiveDiscrepancyToResolve(null)}
        onSuccess={() => {
          loadLedgerData();
          if (activeTab === 'discrepancies') loadDiscrepancyData();
        }}
      />
    </div>
  );
};
