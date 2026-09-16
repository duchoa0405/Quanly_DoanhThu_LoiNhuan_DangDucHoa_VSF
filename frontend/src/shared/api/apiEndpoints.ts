export const API_ENDPOINTS = {
  ORDERS: {
    BASE: '/api/v1/orders',
    PREVIEW_FEE: '/api/v1/orders/preview-fee',
    UPDATE_STATUS: (id: string) => `/api/v1/orders/${id}/status`,
    CANCEL: (id: string) => `/api/v1/orders/${id}/cancel`,
  },
  SETTLEMENT: {
    LEDGER: '/api/v1/settlement/ledger',
    IMPORT: '/api/v1/settlement/statements/import',
  },
  DISCREPANCIES: {
    BASE: '/api/v1/discrepancies',
    AUDIT: '/api/v1/discrepancies/audit',
    APPROVE: (id: string) => `/api/v1/discrepancies/${id}/approve`,
  },
  ANALYTICS: {
    KPIS: '/api/v1/analytics/kpis',
    TREND: '/api/v1/analytics/trend',
    CHANNEL_SHARE: '/api/v1/analytics/channel-share',
    TOP_SKUS: '/api/v1/analytics/top-skus',
    EXPORT_CSV: '/api/v1/analytics/export-csv',
  },
};
