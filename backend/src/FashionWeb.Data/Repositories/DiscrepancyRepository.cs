using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Results;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class DiscrepancyRepository : IDiscrepancyRepository
{
    private readonly AppDbContext _context;

    public DiscrepancyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<DiscrepancyAudit?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.DiscrepancyAudits
            .Include(d => d.ReconciliationRecord)
            .ThenInclude(r => r!.Order)
            .FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task<DiscrepancyDetailResult?> GetDetailByIdAsync(Guid id, CancellationToken ct = default)
    {
        var audit = await _context.DiscrepancyAudits
            .Include(d => d.ReconciliationRecord)
            .ThenInclude(r => r!.Order)
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == id, ct);

        if (audit == null || audit.ReconciliationRecord?.Order == null)
            return null;

        var recon = audit.ReconciliationRecord;
        var order = recon.Order!;

        return new DiscrepancyDetailResult(
            Id: audit.Id,
            ReconciliationId: audit.ReconciliationId,
            OrderId: order.Id,
            ExternalOrderId: order.ExternalOrderId,
            Channel: order.Channel,
            DiscrepancyType: audit.DiscrepancyType,
            ExplanationNote: audit.ExplanationNote,
            VarianceAmount: recon.VarianceAmount ?? 0.00m,
            IsResolved: audit.IsResolved,
            CreatedAt: audit.CreatedAt,
            ResolvedAt: audit.ResolvedAt,
            ResolvedBy: audit.ResolvedBy,
            ResolutionNotes: audit.ResolutionNotes
        );
    }

    public async Task<PagedResult<DiscrepancyAudit>> ListAsync(DiscrepancyQueryFilter filter, CancellationToken ct = default)
    {
        var query = _context.DiscrepancyAudits
            .Include(d => d.ReconciliationRecord)
            .ThenInclude(r => r!.Order)
            .AsNoTracking();

        if (filter.Channel.HasValue)
            query = query.Where(d => d.ReconciliationRecord != null &&
                                     d.ReconciliationRecord.Order != null &&
                                     d.ReconciliationRecord.Order.Channel == filter.Channel.Value);

        if (filter.DiscrepancyType.HasValue)
            query = query.Where(d => d.DiscrepancyType == filter.DiscrepancyType.Value);

        if (filter.IsResolved.HasValue)
        {
            query = filter.IsResolved.Value
                ? query.Where(d => d.ResolvedAt != null)
                : query.Where(d => d.ResolvedAt == null);
        }

        if (filter.FromDate.HasValue)
            query = query.Where(d => d.CreatedAt >= filter.FromDate.Value);

        if (filter.ToDate.HasValue)
            query = query.Where(d => d.CreatedAt <= filter.ToDate.Value);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var pattern = filter.Search.Trim().ToLower();
            query = query.Where(d => d.ExplanationNote.ToLower().Contains(pattern) ||
                                     (d.ResolutionNotes != null && d.ResolutionNotes.ToLower().Contains(pattern)) ||
                                     (d.ReconciliationRecord != null &&
                                      d.ReconciliationRecord.Order != null &&
                                      d.ReconciliationRecord.Order.ExternalOrderId.ToLower().Contains(pattern)));
        }

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync(ct);

        return new PagedResult<DiscrepancyAudit>(items, totalItems, filter.Page, filter.PageSize);
    }

    public async Task AddAsync(DiscrepancyAudit audit, CancellationToken ct = default)
    {
        await _context.DiscrepancyAudits.AddAsync(audit, ct);
    }

    public Task UpdateAsync(DiscrepancyAudit audit, CancellationToken ct = default)
    {
        _context.DiscrepancyAudits.Update(audit);
        return Task.CompletedTask;
    }
}
