using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class DiscrepancyService : IDiscrepancyService
{
    private readonly IDiscrepancyRepository _discrepancyRepo;

    public DiscrepancyService(IDiscrepancyRepository discrepancyRepo)
    {
        _discrepancyRepo = discrepancyRepo;
    }

    public async Task<PagedResult<DiscrepancyAudit>> ListDiscrepanciesAsync(DiscrepancyQueryFilter filter, CancellationToken ct = default)
    {
        return await _discrepancyRepo.ListAsync(filter, ct);
    }

    public async Task<DiscrepancyDetailResult?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _discrepancyRepo.GetDetailByIdAsync(id, ct);
    }

    public async Task<DiscrepancyAudit> ResolveDiscrepancyAsync(ResolveDiscrepancyCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.ResolutionNotes))
            throw new ArgumentException("Resolution notes must be provided.", nameof(command));

        var audit = await _discrepancyRepo.GetByIdAsync(command.DiscrepancyId, ct);
        if (audit == null)
            throw new KeyNotFoundException($"Discrepancy audit with ID '{command.DiscrepancyId}' was not found.");

        if (audit.IsResolved)
            throw new InvalidOperationException($"Discrepancy audit '{command.DiscrepancyId}' has already been resolved.");

        audit.Resolve(command.ResolutionNotes.Trim(), command.ActorIdentity);
        await _discrepancyRepo.UpdateAsync(audit, ct);
        return audit;
    }
}
