using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record DrilldownOrderResult(
    Guid Id,
    string ExternalOrderId,
    ChannelType Channel,
    DateTime DeliveredAt,
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    decimal Cogs,
    decimal ContributionProfit,
    decimal ContributionMarginPct
);

public record DrilldownOrderListResult(
    List<DrilldownOrderResult> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
