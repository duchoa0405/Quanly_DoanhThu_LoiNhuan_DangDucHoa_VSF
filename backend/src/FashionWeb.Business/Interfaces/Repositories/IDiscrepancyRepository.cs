using FashionWeb.Business.Domain.Entities;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IDiscrepancyRepository
{
    Task<DiscrepancyAudit?> GetByIdAsync(Guid id);
    Task<IEnumerable<DiscrepancyAudit>> GetAllAsync(string? status, int page, int pageSize);
    Task AddAsync(DiscrepancyAudit audit);
    Task UpdateAsync(DiscrepancyAudit audit);
    Task SaveChangesAsync();
}
