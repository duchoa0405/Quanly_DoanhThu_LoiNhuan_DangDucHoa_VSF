import React, { useEffect, useState, useCallback } from 'react';
import { catalogApi } from './api/catalogApi';
import { Product } from './types/catalog.types';
import { MoneyText } from '../../shared/ui/MoneyText';
import { Badge } from '../../shared/ui/Badge';
import { Button } from '../../shared/ui/Button';

export const CatalogPage: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  const loadProducts = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const data = await catalogApi.getProducts({ pageSize: 50 });
      setProducts(data.items || []);
    } catch (err: unknown) {
      console.error('Failed to load catalog:', err);
      setError('Không thể kết nối đến danh mục sản phẩm.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    loadProducts();
  }, [loadProducts]);

  return (
    <div>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <div>
          <h1 style={{ fontSize: '20px', fontWeight: 700 }}>Danh Mục Sản Phẩm & Giá Vốn (SCR-04)</h1>
          <p style={{ fontSize: '13px', color: 'var(--color-text-secondary)' }}>
            Quản lý mã SKU, đơn giá bán lẻ và giá vốn (COGS) phục vụ tính biên lợi nhuận đóng góp.
          </p>
        </div>
        <Button variant="primary" onClick={loadProducts}>
          Làm mới
        </Button>
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
              </tr>
            </thead>
            <tbody>
              {products.length === 0 ? (
                <tr>
                  <td colSpan={6} style={{ padding: '32px', textAlign: 'center', color: 'var(--color-text-secondary)' }}>
                    Chưa có sản phẩm nào trong hệ thống.
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
                    </tr>
                  ))
                )
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
};
