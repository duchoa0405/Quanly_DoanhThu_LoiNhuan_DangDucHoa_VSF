using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record OrderItemDetailResult(
    Guid Id,
    Guid ProductVariantId,
    string SkuCode,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    decimal? UnitCostSnapshot,
    decimal? TotalCost
);

public record OrderStatusHistoryResult(
    Guid Id,
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string ChangedBy,
    DateTime ChangedAt
);

public record FeeSnapshotResult(
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal FixedFee,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    DateTime SnapshottedAt
);

public record OrderDetailResult(
    Guid Id,
    string ExternalOrderId,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    OrderStatus Status,
    string? CustomerName,
    string? CustomerPhone,
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    DateTime OrderDate,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    decimal? Cogs,
    decimal? ContributionProfit,
    List<OrderItemDetailResult> Items,
    List<OrderStatusHistoryResult> StatusHistory,
    FeeSnapshotResult? FeeSnapshot
);
