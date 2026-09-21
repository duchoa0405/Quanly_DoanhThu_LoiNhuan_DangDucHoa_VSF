using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Api.Contracts.Settlements;

public record SettlementLedgerItemResponse(
    Guid Id,
    Guid OrderId,
    string ExternalOrderId,
    ChannelType Channel,
    decimal GrossRevenue,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal FixedFee,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    decimal? ActualSettlement,
    decimal? VarianceAmount,
    ReconciliationStatus ReconciliationStatus,
    DateTime DeliveredAt,
    DateTime? ReconciledAt
);

public record PagedSettlementLedgerResponse(
    List<SettlementLedgerItemResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);

public record SettlementSummaryResponse(
    int PendingSettlementCount,
    int ReconciledCount,
    int DiscrepancyCount
);

public record ReconcileSettlementRequest(
    decimal ActualSettlement,
    Guid? OrderId = null,
    string? Notes = null,
    DiscrepancyType? DiscrepancyType = null
);

public record ReconciliationResponse(
    Guid Id,
    Guid OrderId,
    decimal ProjectedSettlement,
    decimal? ActualSettlement,
    decimal? VarianceAmount,
    ReconciliationStatus ReconciliationStatus,
    string? ReconciliationNotes,
    DateTime? ReconciledAt,
    string? ReconciledBy
);
