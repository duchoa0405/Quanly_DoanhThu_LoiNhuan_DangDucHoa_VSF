import React, { useState, useEffect } from 'react';
import { catalogApi } from '../../catalog/api/catalogApi';
import { SelectableVariantResponse } from '../../catalog/types/catalog.types';
import { ordersApi } from '../api/ordersApi';
import {
  CreateOrderItemPayload,
  CreateOrderPayload,
  FeeBreakdownResponse,
  PaymentMethod,
  SalesChannel,
} from '../types/order.types';
import { Button } from '../../../shared/ui/Button';
import { MoneyText } from '../../../shared/ui/MoneyText';

interface CreateOrderModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
}

export const CreateOrderModal: React.FC<CreateOrderModalProps> = ({ isOpen, onClose, onSuccess }) => {
  const [channel, setChannel] = useState<SalesChannel>('TIKTOK');
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>('MARKETPLACE_WALLET');
  const [externalOrderId, setExternalOrderId] = useState<string>('');
  const [customerName, setCustomerName] = useState<string>('');
  const [customerPhone, setCustomerPhone] = useState<string>('');
  const [shopVoucher, setShopVoucher] = useState<string>('0');

  // Variant selector
  const [variants, setVariants] = useState<SelectableVariantResponse[]>([]);
  const [selectedVariantId, setSelectedVariantId] = useState<string>('');
  const [quantity, setQuantity] = useState<number>(1);
  const [unitPrice, setUnitPrice] = useState<number>(0);
  const [items, setItems] = useState<CreateOrderItemPayload[]>([]);

  // Fee preview
  const [feePreview, setFeePreview] = useState<FeeBreakdownResponse | null>(null);
  const [previewLoading, setPreviewLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (isOpen) {
      catalogApi.getSelectableVariants().then((res) => {
        setVariants(res);
        if (res.length > 0) {
          setSelectedVariantId(res[0].id);
          setUnitPrice(res[0].retailPrice);
        }
      });
      // Generate a mock external order ID for convenience
      setExternalOrderId(`ORD-${Date.now().toString().slice(-6)}`);
    }
  }, [isOpen]);

  // Adjust payment method options when channel changes
  const handleChannelChange = (newChannel: SalesChannel) => {
    setChannel(newChannel);
    if (newChannel === 'POS') {
      setPaymentMethod('CASH');
    } else {
      setPaymentMethod('MARKETPLACE_WALLET');
    }
  };

  const handleVariantChange = (variantId: string) => {
    setSelectedVariantId(variantId);
    const found = variants.find((v) => v.id === variantId);
    if (found) {
      setUnitPrice(found.retailPrice);
    }
  };

  const handleAddItem = () => {
    if (!selectedVariantId) return;
    if (quantity <= 0) {
      alert('Số lượng phải lớn hơn 0');
      return;
    }
    if (unitPrice < 0) {
      alert('Đơn giá không được âm');
      return;
    }

    if (items.some((i) => i.productVariantId === selectedVariantId)) {
      alert('Sản phẩm đã tồn tại trong đơn hàng. Vui lòng điều chỉnh số lượng.');
      return;
    }

    setItems([...items, { productVariantId: selectedVariantId, quantity, unitPrice }]);
  };

  const handleRemoveItem = (variantId: string) => {
    setItems(items.filter((i) => i.productVariantId !== variantId));
  };

  const calculateSubtotal = () => {
    return items.reduce((acc, i) => acc + i.quantity * i.unitPrice, 0);
  };

  // Recalculate fee preview when subtotal, voucher, channel, or paymentMethod change
  useEffect(() => {
    const subtotal = calculateSubtotal();
    const parsedVoucher = parseFloat(shopVoucher) || 0;

    if (subtotal > 0 && parsedVoucher <= subtotal) {
      setPreviewLoading(true);
      ordersApi
        .previewFee({
          channel,
          paymentMethod,
          subtotal,
          shopVoucher: parsedVoucher,
        })
        .then((res) => setFeePreview(res))
        .catch((err) => console.error('Fee preview error:', err))
        .finally(() => setPreviewLoading(false));
    } else {
      setFeePreview(null);
    }
  }, [items, shopVoucher, channel, paymentMethod]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (items.length === 0) {
      alert('Vui lòng thêm ít nhất một sản phẩm vào đơn hàng.');
      return;
    }

    const subtotal = calculateSubtotal();
    const parsedVoucher = parseFloat(shopVoucher) || 0;
    if (parsedVoucher > subtotal) {
      alert('Shop voucher không được lớn hơn tổng giá trị hàng (Subtotal).');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      const payload: CreateOrderPayload = {
        externalOrderId: externalOrderId.trim(),
        channel,
        paymentMethod,
        customerName: customerName.trim() || undefined,
        customerPhone: customerPhone.trim() || undefined,
        shopVoucher: parsedVoucher,
        items,
      };

      const result = await ordersApi.createOrder(payload);
      if (result.status === 'DELIVERED') {
        alert('Đơn hàng POS tại quầy đã được tạo và thanh toán thành công (DELIVERED)!');
      } else {
        alert('Đơn hàng trực tuyến đã được tạo thành công ở trạng thái PENDING!');
      }

      onSuccess();
      onClose();
    } catch (err: unknown) {
      console.error('Failed to create order:', err);
      setError('Tạo đơn hàng thất bại. Vui lòng kiểm tra lại dữ liệu và quyền truy cập.');
    } finally {
      setSubmitting(false);
    }
  };

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
          maxWidth: '680px',
          maxHeight: '90vh',
          overflowY: 'auto',
          boxShadow: '0 20px 25px -5px rgba(0, 0, 0, 0.1)',
        }}
      >
        <div
          style={{
            padding: '20px 24px',
            borderBottom: '1px solid #e2e8f0',
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
          }}
        >
          <div>
            <h2 style={{ fontSize: '18px', fontWeight: 700, margin: 0 }}>Tạo Đơn Hàng Mới (Live Fee Preview)</h2>
            <p style={{ fontSize: '12px', color: '#64748b', margin: '4px 0 0 0' }}>
              {channel === 'POS'
                ? 'Đơn tại quầy POS hoàn tất thanh toán sẽ trực tiếp chuyển sang trạng thái DELIVERED.'
                : 'Đơn trực tuyến khởi tạo ở trạng thái PENDING trước khi đóng gói & giao hàng.'}
            </p>
          </div>
          <button
            type="button"
            onClick={onClose}
            style={{
              background: 'none',
              border: 'none',
              fontSize: '20px',
              cursor: 'pointer',
              color: '#94a3b8',
            }}
          >
            ✕
          </button>
        </div>

        <form onSubmit={handleSubmit} style={{ padding: '24px' }}>
          {error && (
            <div
              style={{
                padding: '10px 14px',
                marginBottom: '16px',
                borderRadius: '6px',
                background: '#fef2f2',
                color: '#b91c1c',
                fontSize: '13px',
              }}
            >
              {error}
            </div>
          )}

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Kênh bán hàng:
              </label>
              <select
                value={channel}
                onChange={(e) => handleChannelChange(e.target.value as SalesChannel)}
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              >
                <option value="TIKTOK">TikTok Shop (Online)</option>
                <option value="SHOPEE">Shopee (Online)</option>
                <option value="POS">In-Store POS (Tại quầy)</option>
              </select>
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Phương thức thanh toán:
              </label>
              <select
                value={paymentMethod}
                onChange={(e) => setPaymentMethod(e.target.value as PaymentMethod)}
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              >
                {channel === 'POS' ? (
                  <>
                    <option value="CASH">Tiền mặt (CASH)</option>
                    <option value="POS_CARD_QR">Thẻ / Chuyển khoản QR (POS_CARD_QR)</option>
                  </>
                ) : (
                  <option value="MARKETPLACE_WALLET">Ví sàn TMĐT (MARKETPLACE_WALLET)</option>
                )}
              </select>
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '16px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Mã đơn hàng (External Order ID):
              </label>
              <input
                type="text"
                required
                value={externalOrderId}
                onChange={(e) => setExternalOrderId(e.target.value)}
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              />
            </div>

            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Shop Voucher giảm giá (₫):
              </label>
              <input
                type="number"
                min="0"
                value={shopVoucher}
                onChange={(e) => setShopVoucher(e.target.value)}
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '16px', marginBottom: '20px' }}>
            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Tên khách hàng:
              </label>
              <input
                type="text"
                value={customerName}
                onChange={(e) => setCustomerName(e.target.value)}
                placeholder="Khách vãng lai"
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              />
            </div>
            <div>
              <label style={{ display: 'block', fontSize: '13px', fontWeight: 600, marginBottom: '6px' }}>
                Số điện thoại:
              </label>
              <input
                type="text"
                value={customerPhone}
                onChange={(e) => setCustomerPhone(e.target.value)}
                placeholder="09..."
                style={{ width: '100%', padding: '8px 12px', borderRadius: '6px', border: '1px solid #cbd5e1' }}
              />
            </div>
          </div>

          {/* Product Items Selection */}
          <div style={{ background: '#f8fafc', padding: '16px', borderRadius: '8px', marginBottom: '20px' }}>
            <h3 style={{ fontSize: '14px', fontWeight: 700, margin: '0 0 12px 0' }}>Thêm sản phẩm (Catalog Integration)</h3>
            <div style={{ display: 'grid', gridTemplateColumns: '2fr 1fr 1fr auto', gap: '10px', alignItems: 'flex-end' }}>
              <div>
                <label style={{ display: 'block', fontSize: '12px', color: '#64748b', marginBottom: '4px' }}>
                  Biến thể SKU:
                </label>
                <select
                  value={selectedVariantId}
                  onChange={(e) => handleVariantChange(e.target.value)}
                  style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '13px' }}
                >
                  {variants.map((v) => (
                    <option key={v.id} value={v.id}>
                      {v.skuCode} - {v.productName} ({v.color || 'Free'} / {v.size || 'Free'})
                    </option>
                  ))}
                </select>
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '12px', color: '#64748b', marginBottom: '4px' }}>
                  Số lượng:
                </label>
                <input
                  type="number"
                  min="1"
                  value={quantity}
                  onChange={(e) => setQuantity(parseInt(e.target.value) || 1)}
                  style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '13px' }}
                />
              </div>
              <div>
                <label style={{ display: 'block', fontSize: '12px', color: '#64748b', marginBottom: '4px' }}>
                  Đơn giá bán (₫):
                </label>
                <input
                  type="number"
                  min="0"
                  value={unitPrice}
                  onChange={(e) => setUnitPrice(parseFloat(e.target.value) || 0)}
                  style={{ width: '100%', padding: '6px 10px', borderRadius: '4px', border: '1px solid #cbd5e1', fontSize: '13px' }}
                />
              </div>
              <Button variant="secondary" type="button" onClick={handleAddItem}>
                + Thêm
              </Button>
            </div>

            {/* Added items table */}
            {items.length > 0 && (
              <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '12px', fontSize: '12px' }}>
                <thead>
                  <tr style={{ borderBottom: '1px solid #e2e8f0', color: '#64748b', textAlign: 'left' }}>
                    <th style={{ padding: '6px 8px' }}>SKU</th>
                    <th style={{ padding: '6px 8px' }}>SL</th>
                    <th style={{ padding: '6px 8px' }}>Đơn giá</th>
                    <th style={{ padding: '6px 8px' }}>Thành tiền</th>
                    <th style={{ padding: '6px 8px' }}>Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {items.map((item) => {
                    const variant = variants.find((v) => v.id === item.productVariantId);
                    return (
                      <tr key={item.productVariantId} style={{ borderBottom: '1px solid #f1f5f9' }}>
                        <td style={{ padding: '6px 8px', fontWeight: 600 }}>{variant?.skuCode || item.productVariantId}</td>
                        <td style={{ padding: '6px 8px' }}>{item.quantity}</td>
                        <td style={{ padding: '6px 8px' }}>
                          <MoneyText amount={item.unitPrice} />
                        </td>
                        <td style={{ padding: '6px 8px', fontWeight: 600 }}>
                          <MoneyText amount={item.quantity * item.unitPrice} />
                        </td>
                        <td style={{ padding: '6px 8px' }}>
                          <button
                            type="button"
                            onClick={() => handleRemoveItem(item.productVariantId)}
                            style={{ color: '#ef4444', background: 'none', border: 'none', cursor: 'pointer' }}
                          >
                            Xóa
                          </button>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            )}
          </div>

          {/* Live Fee Preview Box */}
          {feePreview && (
            <div
              style={{
                background: '#eff6ff',
                border: '1px solid #bfdbfe',
                borderRadius: '8px',
                padding: '16px',
                marginBottom: '20px',
                fontSize: '13px',
              }}
            >
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '8px' }}>
                <span style={{ fontWeight: 700, color: '#1e40af' }}>⚡ Live Fee Preview (Bóc Tách Phí Sàn Backend)</span>
                {previewLoading && <span style={{ fontSize: '11px', color: '#64748b' }}>Đang tính toán...</span>}
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '10px' }}>
                <div>
                  <span style={{ color: '#64748b' }}>Doanh thu gộp:</span>{' '}
                  <strong>
                    <MoneyText amount={feePreview.grossRevenue} />
                  </strong>
                </div>
                <div>
                  <span style={{ color: '#64748b' }}>Tổng phí sàn:</span>{' '}
                  <strong style={{ color: '#ef4444' }}>
                    <MoneyText amount={feePreview.totalPlatformFees} />
                  </strong>
                </div>
                <div>
                  <span style={{ color: '#64748b' }}>Dự kiến ví nhận:</span>{' '}
                  <strong style={{ color: '#10b981' }}>
                    <MoneyText amount={feePreview.projectedSettlement} />
                  </strong>
                </div>
              </div>
              <div style={{ marginTop: '8px', fontSize: '11px', color: '#64748b', display: 'flex', gap: '12px' }}>
                <span>Hoa hồng: {feePreview.commissionFee.toLocaleString()}₫</span>
                <span>Phí TT: {feePreview.paymentFee.toLocaleString()}₫</span>
                <span>Dịch vụ: {feePreview.serviceFee.toLocaleString()}₫</span>
                <span>Cố định: {feePreview.fixedFee.toLocaleString()}₫</span>
              </div>
            </div>
          )}

          <div style={{ display: 'flex', justifyContent: 'flex-end', gap: '12px' }}>
            <Button variant="secondary" type="button" onClick={onClose} disabled={submitting}>
              Hủy bỏ
            </Button>
            <Button variant="primary" type="submit" disabled={submitting}>
              {submitting ? 'Đang lưu đơn...' : '+ Xác Nhận Lưu Đơn'}
            </Button>
          </div>
        </form>
      </div>
    </div>
  );
};
