namespace FashionWeb.Business.Results;

public record TopSkuResult(
    string SkuCode,
    string ProductName,
    int DeliveredUnits,
    decimal GrossRevenue,
    decimal Cogs,
    decimal ContributionProfit,
    decimal ContributionMarginPct
);
