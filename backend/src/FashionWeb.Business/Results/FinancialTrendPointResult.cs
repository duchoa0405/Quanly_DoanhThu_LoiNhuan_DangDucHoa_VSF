namespace FashionWeb.Business.Results;

public record FinancialTrendPointResult(
    DateOnly Date,
    decimal GrossRevenue,
    decimal ContributionProfit
);
