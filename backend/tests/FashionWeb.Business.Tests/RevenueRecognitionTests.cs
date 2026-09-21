using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using Xunit;

namespace FashionWeb.Business.Tests;

public class RevenueRecognitionTests
{
    [Fact]
    public void OrderGrossRevenue_CalculatesAccurately_WithoutPhantomDeductions()
    {
        // Arrange
        var order = new Order();
        decimal subtotal = 1000000m;
        decimal shopVoucher = 150000m;

        // Act
        order.SetFinancials(subtotal, shopVoucher);

        // Assert
        Assert.Equal(850000m, order.GrossRevenue);
    }

    [Fact]
    public void OrderFinancials_VoucherExceedingSubtotal_ThrowsArgumentException()
    {
        var order = new Order();
        Assert.Throws<ArgumentException>(() => order.SetFinancials(100000m, 120000m));
    }

    [Fact]
    public void RecognizedGrossRevenue_FiltersStrictlyForDeliveredStatus()
    {
        // Arrange: A mixed set of orders in different lifecycle states
        var orders = new List<Order>
        {
            CreateOrder(OrderStatus.PENDING, 500000m, 50000m),   // Gross: 450,000 (NOT recognized)
            CreateOrder(OrderStatus.SHIPPED, 300000m, 0m),       // Gross: 300,000 (NOT recognized)
            CreateOrder(OrderStatus.CANCELLED, 800000m, 100000m),// Gross: 700,000 (NOT recognized)
            CreateOrder(OrderStatus.DELIVERED, 600000m, 50000m), // Gross: 550,000 (RECOGNIZED)
            CreateOrder(OrderStatus.DELIVERED, 400000m, 0m)      // Gross: 400,000 (RECOGNIZED)
        };

        // Act: Apply canonical recognition filter
        var recognizedRevenue = orders
            .Where(o => o.Status == OrderStatus.DELIVERED)
            .Sum(o => o.GrossRevenue);

        // Assert: Only 550k + 400k = 950k is recognized. PENDING (450k), SHIPPED (300k), and CANCELLED (700k) contribute 0.
        Assert.Equal(950000m, recognizedRevenue);
    }

    [Fact]
    public void NonDeliveredOrders_ContributeZeroToRecognizedRevenue()
    {
        var nonDeliveredOrders = new List<Order>
        {
            CreateOrder(OrderStatus.PENDING, 1000000m, 0m),
            CreateOrder(OrderStatus.SHIPPED, 2000000m, 100000m),
            CreateOrder(OrderStatus.CANCELLED, 500000m, 0m)
        };

        var recognizedRevenue = nonDeliveredOrders
            .Where(o => o.Status == OrderStatus.DELIVERED)
            .Sum(o => o.GrossRevenue);

        Assert.Equal(0.00m, recognizedRevenue);
    }

    private static Order CreateOrder(OrderStatus status, decimal subtotal, decimal voucher)
    {
        var order = Order.CreateTestInstance(
            id: Guid.NewGuid(),
            status: status,
            channel: ChannelType.SHOPEE,
            paymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            externalOrderId: $"EXT-{Guid.NewGuid():N}"
        );
        order.SetFinancials(subtotal, voucher);
        return order;
    }
}
