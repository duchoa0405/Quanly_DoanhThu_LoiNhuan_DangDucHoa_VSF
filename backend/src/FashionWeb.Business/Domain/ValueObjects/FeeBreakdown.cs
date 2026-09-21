using FashionWeb.Business.Domain.Entities;

namespace FashionWeb.Business.Domain.ValueObjects;

public record FeeBreakdown(
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal FixedFee,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement,
    FeeSchedule? AppliedSchedule = null
);
