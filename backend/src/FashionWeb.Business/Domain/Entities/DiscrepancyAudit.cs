using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class DiscrepancyAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReconciliationId { get; set; }
    public DiscrepancyType DiscrepancyType { get; set; }
    public string ExplanationNote { get; set; } = string.Empty;
    public string? ResolutionNotes { get; private set; }
    public string? ResolvedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ReconciliationRecord? ReconciliationRecord { get; set; }

    public bool IsResolved => ResolvedAt != null;

    public void Resolve(string resolutionNotes, string actorIdentity, DateTime? now = null)
    {
        if (string.IsNullOrWhiteSpace(resolutionNotes))
            throw new Exceptions.ValidationException("Resolution notes cannot be empty.");

        ResolutionNotes = resolutionNotes;
        ResolvedBy = actorIdentity;
        ResolvedAt = now ?? DateTime.UtcNow;
    }
}
