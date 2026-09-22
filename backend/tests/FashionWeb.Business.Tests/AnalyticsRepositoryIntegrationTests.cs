using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Filters;
using FashionWeb.Data.Context;
using FashionWeb.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FashionWeb.Business.Tests;

public class AnalyticsRepositoryIntegrationTests
{
    private AppDbContext CreateInMemoryDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: dbName)
            .Options;

        return new AppDbContext(options);
    }

    [Fact]
    public async Task QueryKpisAsync_OrderPlacedInAugust_DeliveredInSeptember_CountedStrictlyInSeptember()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(QueryKpisAsync_OrderPlacedInAugust_DeliveredInSeptember_CountedStrictlyInSeptember));
        var repo = new AnalyticsRepository(context);

        var orderDate = new DateTime(2026, 8, 25, 12, 0, 0, DateTimeKind.Utc);
        var deliveredAt = new DateTime(2026, 9, 2, 15, 0, 0, DateTimeKind.Utc);

        var order = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 500000m,
            channel: ChannelType.TIKTOK,
            paymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            externalOrderId: "TT-AUG-SEP-1",
            orderDate: orderDate,
            deliveredAt: deliveredAt,
            subtotal: 550000m,
            shopVoucher: 50000m
        );

        var feeSnapshot = new OrderFeeSnapshot
        {
            OrderId = order.Id,
            CommissionFeeAmount = 25000m,
            PaymentFeeAmount = 15000m,
            TotalPlatformFees = 40000m,
            ProjectedSettlement = 460000m
        };

        var item = new OrderItem
        {
            OrderId = order.Id,
            ProductVariantId = Guid.NewGuid(),
            SkuCodeSnapshot = "DRESS-001",
            ProductNameSnapshot = "Floral Summer Dress",
            Quantity = 2,
            UnitPrice = 275000m,
            LineTotal = 550000m,
            UnitCostSnapshot = 120000m,
            TotalCost = 240000m
        };

        context.Orders.Add(order);
        context.OrderFeeSnapshots.Add(feeSnapshot);
        context.OrderItems.Add(item);
        await context.SaveChangesAsync();

        // Act 1: Query August
        var augustFilter = new AnalyticsFilter(
            FromDate: new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 8, 31, 23, 59, 59, DateTimeKind.Utc)
        );
        var augustKpi = await repo.QueryKpisAsync(augustFilter);

        // Act 2: Query September
        var septemberFilter = new AnalyticsFilter(
            FromDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc)
        );
        var septemberKpi = await repo.QueryKpisAsync(septemberFilter);

        // Assert
        Assert.Equal(0, augustKpi.DeliveredOrderCount);
        Assert.Equal(0m, augustKpi.GrossRevenue);
        Assert.Equal(0m, augustKpi.ContributionProfit);

        Assert.Equal(1, septemberKpi.DeliveredOrderCount);
        Assert.Equal(500000m, septemberKpi.GrossRevenue);
        Assert.Equal(40000m, septemberKpi.TotalPlatformFees);
        Assert.Equal(460000m, septemberKpi.ProjectedSettlement);
        Assert.Equal(240000m, septemberKpi.Cogs);
        Assert.Equal(220000m, septemberKpi.ContributionProfit);
        Assert.Equal(44.00m, septemberKpi.ContributionMarginPct);
    }

    [Fact]
    public async Task QueryKpisAsync_NonDeliveredOrders_ContributeZeroToRecognizedFinancials()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(QueryKpisAsync_NonDeliveredOrders_ContributeZeroToRecognizedFinancials));
        var repo = new AnalyticsRepository(context);

        var pendingOrder = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.PENDING,
            channel: ChannelType.SHOPEE,
            orderDate: new DateTime(2026, 9, 5, 0, 0, 0, DateTimeKind.Utc),
            subtotal: 300000m,
            shopVoucher: 0m
        );

        var shippedOrder = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.SHIPPED,
            channel: ChannelType.SHOPEE,
            orderDate: new DateTime(2026, 9, 6, 0, 0, 0, DateTimeKind.Utc),
            subtotal: 400000m,
            shopVoucher: 20000m
        );

        var cancelledOrder = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.CANCELLED,
            channel: ChannelType.SHOPEE,
            orderDate: new DateTime(2026, 9, 7, 0, 0, 0, DateTimeKind.Utc),
            subtotal: 500000m,
            shopVoucher: 50000m
        );

        context.Orders.AddRange(pendingOrder, shippedOrder, cancelledOrder);
        await context.SaveChangesAsync();

        // Act
        var filter = new AnalyticsFilter(
            FromDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc)
        );
        var kpis = await repo.QueryKpisAsync(filter);

        // Assert: 0 recognized revenue and 0 orders
        Assert.Equal(0, kpis.DeliveredOrderCount);
        Assert.Equal(0.00m, kpis.GrossRevenue);
        Assert.Equal(0.00m, kpis.TotalPlatformFees);
        Assert.Equal(0.00m, kpis.ContributionProfit);
    }

    [Fact]
    public async Task QueryTopSkusAsync_MultiLineOrder_VoucherAndFeesAllocatedProportionally_ReconcilesToOrderTotals()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(QueryTopSkusAsync_MultiLineOrder_VoucherAndFeesAllocatedProportionally_ReconcilesToOrderTotals));
        var repo = new AnalyticsRepository(context);

        var deliveredAt = new DateTime(2026, 9, 10, 10, 0, 0, DateTimeKind.Utc);
        var order = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 900000m, // Subtotal 1,000,000 - Voucher 100,000
            channel: ChannelType.SHOPEE,
            deliveredAt: deliveredAt,
            subtotal: 1000000m,
            shopVoucher: 100000m
        );

        var feeSnapshot = new OrderFeeSnapshot
        {
            OrderId = order.Id,
            CommissionFeeAmount = 50000m,
            PaymentFeeAmount = 30000m,
            TotalPlatformFees = 80000m,
            ProjectedSettlement = 820000m
        };

        // Line 1: 60% of subtotal (600,000)
        var item1 = new OrderItem
        {
            OrderId = order.Id,
            ProductVariantId = Guid.NewGuid(),
            SkuCodeSnapshot = "SHIRT-RED-M",
            ProductNameSnapshot = "Linen Shirt",
            Quantity = 2,
            UnitPrice = 300000m,
            LineTotal = 600000m,
            UnitCostSnapshot = 150000m,
            TotalCost = 300000m
        };

        // Line 2: 40% of subtotal (400,000)
        var item2 = new OrderItem
        {
            OrderId = order.Id,
            ProductVariantId = Guid.NewGuid(),
            SkuCodeSnapshot = "PANTS-BLK-L",
            ProductNameSnapshot = "Tailored Pants",
            Quantity = 1,
            UnitPrice = 400000m,
            LineTotal = 400000m,
            UnitCostSnapshot = 200000m,
            TotalCost = 200000m
        };

        context.Orders.Add(order);
        context.OrderFeeSnapshots.Add(feeSnapshot);
        context.OrderItems.AddRange(item1, item2);
        await context.SaveChangesAsync();

        // Act
        var filter = new TopSkuFilter(
            FromDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc),
            Limit: 10
        );
        var topSkus = await repo.QueryTopSkusAsync(filter);

        // Assert: 2 items returned
        Assert.Equal(2, topSkus.Count);

        var shirt = topSkus.First(x => x.SkuCode == "SHIRT-RED-M");
        var pants = topSkus.First(x => x.SkuCode == "PANTS-BLK-L");

        // Line 1: Allocated voucher = 60,000. Gross = 540,000. Fee = 48,000. COGS = 300,000. Profit = 192,000.
        Assert.Equal(540000m, shirt.GrossRevenue);
        Assert.Equal(192000m, shirt.ContributionProfit);

        // Line 2: Allocated voucher = 40,000. Gross = 360,000. Fee = 32,000. COGS = 200,000. Profit = 128,000.
        Assert.Equal(360000m, pants.GrossRevenue);
        Assert.Equal(128000m, pants.ContributionProfit);

        // Reconciliation
        var totalSkuGross = shirt.GrossRevenue + pants.GrossRevenue;
        var totalSkuProfit = shirt.ContributionProfit + pants.ContributionProfit;

        var expectedOrderProfit = order.GrossRevenue - feeSnapshot.TotalPlatformFees - (item1.TotalCost + item2.TotalCost);

        Assert.Equal(order.GrossRevenue, totalSkuGross);
        Assert.Equal(expectedOrderProfit, totalSkuProfit);
    }

    [Fact]
    public async Task QueryKpisAsync_DeliveredOrderMissingFeeSnapshot_ThrowsBusinessRuleException()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(QueryKpisAsync_DeliveredOrderMissingFeeSnapshot_ThrowsBusinessRuleException));
        var repo = new AnalyticsRepository(context);

        var order = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 500000m,
            channel: ChannelType.TIKTOK,
            deliveredAt: new DateTime(2026, 9, 15, 12, 0, 0, DateTimeKind.Utc),
            subtotal: 500000m,
            shopVoucher: 0m
        );

        // Do NOT add OrderFeeSnapshot to simulate broken invariant
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var filter = new AnalyticsFilter(
            FromDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc)
        );

        // Act & Assert
        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => repo.QueryKpisAsync(filter));
        Assert.Contains("missing an OrderFeeSnapshot", ex.Message);
    }

    [Fact]
    public async Task QueryChannelBreakdownAsync_ChannelFilter_ReturnsStrictlyMatchingChannelData()
    {
        // Arrange
        using var context = CreateInMemoryDbContext(nameof(QueryChannelBreakdownAsync_ChannelFilter_ReturnsStrictlyMatchingChannelData));
        var repo = new AnalyticsRepository(context);

        var deliveredAt = new DateTime(2026, 9, 12, 10, 0, 0, DateTimeKind.Utc);

        var ttOrder = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 300000m,
            channel: ChannelType.TIKTOK,
            deliveredAt: deliveredAt,
            subtotal: 300000m,
            shopVoucher: 0m
        );
        var ttSnapshot = new OrderFeeSnapshot { OrderId = ttOrder.Id, TotalPlatformFees = 30000m, ProjectedSettlement = 270000m };

        var shopeeOrder = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: OrderStatus.DELIVERED,
            grossRevenue: 500000m,
            channel: ChannelType.SHOPEE,
            deliveredAt: deliveredAt,
            subtotal: 500000m,
            shopVoucher: 0m
        );
        var shopeeSnapshot = new OrderFeeSnapshot { OrderId = shopeeOrder.Id, TotalPlatformFees = 50000m, ProjectedSettlement = 450000m };

        context.Orders.AddRange(ttOrder, shopeeOrder);
        context.OrderFeeSnapshots.AddRange(ttSnapshot, shopeeSnapshot);
        await context.SaveChangesAsync();

        // Act
        var filter = new AnalyticsFilter(
            FromDate: new DateTime(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc),
            ToDate: new DateTime(2026, 9, 30, 23, 59, 59, DateTimeKind.Utc),
            Channel: ChannelType.TIKTOK
        );
        var breakdown = await repo.QueryChannelBreakdownAsync(filter);

        // Assert: Strictly 1 channel returned (TikTok)
        Assert.Single(breakdown);
        Assert.Equal(ChannelType.TIKTOK, breakdown[0].Channel);
        Assert.Equal(1, breakdown[0].DeliveredOrders);
        Assert.Equal(300000m, breakdown[0].GrossRevenue);
        Assert.Equal(30000m, breakdown[0].TotalPlatformFees);
    }
}
