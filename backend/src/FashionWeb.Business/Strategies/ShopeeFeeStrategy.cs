using FashionWeb.Business.Common;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class ShopeeFeeStrategy : IPlatformFeeStrategy
{
    public ChannelType Channel => ChannelType.SHOPEE;

    public FeeBreakdown Calculate(decimal subtotal, decimal voucher, FeeSchedule schedule)
    {
        var grossRevenue = MoneyMath.CalculateGrossRevenue(subtotal, voucher);
        var commissionFee = MoneyMath.CalculateRateFee(subtotal, schedule.CommissionRate);
        var paymentFee = MoneyMath.CalculateRateFee(grossRevenue, schedule.PaymentFeeRate);
        var rawService = MoneyMath.CalculateRateFee(subtotal, schedule.ServiceFeeRate);
        var serviceFee = schedule.ServiceFeeCap.HasValue ? Math.Min(rawService, schedule.ServiceFeeCap.Value) : rawService;
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
