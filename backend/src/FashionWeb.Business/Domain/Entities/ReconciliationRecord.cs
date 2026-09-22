using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class ReconciliationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public decimal ProjectedSettlement { get; set; }
    public decimal? ActualSettlement { get; private set; }
    public decimal? VarianceAmount { get; private set; }
    public ReconciliationStatus Status { get; private set; } = ReconciliationStatus.PENDING_SETTLEMENT;
    public string? ReconciliationNotes { get; private set; }
    public DateTime? ReconciledAt { get; private set; }
    public string? ReconciledBy { get; private set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public Order? Order { get; set; }
    public List<DiscrepancyAudit> Audits { get; set; } = new();

    public DiscrepancyAudit? Reconcile(decimal actualSettlement, string? notes, DiscrepancyType? discrepancyType, string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;

        // 1. Calculate variance first
        var variance = FashionWeb.Business.Common.MoneyMath.Round(ProjectedSettlement - actualSettlement);

        // 2. Validate first before mutating any in-memory state
        if (variance != 0m)
        {
            if (string.IsNullOrWhiteSpace(notes))
                throw new Exceptions.ValidationException("Reconciliation notes are mandatory when variance is detected.");
            if (!discrepancyType.HasValue)
                throw new Exceptions.ValidationException("Discrepancy type classification is mandatory when variance is detected.");
        }

        // 3. Mutate aggregate state atomically only after validations succeed
        ActualSettlement = actualSettlement;
        VarianceAmount = variance;
        ReconciledAt = timestamp;
        ReconciledBy = actorIdentity;
        ReconciliationNotes = notes;
        UpdatedAt = timestamp;

        if (variance == 0m)
        {
            Status = ReconciliationStatus.RECONCILED;
            return null;
        }
        else
        {
            Status = ReconciliationStatus.DISCREPANCY;

            var audit = new DiscrepancyAudit
            {
                ReconciliationId = Id,
                DiscrepancyType = discrepancyType!.Value,
                ExplanationNote = notes!,
                CreatedAt = timestamp
            };

            Audits.Add(audit);
            return audit;
        }
    }
}
