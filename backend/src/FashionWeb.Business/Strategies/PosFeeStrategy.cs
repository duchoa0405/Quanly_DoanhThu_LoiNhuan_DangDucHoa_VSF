using FashionWeb.Business.Common;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class PosFeeStrategy : IPlatformFeeStrategy
{
    public ChannelType Channel => ChannelType.POS;

    public FeeBreakdown Calculate(decimal subtotal, decimal voucher, FeeSchedule schedule)
    {
        var grossRevenue = MoneyMath.CalculateGrossRevenue(subtotal, voucher);
        var commissionFee = MoneyMath.CalculateRateFee(subtotal, schedule.CommissionRate);
        var paymentFee = MoneyMath.CalculateRateFee(grossRevenue, schedule.PaymentFeeRate);
        var serviceFee = 0.00m;
        var fixedFee = schedule.FixedFeePerOrder;
        var totalFees = MoneyMath.Round(commissionFee + paymentFee + serviceFee + fixedFee);
        var projectedSettlement = MoneyMath.Round(grossRevenue - totalFees);

        return new FeeBreakdown(
            Subtotal: subtotal,
            ShopVoucher: voucher,
            GrossRevenue: grossRevenue,
            CommissionFee: commissionFee,
            PaymentFee: paymentFee,
            ServiceFee: serviceFee,
            FixedFee: fixedFee,
            TotalPlatformFees: totalFees,
            ProjectedSettlement: projectedSettlement,
            AppliedSchedule: schedule
        );
    }
}
