using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Results;

public record SettlementLedgerResult(
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
    ReconciliationStatus Status,
    DateTime DeliveredAt,
    DateTime? ReconciledAt
);

public record SettlementLedgerListResult(
    List<SettlementLedgerResult> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
