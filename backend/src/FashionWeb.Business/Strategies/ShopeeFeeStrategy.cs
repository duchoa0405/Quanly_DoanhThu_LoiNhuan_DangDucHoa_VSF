using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class ShopeeFeeStrategy : IPlatformFeeStrategy
{
    public string ChannelCode => "SHOPEE";

    public FeeBreakdown CalculateFees(decimal subtotal, decimal shopVoucher)
    {
        var netCustomerPayment = Math.Max(0, subtotal - shopVoucher);
        var commissionFee = Math.Round(netCustomerPayment * 0.045m, 0);
        var paymentFee = Math.Round(netCustomerPayment * 0.04m, 0);
        var serviceFee = Math.Min(20000m, Math.Round(netCustomerPayment * 0.02m, 0));
        var fixedFee = 0m;

        var totalFees = commissionFee + paymentFee + fixedFee + serviceFee;
        var expectedPayout = netCustomerPayment - totalFees;

        return new FeeBreakdown(
            Subtotal: subtotal,
            ShopVoucher: shopVoucher,
            NetCustomerPayment: netCustomerPayment,
            CommissionFee: commissionFee,
            PaymentFee: paymentFee,
            FixedFee: fixedFee,
            ServiceFee: serviceFee,
            TotalFees: totalFees,
            ExpectedNetPayout: expectedPayout
        );
    }
}
