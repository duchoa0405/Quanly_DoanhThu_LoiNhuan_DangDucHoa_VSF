using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class DiscrepancyAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ReconciliationId { get; set; }
    public DiscrepancyType DiscrepancyType { get; set; }
    public string ExplanationNote { get; set; } = string.Empty;
    public string? ResolutionNotes { get; set; }
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ReconciliationRecord? ReconciliationRecord { get; set; }

    public bool IsResolved => ResolvedAt != null;

    public void Resolve(string resolutionNotes, string actorIdentity)
    {
        if (string.IsNullOrWhiteSpace(resolutionNotes))
            throw new ArgumentException("Resolution notes cannot be empty.", nameof(resolutionNotes));

        ResolutionNotes = resolutionNotes;
        ResolvedBy = actorIdentity;
        ResolvedAt = DateTime.UtcNow;
    }
}
