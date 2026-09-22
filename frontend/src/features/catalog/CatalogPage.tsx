import React, { useEffect, useState } from 'react';
import { useCatalog } from './hooks/useCatalog';
import { ProductVariant } from './types/catalog.types';
import { CreateProductModal } from './components/CreateProductModal';
import { EditVariantModal } from './components/EditVariantModal';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const CatalogPage: React.FC = () => {
  const { products, loading, error, fetchProducts, createProduct, updateVariant } = useCatalog();

  const [isCreateOpen, setIsCreateOpen] = useState(false);
  const [editingVariant, setEditingVariant] = useState<{ variant: ProductVariant; productName: string } | null>(null);

  useEffect(() => {
    fetchProducts({ pageSize: 50 });
  }, [fetchProducts]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Danh Mục Sản Phẩm & Giá Vốn (SCR-04)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Quản lý mã SKU, đơn giá bán lẻ và giá vốn (COGS) phục vụ tính biên lợi nhuận đóng góp.
          </p>
        </div>
        <div style={{ display: 'flex', gap: '8px' }}>
          <Button variant="secondary" onClick={() => fetchProducts({ pageSize: 50 })}>
            Làm mới
          </Button>
          <Button variant="primary" onClick={() => setIsCreateOpen(true)}>
            + Thêm Sản Phẩm Mới
          </Button>
        </div>
      </div>

      {error && (
        <div style={{ padding: '12px 16px', marginBottom: '16px', borderRadius: '6px', background: '#fef2f2', border: '1px solid #fecaca', color: '#b91c1c', fontSize: '13px' }}>
          {error}
        </div>
      )}

      {loading ? (
        <div style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
          Đang tải danh mục...
        </div>
      ) : (
        <div style={{ background: '#ffffff', borderRadius: '8px', border: '1px solid var(--color-border-hairline)', overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left', fontSize: '13px' }}>
            <thead>
              <tr style={{ background: '#f8fafc', borderBottom: '1px solid var(--color-border-hairline)', color: 'var(--color-text-secondary)' }}>
                <th style={{ padding: '12px 16px' }}>MÃ SKU</th>
                <th style={{ padding: '12px 16px' }}>TÊN SẢN PHẨM</th>
                <th style={{ padding: '12px 16px' }}>PHÂN LOẠI / SIZE</th>
                <th style={{ padding: '12px 16px' }}>GIÁ BÁN LẺ</th>
                <th style={{ padding: '12px 16px' }}>GIÁ VỐN (COGS)</th>
                <th style={{ padding: '12px 16px' }}>TRẠNG THÁI</th>
                <th style={{ padding: '12px 16px', textAlign: 'right' }}>THAO TÁC</th>
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={7} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                    Chưa có sản phẩm nào trong hệ thống. Nhấn "+ Thêm Sản Phẩm Mới" để bắt đầu.
                  </td>
                </tr>
              ) : (
                products.flatMap((product) =>
                  product.variants.map((v) => (
                    <tr key={v.id} style={{ borderBottom: '1px solid var(--color-border-hairline)' }}>
                      <td style={{ padding: '12px 16px', fontWeight: 600 }}>{v.skuCode}</td>
                      <td style={{ padding: '12px 16px' }}>{product.name}</td>
                      <td style={{ padding: '12px 16px', color: 'var(--color-text-secondary)' }}>
                        {[v.color, v.size].filter(Boolean).join(' - ') || 'Mặc định'}
                      </td>
                      <td style={{ padding: '12px 16px', fontWeight: 500 }}>
                        <MoneyText amount={v.retailPrice} />
                      </td>
                      <td style={{ padding: '12px 16px', color: 'var(--color-text-secondary)' }}>
                        <MoneyText amount={v.costPrice} />
                      </td>
                      <td style={{ padding: '12px 16px' }}>
                        <Badge label={v.isActive ? 'Đang bán' : 'Ngừng bán'} variant={v.isActive ? 'success' : 'danger'} />
                      </td>
                      <td style={{ padding: '12px 16px', textAlign: 'right' }}>
                        <button
                          type="button"
                          onClick={() => setEditingVariant({ variant: v, productName: product.name })}
                          style={{
                            padding: '4px 10px',
                            fontSize: '11px',
                            fontWeight: 600,
                            borderRadius: '4px',
                            border: '1px solid #cbd5e1',
                            background: '#f8fafc',
                            color: '#334155',
                            cursor: 'pointer',
                          }}
                        >
                          Sửa giá / trạng thái
                        </button>
                      </td>
                    </tr>
                  ))
                )
              )}
            </tbody>
          </table>
        </div>
      )}

      {/* Modals */}
      <CreateProductModal
        isOpen={isCreateOpen}
        onClose={() => setIsCreateOpen(false)}
        onSuccess={createProduct}
      />

      <EditVariantModal
        isOpen={editingVariant !== null}
        variant={editingVariant?.variant || null}
        productName={editingVariant?.productName}
        onClose={() => setEditingVariant(null)}
        onSuccess={updateVariant}
      />
    </div>
  );
};
