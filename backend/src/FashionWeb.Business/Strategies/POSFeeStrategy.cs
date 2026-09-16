using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class POSFeeStrategy : IPlatformFeeStrategy
{
    public string ChannelCode => "POS";

    public FeeBreakdown CalculateFees(decimal subtotal, decimal shopVoucher)
    {
        var netCustomerPayment = Math.Max(0, subtotal - shopVoucher);
        var paymentFee = Math.Round(netCustomerPayment * 0.01m, 0);
        var commissionFee = 0m;
        var fixedFee = 0m;
        var serviceFee = 0m;

        var totalFees = paymentFee;
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
