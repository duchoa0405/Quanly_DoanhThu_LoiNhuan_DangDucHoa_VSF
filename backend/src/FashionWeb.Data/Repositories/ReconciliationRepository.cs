using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
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

    public async Task<IEnumerable<ReconciliationRecord>> GetAllAsync(string? channel, string? status, int page, int pageSize)
    {
        return await _context.ReconciliationRecords.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task AddAsync(ReconciliationRecord record) => await _context.ReconciliationRecords.AddAsync(record);
    public async Task AddStatementImportAsync(StatementImport import) => await _context.StatementImports.AddAsync(import);
    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
