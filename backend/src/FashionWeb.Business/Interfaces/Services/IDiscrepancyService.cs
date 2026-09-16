namespace FashionWeb.Business.Interfaces.Services;

public interface IDiscrepancyService
{
    Task<object> GetDiscrepanciesAsync(string? status, int page, int pageSize);
    Task<object> CreateAuditAsync(object request);
    Task<object> ApproveAuditAsync(Guid id, string approverId, string resolutionNotes);
}
