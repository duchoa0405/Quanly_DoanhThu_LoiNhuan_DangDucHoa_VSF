namespace FashionWeb.Api.Contracts.Discrepancies;

public record CreateDiscrepancyRequest(
    Guid OrderId,
    string ChannelOrderCode,
    decimal ExpectedPayout,
    decimal ActualPayout,
    decimal VarianceAmount,
    string DiscrepancyType,
    string ExplanationNote
);

public record ApproveDiscrepancyRequest(
    string ApproverId,
    string ResolutionNotes
);
