using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Results;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class ReconciliationRepository : IReconciliationRepository
{
    private readonly AppDbContext _context;

    public ReconciliationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ReconciliationRecord?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default)
    {
        return await _context.ReconciliationRecords
            .Include(r => r.Order)
            .Include(r => r.Audits)
            .FirstOrDefaultAsync(r => r.OrderId == orderId, ct);
    }

    public async Task<ReconciliationRecord?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.ReconciliationRecords
            .Include(r => r.Order)
            .Include(r => r.Audits)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<PagedResult<SettlementLedgerResult>> ListLedgerAsync(SettlementQueryFilter filter, CancellationToken ct = default)
    {
        var query = from r in _context.ReconciliationRecords
                    join o in _context.Orders on r.OrderId equals o.Id
                    join fs in _context.OrderFeeSnapshots on o.Id equals fs.OrderId into feeSnapshots
                    from fs in feeSnapshots.DefaultIfEmpty()
                    select new { r, o, fs };

        if (filter.Channel.HasValue)
            query = query.Where(x => x.o.Channel == filter.Channel.Value);

        if (filter.Status.HasValue)
            query = query.Where(x => x.r.Status == filter.Status.Value);

        if (filter.FromDate.HasValue)
            query = query.Where(x => (x.o.DeliveredAt ?? x.o.OrderDate) >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(x => (x.o.DeliveredAt ?? x.o.OrderDate) <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = filter.Search.Trim().ToLower();
            query = query.Where(x => x.o.ExternalOrderId.ToLower().Contains(pattern) ||
                                     (x.r.ReconciliationNotes != null && x.r.ReconciliationNotes.ToLower().Contains(pattern)));
        }

        var totalItems = await query.CountAsync(ct);

        var rawList = await query
            .OrderByDescending(x => x.o.DeliveredAt ?? x.o.OrderDate)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        var items = rawList.Select(x => new SettlementLedgerResult(
            Id: x.r.Id,
            OrderId: x.o.Id,
            ExternalOrderId: x.o.ExternalOrderId,
            Channel: x.o.Channel,
            GrossRevenue: x.o.GrossRevenue,
            CommissionFee: x.fs != null ? x.fs.CommissionFeeAmount : 0.00m,
            PaymentFee: x.fs != null ? x.fs.PaymentFeeAmount : 0.00m,
            ServiceFee: x.fs != null ? x.fs.ServiceFeeAmount : 0.00m,
            FixedFee: x.fs != null ? x.fs.FixedFeeAmount : 0.00m,
            TotalPlatformFees: x.fs != null ? x.fs.TotalPlatformFees : 0.00m,
            ProjectedSettlement: x.r.ProjectedSettlement,
            ActualSettlement: x.r.ActualSettlement,
            VarianceAmount: x.r.VarianceAmount,
            Status: x.r.Status,
            DeliveredAt: x.o.DeliveredAt ?? x.o.OrderDate,
            ReconciledAt: x.r.ReconciledAt
        )).ToList();

        return new PagedResult<SettlementLedgerResult>(items, totalItems, filter.Page, filter.PageSize);
    }

    public async Task<SettlementSummaryResult> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, ChannelType? channel, CancellationToken ct = default)
    {
        var query = from r in _context.ReconciliationRecords
                    join o in _context.Orders on r.OrderId equals o.Id
                    select new { r, o };

        if (channel.HasValue)
            query = query.Where(x => x.o.Channel == channel.Value);

        if (fromDate.HasValue)
            query = query.Where(x => (x.o.DeliveredAt ?? x.o.OrderDate) >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(x => (x.o.DeliveredAt ?? x.o.OrderDate) <= toDate.Value);

        var pendingCount = await query.CountAsync(x => x.r.Status == ReconciliationStatus.PENDING_SETTLEMENT, ct);
        var reconciledCount = await query.CountAsync(x => x.r.Status == ReconciliationStatus.RECONCILED, ct);
        var discrepancyCount = await query.CountAsync(x => x.r.Status == ReconciliationStatus.DISCREPANCY, ct);

        return new SettlementSummaryResult(
            PendingSettlementCount: pendingCount,
            ReconciledCount: reconciledCount,
            DiscrepancyCount: discrepancyCount
        );
    }

    public async Task AddAsync(ReconciliationRecord record, CancellationToken ct = default)
    {
        await _context.ReconciliationRecords.AddAsync(record, ct);
    }

    public Task UpdateAsync(ReconciliationRecord record, CancellationToken ct = default)
    {
        _context.ReconciliationRecords.Update(record);
        return Task.CompletedTask;
    }
}
