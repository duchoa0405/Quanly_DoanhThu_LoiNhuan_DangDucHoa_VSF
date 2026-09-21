using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public class ShopeeFeeStrategy : IPlatformFeeStrategy
{
    public ChannelType Channel => ChannelType.SHOPEE;

    public FeeBreakdown Calculate(decimal subtotal, decimal voucher, FeeSchedule schedule)
    {
        var grossRevenue = subtotal - voucher;
        var commissionFee = Math.Round(subtotal * schedule.CommissionRate, 2, MidpointRounding.AwayFromZero);
        var paymentFee = Math.Round(grossRevenue * schedule.PaymentFeeRate, 2, MidpointRounding.AwayFromZero);
        var rawService = Math.Round(subtotal * schedule.ServiceFeeRate, 2, MidpointRounding.AwayFromZero);
        var serviceFee = schedule.ServiceFeeCap.HasValue ? Math.Min(rawService, schedule.ServiceFeeCap.Value) : rawService;
        var fixedFee = schedule.FixedFeePerOrder;
        var totalFees = commissionFee + paymentFee + serviceFee + fixedFee;
        var projectedSettlement = grossRevenue - totalFees;

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
