using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record DiscrepancyDetailResult(
    Guid Id,
    Guid ReconciliationId,
    Guid OrderId,
    string ExternalOrderId,
    ChannelType Channel,
    DiscrepancyType DiscrepancyType,
    string ExplanationNote,
    decimal VarianceAmount,
    bool IsResolved,
    DateTime CreatedAt,
    DateTime? ResolvedAt,
    string? ResolvedBy,
    string? ResolutionNotes
);
