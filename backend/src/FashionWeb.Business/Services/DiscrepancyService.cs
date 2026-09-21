using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class DiscrepancyService : IDiscrepancyService
{
    private readonly IDiscrepancyRepository _discrepancyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public DiscrepancyService(
        IDiscrepancyRepository discrepancyRepository,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _discrepancyRepository = discrepancyRepository ?? throw new ArgumentNullException(nameof(discrepancyRepository));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<PagedResult<DiscrepancyAudit>> ListDiscrepanciesAsync(DiscrepancyQueryFilter filter, CancellationToken ct = default)
    {
        return await _discrepancyRepository.ListAsync(filter, ct);
    }

    public async Task<DiscrepancyDetailResult?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _discrepancyRepository.GetDetailByIdAsync(id, ct);
    }

    public async Task<DiscrepancyAudit> ResolveDiscrepancyAsync(ResolveDiscrepancyCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.ResolutionNotes))
            throw new ValidationException("Resolution notes must be provided.");

        var audit = await _discrepancyRepository.GetByIdAsync(command.DiscrepancyId, ct);
        if (audit == null)
            throw new NotFoundException($"Discrepancy audit with ID '{command.DiscrepancyId}' was not found.");

        if (audit.IsResolved)
            throw new BusinessRuleException($"Discrepancy audit '{command.DiscrepancyId}' has already been resolved.");

        var now = _timeProvider.GetUtcNow().UtcDateTime;
        audit.Resolve(command.ResolutionNotes.Trim(), command.ActorIdentity, now);

        await _discrepancyRepository.UpdateAsync(audit, ct);

        if (_unitOfWork != null)
        {
            await _unitOfWork.SaveChangesAsync(ct);
        }

        return audit;
    }
}
