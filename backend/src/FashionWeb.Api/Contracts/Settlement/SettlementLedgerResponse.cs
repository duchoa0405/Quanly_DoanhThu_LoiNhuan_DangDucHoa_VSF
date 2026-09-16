namespace FashionWeb.Api.Contracts.Settlement;

public record SettlementLedgerResponse(
    Guid OrderId,
    string ChannelOrderCode,
    string ChannelCode,
    decimal CustomerPaid,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal ExpectedPayout,
    decimal ActualWalletPayout,
    decimal DiscrepancyAmount,
    string ReconciliationStatus
);
