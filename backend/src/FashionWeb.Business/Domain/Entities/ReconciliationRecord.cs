using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class ReconciliationRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid? StatementImportId { get; set; }
    public decimal ExpectedPayout { get; set; }
    public decimal ActualPayout { get; set; }
    public decimal DiscrepancyAmount => ActualPayout - ExpectedPayout;
    public ReconciliationStatus Status { get; set; } = ReconciliationStatus.PendingSettlement;
    public DateTime? ReconciledDate { get; set; }
}
