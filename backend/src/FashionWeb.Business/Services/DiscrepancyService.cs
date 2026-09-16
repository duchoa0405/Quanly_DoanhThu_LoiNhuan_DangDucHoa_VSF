using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;

namespace FashionWeb.Business.Services;

public class DiscrepancyService : IDiscrepancyService
{
    private readonly IDiscrepancyRepository _discrepancyRepo;

    public DiscrepancyService(IDiscrepancyRepository discrepancyRepo)
    {
        _discrepancyRepo = discrepancyRepo;
    }

    public async Task<object> GetDiscrepanciesAsync(string? status, int page, int pageSize)
    {
        var audits = await _discrepancyRepo.GetAllAsync(status, page, pageSize);
        return new { items = audits, page, pageSize };
    }

    public async Task<object> CreateAuditAsync(object request)
    {
        var audit = new DiscrepancyAudit();
        await _discrepancyRepo.AddAsync(audit);
        await _discrepancyRepo.SaveChangesAsync();
        return audit;
    }

    public async Task<object> ApproveAuditAsync(Guid id, string approverId, string resolutionNotes)
    {
        var audit = await _discrepancyRepo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy hồ sơ khiếu nại ID: {id}");

        audit.Status = "Approved";
        audit.ApprovedBy = approverId;
        audit.ResolutionNotes = resolutionNotes;
        audit.ResolvedAt = DateTime.UtcNow;

        await _discrepancyRepo.UpdateAsync(audit);
        await _discrepancyRepo.SaveChangesAsync();
        return audit;
    }
}
