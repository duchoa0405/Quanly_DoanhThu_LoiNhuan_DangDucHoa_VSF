import { httpClient } from '../../../shared/api/httpClient';
import {
  CreateProductRequest,
  PagedProductListResponse,
  Product,
  ProductVariant,
  SelectableVariantListResponse,
  SelectableVariantResponse,
  UpdateProductRequest,
  UpdateVariantRequest,
} from '../types/catalog.types';

export const catalogApi = {
  getProducts: async (params?: { search?: string; category?: string; page?: number; pageSize?: number }): Promise<PagedProductListResponse> => {
    const res = await httpClient.get<PagedProductListResponse>('/catalog/products', { params });
    return res.data;
  },

  getProductById: async (id: string): Promise<Product> => {
    const res = await httpClient.get<Product>(`/catalog/products/${id}`);
    return res.data;
  },

  createProduct: async (payload: CreateProductRequest): Promise<Product> => {
    const res = await httpClient.post<Product>('/catalog/products', payload);
    return res.data;
  },

  updateProduct: async (id: string, payload: UpdateProductRequest): Promise<Product> => {
    const res = await httpClient.put<Product>(`/catalog/products/${id}`, payload);
    return res.data;
  },

  updateVariant: async (variantId: string, payload: UpdateVariantRequest): Promise<ProductVariant> => {
    const res = await httpClient.patch<ProductVariant>(`/catalog/variants/${variantId}`, payload);
    return res.data;
  },

  getSelectableVariants: async (params?: { search?: string }): Promise<SelectableVariantResponse[]> => {
    const res = await httpClient.get<SelectableVariantListResponse>('/catalog/variants/selectable', { params });
    return res.data.items || [];
  },
};
