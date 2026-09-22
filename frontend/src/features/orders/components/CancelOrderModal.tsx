import React, { useState } from 'react';
import { ordersApi } from '../api/ordersApi';
import { OrderListItemResponse } from '../types/order.types';
import { Button } from '../../../shared/ui/Button';

interface CancelOrderModalProps {
  order: OrderListItemResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CancelOrderModal: React.FC<CancelOrderModalProps> = ({ order, isOpen, onClose, onSuccess }) => {
  const [reason, setReason] = useState<string>('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !order) return null;

  const handleCancelSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!reason.trim()) {
      alert('Vui lòng nhập lý do hủy đơn hàng.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      await ordersApi.cancelOrder(order.id, { cancellationReason: reason.trim() });
      alert('Đơn hàng đã được hủy thành công!');
      onSuccess();
      onClose();
    } catch (err: unknown) {
      console.error('Failed to cancel order:', err);
      setError('Hủy đơn hàng thất bại. Lưu ý: Đơn hàng đã DELIVERED không thể hủy.');
    } finally {
      setSubmitting(false);
    }
  };

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
          maxWidth: '480px',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
        }}
      >
        <div
          style={{
            padding: '16px 20px',
            borderBottom: '1px solid #e2e8f0',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <h2 style={{ fontSize: '16px', fontWeight: 700, margin: 0, color: '#dc2626' }}>
            Xác Nhận Hủy Đơn Hàng ({order.externalOrderId})
          </h2>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: 'none', fontSize: '18px', cursor: 'pointer', color: '#94a3b8' }}
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleCancelSubmit} style={{ padding: '20px' }}>
          <div
            style={{
              padding: '12px',
              borderRadius: '6px',
              background: '#fef2f2',
              border: '1px solid #fecaca',
              color: '#991b1b',
              fontSize: '12px',
              marginBottom: '16px',
            }}
          >
            ⚠️ <strong>Cảnh báo tài chính:</strong> Đơn hàng bị hủy sẽ bị loại bỏ hoàn toàn khỏi doanh thu và lợi nhuận ghi nhận (đóng góp 0 ₫ vào báo cáo).
          </div>

          {error && (
            <div
              style={{
                padding: '10px 14px',
                marginBottom: '14px',
                borderRadius: '6px',
                background: '#fef2f2',
                color: '#b91c1c',
                fontSize: '12px',
              }}
            >
              {error}
            </div>
          )}

          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
              Lý do hủy đơn (Bắt buộc):
            </label>
            <textarea
              required
              rows={3}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              placeholder="Nhập chi tiết lý do khách hủy hoặc hoàn trả hàng trước giao..."
              style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px' }}
            />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
            <Button variant="secondary" type="button" onClick={onClose} disabled={submitting}>
              Đóng
            </Button>
            <Button variant="primary" type="submit" disabled={submitting}>
              {submitting ? 'Đang xử lý...' : 'Xác Nhận Hủy'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
