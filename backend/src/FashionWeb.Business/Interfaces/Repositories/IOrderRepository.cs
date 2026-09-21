using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetOrderDetailByIdAsync(Guid id, CancellationToken ct = default);
    Task<Order?> GetByExternalIdAsync(ChannelType channel, string externalOrderId, CancellationToken ct = default);
    Task<bool> ExistsExternalOrderIdAsync(ChannelType channel, string externalOrderId, CancellationToken ct = default);
    Task<PagedResult<Order>> ListAsync(OrderQueryFilter filter, CancellationToken ct = default);
    Task<OrderSummaryResult> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, ChannelType? channel, CancellationToken ct = default);
    Task AddAsync(Order order, CancellationToken ct = default);
    Task UpdateAsync(Order order, CancellationToken ct = default);
    Task AddStatusHistoryAsync(OrderStatusHistory history, CancellationToken ct = default);
    Task AddFeeSnapshotAsync(OrderFeeSnapshot snapshot, CancellationToken ct = default);
}
