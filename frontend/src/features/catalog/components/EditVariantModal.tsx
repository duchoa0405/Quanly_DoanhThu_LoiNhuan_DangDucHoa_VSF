import React, { useState, useEffect } from 'react';
import { ProductVariant, UpdateVariantRequest } from '../types/catalog.types';
import { Button } from '../../../shared/ui/Button';

interface EditVariantModalProps {
  variant: ProductVariant | null;
  productName?: string;
  isOpen: boolean;
  onClose: () => void;
  onSuccess: (variantId: string, req: UpdateVariantRequest) => Promise<unknown>;
}

export const EditVariantModal: React.FC<EditVariantModalProps> = ({
  variant,
  productName,
  isOpen,
  onClose,
  onSuccess,
}) => {
  const [retailPrice, setRetailPrice] = useState<number>(0);
  const [costPrice, setCostPrice] = useState<number>(0);
  const [isActive, setIsActive] = useState<boolean>(true);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (variant) {
      setRetailPrice(variant.retailPrice);
      setCostPrice(variant.costPrice);
      setIsActive(variant.isActive);
    }
  }, [variant]);

  if (!isOpen || !variant) return null;

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (retailPrice < 0 || costPrice < 0) {
      alert('Giá bán lẻ và giá vốn không được âm.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      await onSuccess(variant.id, {
        retailPrice,
        costPrice,
        isActive,
      });
      onClose();
    } catch (err: unknown) {
      console.error('Failed to update variant:', err);
      setError('Cập nhật biến thể thất bại.');
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
          maxWidth: '440px',
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
            <h2 style={{ fontSize: '16px', fontWeight: 700, margin: 0 }}>Cập Nhật Biến Thể SKU</h2>
            <p style={{ fontSize: '12px', color: '#64748b', margin: '4px 0 0 0' }}>
              {productName || 'Sản phẩm'} - <strong>{variant.skuCode}</strong>
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

          <div style={{ marginBottom: '14px' }}>
            <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
              Giá bán lẻ (₫) *:
            </label>
            <input
              type="number"
              min="0"
              required
              value={retailPrice}
              onChange={(e) => setRetailPrice(parseFloat(e.target.value) || 0)}
              style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
            />
          </div>

          <div style={{ marginBottom: '14px' }}>
            <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
              Giá vốn COGS (₫) *:
            </label>
            <input
              type="number"
              min="0"
              required
              value={costPrice}
              onChange={(e) => setCostPrice(parseFloat(e.target.value) || 0)}
              style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
            />
          </div>

          <div style={{ marginBottom: '20px', display: 'flex', alignItems: 'center', gap: '8px' }}>
            <input
              type="checkbox"
              id="isActiveToggle"
              checked={isActive}
              onChange={(e) => setIsActive(e.target.checked)}
              style={{ width: '16px', height: '16px', cursor: 'pointer' }}
            />
            <label htmlFor="isActiveToggle" style={{ fontSize: '13px', fontWeight: 600, cursor: 'pointer' }}>
              Đang kinh doanh (Kích hoạt cho đơn hàng mới)
            </label>
          </div>

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '10px' }}>
            <Button variant="secondary" type="button" onClick={onClose} disabled={submitting}>
              Hủy
            </Button>
            <Button variant="primary" type="submit" disabled={submitting}>
              {submitting ? 'Đang lưu...' : 'Lưu Thay Đổi'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
