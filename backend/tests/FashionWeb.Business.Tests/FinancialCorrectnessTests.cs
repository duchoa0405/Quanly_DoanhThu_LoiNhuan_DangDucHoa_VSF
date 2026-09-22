using FashionWeb.Business.Common;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using Xunit;

namespace FashionWeb.Business.Tests;

public class FinancialCorrectnessTests
{
    [Fact]
    public void OrderPlacedInAugust_DeliveredInSeptember_BelongsToSeptemberReporting()
    {
        // Arrange
        var orderDate = new DateTime(2026, 8, 28, 14, 30, 0, DateTimeKind.Utc);
        var deliveredAt = new DateTime(2026, 9, 2, 10, 0, 0, DateTimeKind.Utc);

        var order = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 500000m,
            channel: ChannelType.TIKTOK,
            paymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            externalOrderId: "TT-AUG-SEP",
            orderDate: orderDate,
            deliveredAt: deliveredAt,
            subtotal: 550000m,
            shopVoucher: 50000m
        );

        var septemberStart = new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
        var septemberEnd = new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc);

        var augustStart = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc);
        var augustEnd = new DateTime(2026, 8, 31, 23, 59, 59, DateTimeKind.Utc);

        // Act
        var inSeptemberReport = order.Status == OrderStatus.DELIVERED
                                && order.DeliveredAt.HasValue
                                && order.DeliveredAt.Value >= septemberStart
                                && order.DeliveredAt.Value <= septemberEnd;

        var inAugustReport = order.Status == OrderStatus.DELIVERED
                             && order.DeliveredAt.HasValue
                             && order.DeliveredAt.Value >= augustStart
                             && order.DeliveredAt.Value <= augustEnd;

        // Assert: Strictly in September, never in August
        Assert.True(inSeptemberReport, "Delivered order must be recognized in September reporting period.");
        Assert.False(inAugustReport, "Delivered order must NOT be recognized in August based on OrderDate.");
    }

    [Theory]
    [InlineData(OrderStatus.PENDING)]
    [InlineData(OrderStatus.SHIPPED)]
    [InlineData(OrderStatus.CANCELLED)]
    public void NonDeliveredOrders_ContributeZeroToRecognizedFinancials(OrderStatus status)
    {
        // Arrange
        var order = Order.CreateTestInstance(
            status: status,
            grossRevenue: 500000m,
            orderDate: DateTime.UtcNow,
            deliveredAt: null,
            subtotal: 500000m,
            shopVoucher: 0m
        );

        // Act: Financial rule: Revenue is recognized ONLY upon DELIVERED with DeliveredAt timestamp
        var isRecognized = order.Status == OrderStatus.DELIVERED && order.DeliveredAt.HasValue;
        var recognizedRevenue = isRecognized ? order.GrossRevenue : 0.00m;

        // Assert
        Assert.False(isRecognized);
        Assert.Equal(0.00m, recognizedRevenue);
    }

    [Fact]
    public void MultiLineOrder_ShopVoucherAllocation_SumEqualsOrderShopVoucher()
    {
        // Arrange
        decimal orderSubtotal = 1000000m;
        decimal orderShopVoucher = 100000m;

        decimal line1Total = 600000m; // 60%
        decimal line2Total = 400000m; // 40%

        // Act
        var alloc1 = FinancialCalculator.AllocateVoucher(orderShopVoucher, line1Total, orderSubtotal);
        var alloc2 = FinancialCalculator.AllocateVoucher(orderShopVoucher, line2Total, orderSubtotal);

        // Assert
        Assert.Equal(60000m, alloc1);
        Assert.Equal(40000m, alloc2);
        Assert.Equal(orderShopVoucher, alloc1 + alloc2);
    }

    [Fact]
    public void MultiLineOrder_PlatformFeeAllocation_SumEqualsTotalPlatformFees()
    {
        // Arrange
        decimal orderSubtotal = 1000000m;
        decimal totalFees = 74500m;

        decimal line1Total = 700000m; // 70%
        decimal line2Total = 300000m; // 30%

        // Act
        var allocFee1 = FinancialCalculator.AllocateFees(totalFees, line1Total, orderSubtotal);
        var allocFee2 = FinancialCalculator.AllocateFees(totalFees, line2Total, orderSubtotal);

        // Assert
        Assert.Equal(52150m, allocFee1); // 74,500 * 0.70
        Assert.Equal(22350m, allocFee2); // 74,500 * 0.30
        Assert.Equal(totalFees, allocFee1 + allocFee2);
    }

    [Fact]
    public void TopSkuContributionProfit_ReconcilesToOrderContributionProfit()
    {
        // Arrange: 1 order with 2 different SKUs
        decimal orderSubtotal = 1000000m;
        decimal shopVoucher = 100000m;
        decimal totalFees = 80000m;

        // Item 1: SKU-A (LineTotal: 600,000, COGS: 250,000)
        decimal item1LineTotal = 600000m;
        decimal item1Cogs = 250000m;

        // Item 2: SKU-B (LineTotal: 400,000, COGS: 150,000)
        decimal item2LineTotal = 400000m;
        decimal item2Cogs = 150000m;

        decimal totalCogs = item1Cogs + item2Cogs; // 400,000

        // Overall Order Financials
        var orderGrossRevenue = FinancialCalculator.CalculateGrossRevenue(orderSubtotal, shopVoucher); // 900,000
        var orderProfit = FinancialCalculator.CalculateOrderProfit(orderGrossRevenue, totalFees, totalCogs); // 900k - 80k - 400k = 420,000

        // Sku Allocation Financials
        var voucher1 = FinancialCalculator.AllocateVoucher(shopVoucher, item1LineTotal, orderSubtotal); // 60,000
        var voucher2 = FinancialCalculator.AllocateVoucher(shopVoucher, item2LineTotal, orderSubtotal); // 40,000

        var fee1 = FinancialCalculator.AllocateFees(totalFees, item1LineTotal, orderSubtotal); // 48,000
        var fee2 = FinancialCalculator.AllocateFees(totalFees, item2LineTotal, orderSubtotal); // 32,000

        var sku1Gross = FinancialCalculator.CalculateSkuGrossRevenue(item1LineTotal, voucher1); // 540,000
        var sku2Gross = FinancialCalculator.CalculateSkuGrossRevenue(item2LineTotal, voucher2); // 360,000

        var sku1Profit = FinancialCalculator.CalculateSkuProfit(item1LineTotal, voucher1, fee1, item1Cogs); // 540k - 48k - 250k = 242,000
        var sku2Profit = FinancialCalculator.CalculateSkuProfit(item2LineTotal, voucher2, fee2, item2Cogs); // 360k - 32k - 150k = 178,000

        // Assert: 100% Exact Reconciliation
        Assert.Equal(orderGrossRevenue, sku1Gross + sku2Gross);
        Assert.Equal(orderProfit, sku1Profit + sku2Profit);
        Assert.Equal(420000m, orderProfit);
        Assert.Equal(242000m, sku1Profit);
        Assert.Equal(178000m, sku2Profit);
    }

    [Fact]
    public void FinancialCalculator_VoucherExceedingSubtotal_ThrowsValidationException()
    {
        Assert.Throws<ValidationException>(() => FinancialCalculator.CalculateGrossRevenue(100000m, 150000m));
    }

    [Fact]
    public void FinancialCalculator_ContributionMargin_CalculatesAccurately()
    {
        // Profit 420,000 on Gross Revenue 900,000 -> 46.67%
        var margin = FinancialCalculator.CalculateContributionMargin(420000m, 900000m);
        Assert.Equal(46.67m, margin);
    }

    [Fact]
    public void FinancialCalculator_ZeroGrossRevenue_YieldsZeroMargin()
    {
        var margin = FinancialCalculator.CalculateContributionMargin(0m, 0m);
        Assert.Equal(0.00m, margin);
    }
}
