using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface IOrderService
{
    Task<Order> CreateOrderAsync(CreateOrderCommand command, CancellationToken ct = default);
    Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<Order>> ListOrdersAsync(OrderQueryFilter filter, CancellationToken ct = default);
    Task<OrderSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default);
    Task<Order> UpdateOrderStatusAsync(UpdateOrderStatusCommand command, CancellationToken ct = default);
    Task<Order> CancelOrderAsync(CancelOrderCommand command, CancellationToken ct = default);
}
