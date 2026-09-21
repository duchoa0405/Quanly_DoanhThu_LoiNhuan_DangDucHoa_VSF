using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Api.Contracts.Discrepancies;

public record ResolveDiscrepancyRequest(
    string ResolutionNotes
);

public record DiscrepancyResponse(
    Guid Id,
    Guid ReconciliationId,
    Guid OrderId,
    string ExternalOrderId,
    ChannelType Channel,
    DiscrepancyType DiscrepancyType,
    string ExplanationNote,
    decimal VarianceAmount,
    bool IsResolved,
    DateTime CreatedAt
);

public record DiscrepancyDetailResponse(
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

public record PagedDiscrepancyListResponse(
    List<DiscrepancyResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
