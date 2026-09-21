using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface IDiscrepancyService
{
    Task<PagedResult<DiscrepancyAudit>> ListDiscrepanciesAsync(DiscrepancyQueryFilter filter, CancellationToken ct = default);
    Task<DiscrepancyDetailResult?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DiscrepancyAudit> ResolveDiscrepancyAsync(ResolveDiscrepancyCommand command, CancellationToken ct = default);
}
