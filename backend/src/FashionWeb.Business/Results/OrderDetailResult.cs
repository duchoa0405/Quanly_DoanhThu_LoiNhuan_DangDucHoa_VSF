using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record OrderItemDetailResult(
    Guid Id,
    Guid ProductVariantId,
    string SkuCodeSnapshot,
    string ProductNameSnapshot,
    int Quantity,
    decimal UnitPrice,
    decimal UnitCostSnapshot,
    decimal LineTotal,
    decimal TotalCost
);

public record OrderStatusHistoryResult(
    Guid Id,
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string? Reason,
    string ChangedBy,
    DateTime ChangedAt
);

public record FeeSnapshotResult(
    Guid Id,
    decimal CommissionRate,
    decimal CommissionFeeAmount,
    decimal PaymentFeeRate,
    decimal PaymentFeeAmount,
    decimal ServiceFeeRate,
    decimal ServiceFeeAmount,
    decimal? ServiceFeeCapSnapshot,
    decimal FixedFeeAmount,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    DateTime SnapshotAt
);

public record OrderDetailResult(
    Guid Id,
    string ExternalOrderId,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    OrderStatus Status,
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    string? CustomerName,
    string? CustomerPhone,
    DateTime OrderDate,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    string? CancellationReason,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    decimal? Cogs,
    decimal? ContributionProfit,
    List<OrderItemDetailResult> Items,
    List<OrderStatusHistoryResult> StatusHistory,
    FeeSnapshotResult? FeeSnapshot
);
