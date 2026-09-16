export const CHANNELS = {
  TIKTOK: { code: 'TIKTOK', label: 'TikTok Shop', color: '#000000', badgeClass: 'badge-tiktok' },
  SHOPEE: { code: 'SHOPEE', label: 'Shopee', color: '#ee4d2d', badgeClass: 'badge-shopee' },
  POS: { code: 'POS', label: 'In-Store POS', color: '#0284c7', badgeClass: 'badge-pos' },
} as const;

export type ChannelCode = keyof typeof CHANNELS;
