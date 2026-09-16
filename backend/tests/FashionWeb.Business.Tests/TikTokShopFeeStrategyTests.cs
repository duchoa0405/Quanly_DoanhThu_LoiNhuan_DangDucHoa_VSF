using FashionWeb.Business.Strategies;
using Xunit;

namespace FashionWeb.Business.Tests;

public class TikTokShopFeeStrategyTests
{
    [Fact]
    public void CalculateFees_ShouldApply4PercentCommission_3PercentPayment_And2kFixed()
    {
        // Arrange
        var strategy = new TikTokShopFeeStrategy();
        decimal subtotal = 450000m;
        decimal voucher = 50000m;
        decimal netPayment = 400000m; // subtotal - voucher

        // Act
        var result = strategy.CalculateFees(subtotal, voucher);

        // Assert: 4% of 400k = 16k, 3% = 12k, fixed = 2k -> total = 30k, payout = 370k
        Assert.Equal(16000m, result.CommissionFee);
        Assert.Equal(12000m, result.PaymentFee);
        Assert.Equal(2000m, result.FixedFee);
        Assert.Equal(30000m, result.TotalFees);
        Assert.Equal(370000m, result.ExpectedNetPayout);
    }
}
