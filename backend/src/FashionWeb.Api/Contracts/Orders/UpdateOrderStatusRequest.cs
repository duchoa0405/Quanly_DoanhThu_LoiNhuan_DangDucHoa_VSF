namespace FashionWeb.Api.Contracts.Orders;

public record UpdateOrderStatusRequest(string NewStatus);
public record CancelOrderRequest(string Reason);
