namespace FashionWeb.Api.Contracts.Settlement;

public record StatementImportResponse(
    Guid StatementId,
    string FileName,
    int TotalRows,
    int MatchedRows,
    int DiscrepancyRows,
    decimal TotalSettledAmount
);
