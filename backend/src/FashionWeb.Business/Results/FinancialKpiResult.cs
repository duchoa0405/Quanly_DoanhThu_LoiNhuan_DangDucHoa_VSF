namespace FashionWeb.Business.Results;

public record FinancialKpiResult(
    decimal GrossRevenue,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    decimal Cogs,
    decimal ContributionProfit,
    decimal ContributionMarginPct,
    int DeliveredOrderCount
);
