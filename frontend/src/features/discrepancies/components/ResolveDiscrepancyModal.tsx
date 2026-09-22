import React, { useState } from 'react';
import { discrepanciesApi } from '../api/discrepanciesApi';
import { DiscrepancyItemResponse } from '../types/discrepancy.types';
import { MoneyText } from '../../../shared/ui/MoneyText';
import { Button } from '../../../shared/ui/Button';

interface ResolveDiscrepancyModalProps {
  discrepancy: DiscrepancyItemResponse | null;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const ResolveDiscrepancyModal: React.FC<ResolveDiscrepancyModalProps> = ({
  discrepancy,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [resolutionNotes, setResolutionNotes] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  if (!isOpen || !discrepancy) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resolutionNotes.trim()) {
      alert('Vui lòng nhập nội dung giải quyết hoặc phương án xử lý sai lệch.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      await discrepanciesApi.resolveDiscrepancy(discrepancy.id, {
        resolutionNotes: resolutionNotes.trim(),
      });
      alert('Đã cập nhật giải quyết biên bản sai lệch thành công!');
      onSuccess();
      onClose();
    } catch (err: unknown) {
      console.error('Failed to resolve discrepancy:', err);
      setError('Xử lý sai lệch thất bại. Vui lòng kiểm tra quyền Finance Manager.');
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
          maxWidth: '500px',
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
          <div>
            <h2 style={{ fontSize: '16px', fontWeight: 700, margin: 0 }}>
              Xử Lý Biên Bản Sai Lệch Đối Soát
            </h2>
            <p style={{ fontSize: '12px', color: '#64748b', margin: '4px 0 0 0' }}>
              Đơn hàng: <strong>{discrepancy.externalOrderId}</strong> ({discrepancy.channel})
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            style={{ background: 'none', border: 'none', fontSize: '18px', cursor: 'pointer', color: '#94a3b8' }}
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ padding: '20px' }}>
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

          <div style={{ background: '#f8fafc', padding: '14px', borderRadius: '8px', marginBottom: '16px' }}>
            <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '8px', fontSize: '12px', marginBottom: '8px' }}>
              <div>
                <span style={{ color: '#64748b' }}>Loại sai lệch:</span>{' '}
                <strong style={{ color: '#ef4444' }}>{discrepancy.discrepancyType}</strong>
              </div>
              <div>
                <span style={{ color: '#64748b' }}>Chênh lệch ví:</span>{' '}
                <strong style={{ color: '#ef4444' }}>
                  <MoneyText amount={discrepancy.varianceAmount} />
                </strong>
              </div>
            </div>
            {discrepancy.explanationNote && (
              <div style={{ fontSize: '12px', color: '#334155' }}>
                <span style={{ color: '#64748b' }}>Giải trình lúc đối soát:</span> {discrepancy.explanationNote}
              </div>
            )}
          </div>

          <div style={{ marginBottom: '16px' }}>
            <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
              Phương án giải quyết & Ghi chú kiểm toán *:
            </label>
            <textarea
              required
              rows={4}
              value={resolutionNotes}
              onChange={(e) => setResolutionNotes(e.target.value)}
              placeholder="Ghi rõ: Sàn đã hoàn phí / Chấp nhận khoản phạt / Khiếu nại thành công..."
              style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1', fontSize: '13px' }}
            />
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
            <Button variant="secondary" type="button" onClick={onClose} disabled={submitting}>
              Hủy
            </Button>
            <Button variant="primary" type="submit" disabled={submitting}>
              {submitting ? 'Đang lưu...' : 'Xác Nhận Giải Quyết'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
