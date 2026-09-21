using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Api.Contracts.Analytics;

public record FinancialKpiResponse(
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    decimal Cogs,
    decimal ContributionProfit,
    decimal ContributionMarginPct,
    int DeliveredOrderCount
);

public record FinancialTrendPoint(
    DateOnly Date,
    decimal GrossRevenue,
    decimal ContributionProfit
);

public record FinancialTrendResponse(
    List<FinancialTrendPoint> Points
);

public record ChannelBreakdownResponse(
    ChannelType Channel,
    int DeliveredOrders,
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ContributionProfit,
    decimal ContributionMarginPct
);

public record ChannelBreakdownListResponse(
    List<ChannelBreakdownResponse> Channels
);

public record TopSkuResponse(
    string SkuCode,
    string ProductName,
    int DeliveredUnits,
    decimal GrossRevenue,
    decimal Cogs,
    decimal ContributionProfit,
    decimal ContributionMarginPct
);

public record TopSkuListResponse(
    List<TopSkuResponse> Items
);

public record DrilldownOrderItem(
    Guid Id,
    string ExternalOrderId,
    ChannelType Channel,
    DateTime DeliveredAt,
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    decimal Cogs,
    decimal ContributionProfit
);

public record PagedDrilldownOrderResponse(
    List<DrilldownOrderItem> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
