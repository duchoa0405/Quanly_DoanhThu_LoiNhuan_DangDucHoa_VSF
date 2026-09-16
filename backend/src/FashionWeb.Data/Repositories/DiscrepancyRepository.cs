using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
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

    public async Task<DiscrepancyAudit?> GetByIdAsync(Guid id) =>
        await _context.DiscrepancyAudits.FirstOrDefaultAsync(d => d.Id == id);

    public async Task<IEnumerable<DiscrepancyAudit>> GetAllAsync(string? status, int page, int pageSize)
    {
        return await _context.DiscrepancyAudits.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
    }

    public async Task AddAsync(DiscrepancyAudit audit) => await _context.DiscrepancyAudits.AddAsync(audit);
    public Task UpdateAsync(DiscrepancyAudit audit)
    {
        _context.DiscrepancyAudits.Update(audit);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync() => await _context.SaveChangesAsync();
}
