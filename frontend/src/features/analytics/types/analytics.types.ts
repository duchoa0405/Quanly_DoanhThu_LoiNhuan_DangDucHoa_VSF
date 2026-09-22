export interface FinancialKpiResponse {
  grossRevenue: number;
  totalPlatformFees: number;
  projectedSettlement: number;
  cogs: number;
  contributionProfit: number;
  contributionMarginPct: number;
  deliveredOrderCount: number;
}

export interface ChannelBreakdownItem {
  channel: 'TIKTOK' | 'SHOPEE' | 'POS';
  deliveredOrders: number;
  grossRevenue: number;
  totalPlatformFees: number;
  contributionProfit: number;
  contributionMarginPct: number;
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
