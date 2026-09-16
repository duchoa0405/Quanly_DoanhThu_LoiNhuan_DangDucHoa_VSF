using FashionWeb.Business.Domain.Entities;

namespace FashionWeb.Business.Interfaces.Services;

public interface IOrderService
{
    Task<object> GetOrdersAsync(string? channel, string? status, int page, int pageSize);
    Task<Order> CreateOrderAsync(object request);
    Task<Order> UpdateStatusAsync(Guid id, string newStatus);
    Task CancelOrderAsync(Guid id, string reason);
}
