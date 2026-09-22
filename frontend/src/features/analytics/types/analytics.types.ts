import { SalesChannel } from '../../orders/types/order.types';

export interface FinancialKpiResponse {
  grossRevenue: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  cogs: number;
  contributionProfit: number;
  contributionMarginPct: number;
  deliveredOrderCount: number;
}

export interface FinancialTrendPoint {
  date: string;
  grossRevenue: number;
  contributionProfit: number;
}

export interface FinancialTrendResponse {
  points: FinancialTrendPoint[];
}

export interface ChannelBreakdownItem {
  channel: SalesChannel;
  deliveredOrders: number;
  grossRevenue: number;
  totalPlatformFees: number;
  contributionProfit: number;
  contributionMarginPct: number;
}

export interface ChannelBreakdownListResponse {
  channels: ChannelBreakdownItem[];
}

export interface TopSkuItem {
  skuCode: string;
  productName: string;
  deliveredUnits: number;
  grossRevenue: number;
  cogs: number;
  contributionProfit: number;
  contributionMarginPct: number;
}

export interface TopSkuListResponse {
  items: TopSkuItem[];
}

export interface DrilldownOrderItem {
  id: string;
  externalOrderId: string;
  channel: SalesChannel;
  deliveredAt: string;
  grossRevenue: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  cogs: number;
  contributionProfit: number;
}

export interface PagedDrilldownOrderResponse {
  items: DrilldownOrderItem[];
  page: number;
  pageSize: number;
  totalItems: number;
  totalPages: number;
}
