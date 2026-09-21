using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Api.Contracts.Orders;

public record CreateOrderItemRequest(
    Guid ProductVariantId,
    int Quantity,
    decimal UnitPrice
);

public record CreateOrderRequest(
    string ExternalOrderId,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    string? CustomerName,
    string? CustomerPhone,
    decimal ShopVoucher,
    List<CreateOrderItemRequest> Items
);

public record FeePreviewRequest(
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    decimal Subtotal,
    decimal ShopVoucher
);

public record FeeBreakdownResponse(
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal FixedFee,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement
);

public record UpdateOrderStatusRequest(
    OrderProgressStatus ToStatus
);

public record CancelOrderRequest(
    string CancellationReason
);

public record OrderItemSummary(
    string SkuCode,
    int Quantity
);

public record OrderListItemResponse(
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
    List<OrderItemSummary> ItemsSummary,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    DateTime CreatedAt
);

public record PagedOrderListResponse(
    List<OrderListItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record OrderItemResponse(
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

public record OrderStatusHistoryResponse(
    Guid Id,
    OrderStatus? FromStatus,
    OrderStatus ToStatus,
    string? Reason,
    string ChangedBy,
    DateTime ChangedAt
);

public record FeeSnapshotResponse(
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

public record OrderResponse(
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
    DateTime? UpdatedAt
);

public record OrderDetailResponse(
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
    List<OrderItemResponse> Items,
    List<OrderStatusHistoryResponse> StatusHistory,
    FeeSnapshotResponse? FeeSnapshot
);

public record OrderSummaryResponse(
    int TotalOrders,
    int PendingOrders,
    int ShippedOrders,
    int DeliveredOrders,
    int CancelledOrders,
    decimal RecognizedGrossRevenue
);
