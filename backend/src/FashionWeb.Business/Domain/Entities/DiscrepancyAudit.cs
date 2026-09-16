using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class DiscrepancyAudit
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public string ChannelOrderCode { get; set; } = string.Empty;
    public decimal DiscrepancyAmount { get; set; }
    public string DiscrepancyType { get; set; } = "WeightPenalty";
    public string ExplanationNote { get; set; } = string.Empty;
    public string Status { get; set; } = "UnderReview";
    public string? ResolutionNotes { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAt { get; set; }
}
