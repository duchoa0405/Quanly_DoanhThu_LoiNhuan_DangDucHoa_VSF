namespace FashionWeb.Business.Results;

public record OrderSummaryResult(
    int TotalOrders,
    int DeliveredOrders,
    decimal GrossRevenue,
    int InTransitOrders,
    int CancelledOrders
);
