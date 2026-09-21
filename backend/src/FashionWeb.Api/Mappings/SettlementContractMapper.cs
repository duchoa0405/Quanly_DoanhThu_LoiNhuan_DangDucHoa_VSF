using FashionWeb.Api.Contracts.Settlements;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Results;

namespace FashionWeb.Api.Mappings;

public static class SettlementContractMapper
{
    public static SettlementLedgerItemResponse MapToLedgerItemResponse(SettlementLedgerResult x)
    {
        return new SettlementLedgerItemResponse(
            Id: x.Id,
            OrderId: x.OrderId,
            ExternalOrderId: x.ExternalOrderId,
            Channel: x.Channel,
            GrossRevenue: x.GrossRevenue,
            CommissionFee: x.CommissionFee,
            PaymentFee: x.PaymentFee,
            ServiceFee: x.ServiceFee,
            FixedFee: x.FixedFee,
            TotalPlatformFees: x.TotalPlatformFees,
            ProjectedSettlement: x.ProjectedSettlement,
            ActualSettlement: x.ActualSettlement,
            VarianceAmount: x.VarianceAmount,
            ReconciliationStatus: x.Status,
            DeliveredAt: x.DeliveredAt,
            ReconciledAt: x.ReconciledAt
        );
    }

    public static ReconciliationResponse MapToReconciliationResponse(ReconciliationRecord record)
    {
        return new ReconciliationResponse(
            Id: record.Id,
            OrderId: record.OrderId,
            ProjectedSettlement: record.ProjectedSettlement,
            ActualSettlement: record.ActualSettlement ?? 0m,
            VarianceAmount: record.VarianceAmount ?? 0m,
            ReconciliationStatus: record.Status,
            ReconciliationNotes: record.ReconciliationNotes,
            ReconciledAt: record.ReconciledAt ?? DateTime.UtcNow,
            ReconciledBy: record.ReconciledBy ?? "system"
        );
    }
}
