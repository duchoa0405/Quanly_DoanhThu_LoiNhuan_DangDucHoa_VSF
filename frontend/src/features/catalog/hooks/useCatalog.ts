import { useState, useCallback } from 'react';
import { catalogApi } from '../api/catalogApi';
import {
  CreateProductRequest,
  Product,
  SelectableVariantResponse,
  UpdateProductRequest,
  UpdateVariantRequest,
} from '../types/catalog.types';

export const useCatalog = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [selectableVariants, setSelectableVariants] = useState<SelectableVariantResponse[]>([]);
  const [pageInfo, setPageInfo] = useState<{ page: number; pageSize: number; totalItems: number; totalPages: number }>({
    page: 1,
    pageSize: 20,
    totalItems: 0,
    totalPages: 0,
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchProducts = useCallback(async (params?: { search?: string; category?: string; page?: number; pageSize?: number }) => {
    setLoading(true);
    setError(null);
    try {
      const res = await catalogApi.getProducts(params);
      setProducts(res.items || []);
      setPageInfo({
        page: res.page,
        pageSize: res.pageSize,
        totalItems: res.totalItems,
        totalPages: res.totalPages,
      });
    } catch (err: unknown) {
      console.error('Failed to load products:', err);
      setError('Không thể kết nối đến máy chủ danh mục.');
    } finally {
      setLoading(false);
    }
  }, []);

  const fetchSelectableVariants = useCallback(async (search?: string) => {
    try {
      const items = await catalogApi.getSelectableVariants({ search });
      setSelectableVariants(items);
      return items;
    } catch (err: unknown) {
      console.error('Failed to load selectable variants:', err);
      return [];
    }
  }, []);

  const createProduct = useCallback(async (payload: CreateProductRequest) => {
    const created = await catalogApi.createProduct(payload);
    await fetchProducts();
    return created;
  }, [fetchProducts]);

  const updateProduct = useCallback(async (id: string, payload: UpdateProductRequest) => {
    const updated = await catalogApi.updateProduct(id, payload);
    await fetchProducts();
    return updated;
  }, [fetchProducts]);

  const updateVariant = useCallback(async (variantId: string, payload: UpdateVariantRequest) => {
    const updated = await catalogApi.updateVariant(variantId, payload);
    await fetchProducts();
    return updated;
  }, [fetchProducts]);

  return {
    products,
    selectableVariants,
    pageInfo,
    loading,
    error,
    fetchProducts,
    fetchSelectableVariants,
    createProduct,
    updateProduct,
    updateVariant,
  };
};
