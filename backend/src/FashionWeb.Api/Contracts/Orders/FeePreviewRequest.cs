namespace FashionWeb.Api.Contracts.Orders;

public record FeePreviewRequest(string ChannelCode, decimal Subtotal, decimal ShopVoucher);
