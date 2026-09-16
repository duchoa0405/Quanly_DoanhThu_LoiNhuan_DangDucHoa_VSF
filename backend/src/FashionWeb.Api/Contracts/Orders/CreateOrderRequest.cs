namespace FashionWeb.Api.Contracts.Orders;

public record CreateOrderRequest(
    string ChannelOrderCode,
    string ChannelCode,
    string CustomerName,
    string CustomerPhone,
    string ShippingAddress,
    string PaymentMethod,
    List<CreateOrderItemRequest> Items,
    decimal ShopVoucher,
    decimal ShippingFeeActual
);

public record CreateOrderItemRequest(
    string Sku,
    string ProductName,
    int Quantity,
    decimal UnitPrice
);
