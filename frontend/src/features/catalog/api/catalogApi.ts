import { httpClient } from '../../../shared/api/httpClient';
import { PagedProductListResponse, Product } from '../types/catalog.types';

export const catalogApi = {
  getProducts: async (params?: { search?: string; category?: string; page?: number; pageSize?: number }): Promise<PagedProductListResponse> => {
    const res = await httpClient.get<PagedProductListResponse>('/catalog/products', { params });
    return res.data;
  },

  getProductById: async (id: string): Promise<Product> => {
    const res = await httpClient.get<Product>(`/catalog/products/${id}`);
    return res.data;
  },
};
