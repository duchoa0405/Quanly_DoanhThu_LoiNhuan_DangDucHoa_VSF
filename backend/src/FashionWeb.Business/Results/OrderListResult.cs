using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record OrderItemSummaryResult(
    string SkuCode,
    int Quantity
);

public record OrderListItemResult(
    Guid Id,
    string ExternalOrderId,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    OrderStatus Status,
    DateTime OrderDate,
    string? CustomerName,
    string? CustomerPhone,
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    int ItemCount,
    List<OrderItemSummaryResult> ItemsSummary,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    DateTime CreatedAt
);

public record OrderListResult(
    List<OrderListItemResult> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
