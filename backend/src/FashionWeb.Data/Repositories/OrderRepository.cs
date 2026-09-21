using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Results;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _context;

    public OrderRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetOrderDetailByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Include(o => o.FeeSnapshot)
            .Include(o => o.ReconciliationRecord)
            .FirstOrDefaultAsync(o => o.Id == id, ct);
    }

    public async Task<Order?> GetByExternalIdAsync(ChannelType channel, string externalOrderId, CancellationToken ct = default)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Channel == channel && o.ExternalOrderId == externalOrderId, ct);
    }

    public async Task<bool> ExistsExternalOrderIdAsync(ChannelType channel, string externalOrderId, CancellationToken ct = default)
    {
        return await _context.Orders
            .AnyAsync(o => o.Channel == channel && o.ExternalOrderId == externalOrderId, ct);
    }

    public async Task<PagedResult<Order>> ListAsync(OrderQueryFilter filter, CancellationToken ct = default)
    {
        var query = _context.Orders
            .Include(o => o.Items)
            .AsNoTracking();

        if (filter.Channel.HasValue)
            query = query.Where(o => o.Channel == filter.Channel.Value);

        if (filter.Status.HasValue)
            query = query.Where(o => o.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(o => o.OrderDate >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(o => o.OrderDate <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = filter.Search.Trim().ToLower();
            query = query.Where(o => o.ExternalOrderId.ToLower().Contains(pattern) ||
                                     (o.CustomerName != null && o.CustomerName.ToLower().Contains(pattern)) ||
                                     (o.CustomerPhone != null && o.CustomerPhone.Contains(pattern)) ||
                                     o.Items.Any(i => i.SkuCodeSnapshot.ToLower().Contains(pattern)));
        }

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.OrderDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<Order>(items, totalItems, filter.Page, filter.PageSize);
    }

    public async Task<OrderSummaryResult> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, ChannelType? channel, CancellationToken ct = default)
    {
        var query = _context.Orders.AsNoTracking();

        if (channel.HasValue)
            query = query.Where(o => o.Channel == channel.Value);

        if (fromDate.HasValue)
            query = query.Where(o => o.OrderDate >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(o => o.OrderDate <= toDate.Value);

        var totalOrders = await query.CountAsync(ct);
        var pendingOrders = await query.CountAsync(o => o.Status == OrderStatus.PENDING, ct);
        var shippedOrders = await query.CountAsync(o => o.Status == OrderStatus.SHIPPED, ct);
        var deliveredOrders = await query.CountAsync(o => o.Status == OrderStatus.DELIVERED, ct);
        var cancelledOrders = await query.CountAsync(o => o.Status == OrderStatus.CANCELLED, ct);

        // Anti-Phantom Revenue Invariant: Recognized gross revenue is strictly for DELIVERED orders
        var recognizedGrossRevenue = await query
            .Where(o => o.Status == OrderStatus.DELIVERED)
            .SumAsync(o => (decimal?)o.GrossRevenue, ct) ?? 0.00m;

        return new OrderSummaryResult(
            TotalOrders: totalOrders,
            PendingOrders: pendingOrders,
            ShippedOrders: shippedOrders,
            DeliveredOrders: deliveredOrders,
            CancelledOrders: cancelledOrders,
            RecognizedGrossRevenue: recognizedGrossRevenue
        );
    }

    public async Task AddAsync(Order order, CancellationToken ct = default)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct = default)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddStatusHistoryAsync(OrderStatusHistory history, CancellationToken ct = default)
    {
        await _context.OrderStatusHistories.AddAsync(history, ct);
        await _context.SaveChangesAsync(ct);
    }

    public async Task AddFeeSnapshotAsync(OrderFeeSnapshot snapshot, CancellationToken ct = default)
    {
        await _context.OrderFeeSnapshots.AddAsync(snapshot, ct);
        await _context.SaveChangesAsync(ct);
    }
}
