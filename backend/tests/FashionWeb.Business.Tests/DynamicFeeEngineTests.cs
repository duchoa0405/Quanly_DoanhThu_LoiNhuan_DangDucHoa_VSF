using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Services;
using FashionWeb.Business.Strategies;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class DynamicFeeEngineTests
{
    private readonly Mock<IFeeScheduleRepository> _mockScheduleRepo;
    private readonly FeeStrategyFactory _strategyFactory;
    private readonly DynamicFeeEngine _engine;

    public DynamicFeeEngineTests()
    {
        _mockScheduleRepo = new Mock<IFeeScheduleRepository>();

        var strategies = new IPlatformFeeStrategy[]
        {
            new TikTokShopFeeStrategy(),
            new ShopeeFeeStrategy(),
            new PosFeeStrategy()
        };

        _strategyFactory = new FeeStrategyFactory(strategies);
        _engine = new DynamicFeeEngine(_mockScheduleRepo.Object, _strategyFactory);
    }

    [Fact]
    public async Task CalculateFeePreview_TikTokShop_CalculatesAccurately()
    {
        // Arrange: Subtotal 500k, Voucher 50k -> Gross Revenue 450k
        var schedule = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.TIKTOK,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate = 0.0400m,
            PaymentFeeRate = 0.0300m,
            ServiceFeeRate = 0.0000m,
            FixedFeePerOrder = 3000m,
            IsActive = true
        };

        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.TIKTOK, PaymentMethod.MARKETPLACE_WALLET, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _engine.CalculateFeePreviewAsync(
            ChannelType.TIKTOK,
            PaymentMethod.MARKETPLACE_WALLET,
            subtotal: 500000m,
            voucher: 50000m
        );

        // Assert:
        // GrossRevenue = 500k - 50k = 450k
        // Commission = 500k * 4% = 20,000 VND
        // Payment = 450k * 3% = 13,500 VND
        // Service = 0 VND
        // Fixed = 3,000 VND
        // TotalPlatformFees = 20k + 13.5k + 3k = 36,500 VND
        // ProjectedSettlement = 450k - 36.5k = 413,500 VND
        Assert.Equal(500000m, result.Subtotal);
        Assert.Equal(500000m - 50000m, result.GrossRevenue);
        Assert.Equal(20000m, result.CommissionFee);
        Assert.Equal(13500m, result.PaymentFee);
        Assert.Equal(0.00m, result.ServiceFee);
        Assert.Equal(3000m, result.FixedFee);
        Assert.Equal(36500m, result.TotalPlatformFees);
        Assert.Equal(413500m, result.ProjectedSettlement);
    }

    [Fact]
    public async Task CalculateFeePreview_Shopee_WithCap_CapsServiceFee()
    {
        // Arrange: Subtotal 2,000,000, Voucher 100,000 -> Gross 1,900,000
        // Service fee 2% of 2M = 40k, but cap is 20k -> should cap at 20k
        var schedule = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.SHOPEE,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate = 0.0450m,
            PaymentFeeRate = 0.0400m,
            ServiceFeeRate = 0.0200m,
            ServiceFeeCap = 20000m,
            FixedFeePerOrder = 0.00m,
            IsActive = true
        };

        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.SHOPEE, PaymentMethod.MARKETPLACE_WALLET, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _engine.CalculateFeePreviewAsync(
            ChannelType.SHOPEE,
            PaymentMethod.MARKETPLACE_WALLET,
            subtotal: 2000000m,
            voucher: 100000m
        );

        // Assert:
        // Commission = 2M * 4.5% = 90,000 VND
        // Payment = 1.9M * 4% = 76,000 VND
        // Service = Min(40,000, 20,000) = 20,000 VND
        // Total Fees = 90k + 76k + 20k = 186,000 VND
        // ProjectedSettlement = 1,900,000 - 186,000 = 1,714,000 VND
        Assert.Equal(90000m, result.CommissionFee);
        Assert.Equal(76000m, result.PaymentFee);
        Assert.Equal(20000m, result.ServiceFee);
        Assert.Equal(186000m, result.TotalPlatformFees);
        Assert.Equal(1714000m, result.ProjectedSettlement);
    }

    [Fact]
    public async Task CalculateFeePreview_POS_Cash_HasZeroFees()
    {
        // Arrange
        var schedule = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.POS,
            PaymentMethod = PaymentMethod.CASH,
            CommissionRate = 0.0000m,
            PaymentFeeRate = 0.0000m,
            ServiceFeeRate = 0.0000m,
            FixedFeePerOrder = 0.00m,
            IsActive = true
        };

        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.POS, PaymentMethod.CASH, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _engine.CalculateFeePreviewAsync(
            ChannelType.POS,
            PaymentMethod.CASH,
            subtotal: 350000m,
            voucher: 0m
        );

        // Assert
        Assert.Equal(0.00m, result.TotalPlatformFees);
        Assert.Equal(350000m, result.ProjectedSettlement);
    }

    [Fact]
    public async Task CalculateFeePreview_POS_CardQr_AppliesOnePercentPaymentFee()
    {
        // Arrange: Subtotal 1,000,000, Voucher 50,000 -> Gross 950,000
        var schedule = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.POS,
            PaymentMethod = PaymentMethod.POS_CARD_QR,
            CommissionRate = 0.0000m,
            PaymentFeeRate = 0.0100m, // 1%
            ServiceFeeRate = 0.0000m,
            FixedFeePerOrder = 0.00m,
            IsActive = true
        };

        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.POS, PaymentMethod.POS_CARD_QR, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        // Act
        var result = await _engine.CalculateFeePreviewAsync(
            ChannelType.POS,
            PaymentMethod.POS_CARD_QR,
            subtotal: 1000000m,
            voucher: 50000m
        );

        // Assert: 950,000 * 1% = 9,500 VND
        Assert.Equal(9500m, result.PaymentFee);
        Assert.Equal(9500m, result.TotalPlatformFees);
        Assert.Equal(940500m, result.ProjectedSettlement);
    }

    [Fact]
    public async Task CalculateFeePreview_VoucherExceedsSubtotal_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _engine.CalculateFeePreviewAsync(ChannelType.TIKTOK, PaymentMethod.MARKETPLACE_WALLET, subtotal: 100000m, voucher: 120000m)
        );
    }

    [Fact]
    public async Task CalculateFeePreview_NoActiveSchedule_ThrowsInvalidOperationException()
    {
        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(It.IsAny<ChannelType>(), It.IsAny<PaymentMethod>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((FeeSchedule?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _engine.CalculateFeePreviewAsync(ChannelType.TIKTOK, PaymentMethod.MARKETPLACE_WALLET, subtotal: 100000m, voucher: 0m)
        );
    }

    [Fact]
    public async Task CalculateAndFreezeFee_DeliveredOrder_CreatesAccurateSnapshot()
    {
        // Arrange
        var schedule = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.TIKTOK,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate = 0.0400m,
            PaymentFeeRate = 0.0300m,
            ServiceFeeRate = 0.0000m,
            FixedFeePerOrder = 3000m,
            IsActive = true
        };

        _mockScheduleRepo
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.TIKTOK, PaymentMethod.MARKETPLACE_WALLET, It.IsAny<CancellationToken>()))
            .ReturnsAsync(schedule);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ExternalOrderId = "TT-12345",
            Channel = ChannelType.TIKTOK,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            Status = OrderStatus.SHIPPED
        };
        order.SetFinancials(subtotal: 500000m, shopVoucher: 50000m);

        // Act
        var snapshot = await _engine.CalculateAndFreezeFeeAsync(order);

        // Assert
        Assert.Equal(order.Id, snapshot.OrderId);
        Assert.Equal(schedule.Id, snapshot.FeeScheduleId);
        Assert.Equal(20000m, snapshot.CommissionFeeAmount);
        Assert.Equal(13500m, snapshot.PaymentFeeAmount);
        Assert.Equal(3000m, snapshot.FixedFeeAmount);
        Assert.Equal(36500m, snapshot.TotalPlatformFees);
        Assert.Equal(413500m, snapshot.ProjectedSettlement);
    }
}
