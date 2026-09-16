namespace FashionWeb.Api.Contracts.Analytics;

public record ExecutiveKpisResponse(
    decimal GrossDeliveredRevenue,
    decimal TotalPlatformFees,
    decimal NetCashReceived,
    int DeliveredOrdersCount,
    decimal MarginPercentage
);

public record DailyTrendPointResponse(
    string Date,
    decimal GrossRevenue,
    decimal NetCashflow,
    decimal PlatformFees
);
