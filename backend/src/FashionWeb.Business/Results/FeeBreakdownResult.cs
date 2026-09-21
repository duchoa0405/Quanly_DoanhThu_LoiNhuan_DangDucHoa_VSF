namespace FashionWeb.Business.Results;

public record FeeBreakdownResult(
    decimal Subtotal,
    decimal ShopVoucher,
    decimal GrossRevenue,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal ServiceFee,
    decimal FixedFee,
    decimal TotalPlatformFees,
    decimal ProjectedSettlement
);
