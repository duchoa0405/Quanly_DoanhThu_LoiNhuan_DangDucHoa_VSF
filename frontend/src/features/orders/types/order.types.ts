export interface Order {
  id: string;
  channelOrderCode: string;
  channel: 'TikTok' | 'Shopee' | 'POS';
  customerName: string;
  status: 'Pending' | 'Shipped' | 'Delivered' | 'Cancelled';
  subtotalAmount: number;
  shopVoucherDiscount: number;
  netCustomerPayment: number;
  orderDate: string;
  feeSnapshot?: {
    totalPlatformFees: number;
    expectedNetPayout: number;
  };
}
