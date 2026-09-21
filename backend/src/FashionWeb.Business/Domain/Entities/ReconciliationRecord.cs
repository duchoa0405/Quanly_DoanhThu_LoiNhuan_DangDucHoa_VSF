using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class ReconciliationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal ProjectedSettlement { get; set; }
    public decimal? ActualSettlement { get; set; }
    public decimal? VarianceAmount { get; set; }
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.PENDING_SETTLEMENT;
    public string? ReconciliationNotes { get; set; }
    public DateTime? ReconciledAt { get; set; }
    public string? ReconciledBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Order? Order { get; set; }
    public List<DiscrepancyAudit> Audits { get; set; } = new();

    public DiscrepancyAudit? Reconcile(decimal actualSettlement, string? notes, DiscrepancyType? discrepancyType, string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        ActualSettlement = actualSettlement;
        VarianceAmount = FashionWeb.Business.Common.MoneyMath.Round(ProjectedSettlement - actualSettlement);
        ReconciledAt = timestamp;
        ReconciledBy = actorIdentity;
        ReconciliationNotes = notes;
        UpdatedAt = timestamp;

        if (VarianceAmount.Value == 0m)
        {
            Status = ReconciliationStatus.RECONCILED;
            return null;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(notes))
                throw new Exceptions.ValidationException("Reconciliation notes are mandatory when variance is detected.");
            if (!discrepancyType.HasValue)
                throw new Exceptions.ValidationException("Discrepancy type classification is mandatory when variance is detected.");

            Status = ReconciliationStatus.DISCREPANCY;

            var audit = new DiscrepancyAudit
            {
                ReconciliationId = Id,
                DiscrepancyType = discrepancyType.Value,
                ExplanationNote = notes,
                CreatedAt = timestamp
            };

            Audits.Add(audit);
            return audit;
        }
    }
}
