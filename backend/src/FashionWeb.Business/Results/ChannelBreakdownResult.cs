using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record ChannelBreakdownResult(
    ChannelType Channel,
    int DeliveredOrders,
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ContributionProfit,
    decimal ContributionMarginPct
);
