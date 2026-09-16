namespace FashionWeb.Business.Domain.ValueObjects;

public record FeeBreakdown(
    decimal Subtotal,
    decimal ShopVoucher,
    decimal NetCustomerPayment,
    decimal CommissionFee,
    decimal PaymentFee,
    decimal FixedFee,
    decimal ServiceFee,
    decimal TotalFees,
    decimal ExpectedNetPayout
);
