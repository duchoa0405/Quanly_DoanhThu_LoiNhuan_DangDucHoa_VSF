export interface ProductVariant {
  id: string;
  productId: string;
  skuCode: string;
  color?: string | null;
  size?: string | null;
  retailPrice: number;
  costPrice: number;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string | null;
}

export interface Product {
  id: string;
  name: string;
  category?: string | null;
  isActive: boolean;
  variants: ProductVariant[];
  createdAt: string;
  updatedAt?: string | null;
}

export interface PagedProductListResponse {
  items: Product[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
