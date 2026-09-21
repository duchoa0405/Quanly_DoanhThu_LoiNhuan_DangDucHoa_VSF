using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Strategies;

namespace FashionWeb.Business.Services;

public class DynamicFeeEngine : IDynamicFeeEngine
{
    private readonly IFeeScheduleRepository _scheduleRepo;
    private readonly FeeStrategyFactory _strategyFactory;

    public DynamicFeeEngine(IFeeScheduleRepository scheduleRepo, FeeStrategyFactory strategyFactory)
    {
        _scheduleRepo = scheduleRepo;
        _strategyFactory = strategyFactory;
    }

    public async Task<FeeBreakdown> CalculateFeePreviewAsync(
        ChannelType channel,
        PaymentMethod method,
        decimal subtotal,
        decimal voucher,
        CancellationToken ct = default)
    {
        if (subtotal < 0m)
            throw new ArgumentException("Subtotal cannot be negative.", nameof(subtotal));

        if (voucher < 0m)
            throw new ArgumentException("Shop voucher cannot be negative.", nameof(voucher));

        if (voucher > subtotal)
            throw new ArgumentException("Shop voucher discount cannot exceed order subtotal.", nameof(voucher));

        var schedule = await _scheduleRepo.GetActiveScheduleAsync(channel, method, ct);
        if (schedule == null)
            throw new InvalidOperationException($"No active fee schedule configured for channel '{channel}' and payment method '{method}'.");

        var strategy = _strategyFactory.GetStrategy(channel);
        return strategy.Calculate(subtotal, voucher, schedule);
    }

    public async Task<OrderFeeSnapshot> CalculateAndFreezeFeeAsync(Order order, CancellationToken ct = default)
    {
        var schedule = await _scheduleRepo.GetActiveScheduleAsync(order.Channel, order.PaymentMethod, ct);
        if (schedule == null)
            throw new InvalidOperationException($"No active fee schedule configured for channel '{order.Channel}' and payment method '{order.PaymentMethod}'.");

        var strategy = _strategyFactory.GetStrategy(order.Channel);
        var breakdown = strategy.Calculate(order.Subtotal, order.ShopVoucher, schedule);

        return new OrderFeeSnapshot
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FeeScheduleId = schedule.Id,
            CommissionFeeRate = schedule.CommissionRate,
            CommissionFeeAmount = breakdown.CommissionFee,
            PaymentFeeRate = schedule.PaymentFeeRate,
            PaymentFeeAmount = breakdown.PaymentFee,
            ServiceFeeRate = schedule.ServiceFeeRate,
            ServiceFeeAmount = breakdown.ServiceFee,
            ServiceFeeCapSnapshot = schedule.ServiceFeeCap,
            FixedFeeAmount = breakdown.FixedFee,
            TotalPlatformFees = breakdown.TotalPlatformFees,
            ProjectedSettlement = breakdown.ProjectedSettlement,
            SnapshotAt = DateTime.UtcNow
        };
    }
}
