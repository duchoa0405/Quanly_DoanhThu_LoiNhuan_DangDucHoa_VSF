namespace FashionWeb.Business.Results;

public record SettlementSummaryResult(
    int PendingSettlementCount,
    int ReconciledCount,
    int DiscrepancyCount
);
