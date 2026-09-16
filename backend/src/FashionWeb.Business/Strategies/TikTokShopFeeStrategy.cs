using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class TikTokShopFeeStrategy : IPlatformFeeStrategy
{
    public string ChannelCode => "TIKTOK";

    public FeeBreakdown CalculateFees(decimal subtotal, decimal shopVoucher)
    {
        var netCustomerPayment = Math.Max(0, subtotal - shopVoucher);
        var commissionFee = Math.Round(netCustomerPayment * 0.04m, 0);
        var paymentFee = Math.Round(netCustomerPayment * 0.03m, 0);
        var fixedFee = 2000m;
        var serviceFee = 0m;

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
