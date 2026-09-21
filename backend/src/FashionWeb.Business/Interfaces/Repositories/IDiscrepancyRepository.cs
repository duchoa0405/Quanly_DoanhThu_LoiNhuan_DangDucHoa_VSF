using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IDiscrepancyRepository
{
    Task<DiscrepancyAudit?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DiscrepancyDetailResult?> GetDetailByIdAsync(Guid id, CancellationToken ct = default);
    Task<PagedResult<DiscrepancyAudit>> ListAsync(DiscrepancyQueryFilter filter, CancellationToken ct = default);
    Task AddAsync(DiscrepancyAudit audit, CancellationToken ct = default);
    Task UpdateAsync(DiscrepancyAudit audit, CancellationToken ct = default);
}
