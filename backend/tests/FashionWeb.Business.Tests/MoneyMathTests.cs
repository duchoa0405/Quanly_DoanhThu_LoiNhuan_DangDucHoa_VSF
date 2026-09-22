using FashionWeb.Business.Common;
using FashionWeb.Business.Exceptions;
using Xunit;

namespace FashionWeb.Business.Tests;

public class MoneyMathTests
{
    [Theory]
    [InlineData(100.004, 100.00)]
    [InlineData(100.005, 100.01)]
    [InlineData(100.015, 100.02)]
    [InlineData(0.000, 0.00)]
    public void Round_AppliesMidpointRoundingAwayFromZero(decimal input, decimal expected)
    {
        var result = MoneyMath.Round(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateGrossRevenue_NormalVoucher_DeductsCorrectly()
    {
        var result = MoneyMath.CalculateGrossRevenue(500000m, 50000m);
        Assert.Equal(450000m, result);
    }

    [Fact]
    public void CalculateGrossRevenue_VoucherEqualsSubtotal_YieldsZero()
    {
        var result = MoneyMath.CalculateGrossRevenue(500000m, 500000m);
        Assert.Equal(0m, result);
    }

    [Fact]
    public void CalculateGrossRevenue_VoucherExceedsSubtotal_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => MoneyMath.CalculateGrossRevenue(500000m, 600000m));
    }

    [Fact]
    public void CalculateGrossRevenue_NegativeVoucher_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => MoneyMath.CalculateGrossRevenue(500000m, -10000m));
    }

    [Fact]
    public void CalculateRateFee_ComputesAndRoundsAwayFromZero()
    {
        // 500,000 * 4.5% = 22,500
        var result = MoneyMath.CalculateRateFee(500000m, 0.045m);
        Assert.Equal(22500m, result);
    }
}
