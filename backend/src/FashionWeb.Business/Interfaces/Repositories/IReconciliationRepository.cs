using FashionWeb.Business.Domain.Entities;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IReconciliationRepository
{
    Task<IEnumerable<ReconciliationRecord>> GetAllAsync(string? channel, string? status, int page, int pageSize);
    Task AddAsync(ReconciliationRecord record);
    Task AddStatementImportAsync(StatementImport import);
    Task SaveChangesAsync();
}
