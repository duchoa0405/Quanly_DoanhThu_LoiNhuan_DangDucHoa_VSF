namespace FashionWeb.Api.Contracts.Orders;

public record OrderResponse(
    Guid Id,
    string ChannelOrderCode,
    string ChannelCode,
    string CustomerName,
    string Status,
    decimal Subtotal,
    decimal ShopVoucher,
    decimal NetPayable,
    decimal TotalFees,
    decimal NetReceivedActual,
    DateTime CreatedAt
);
