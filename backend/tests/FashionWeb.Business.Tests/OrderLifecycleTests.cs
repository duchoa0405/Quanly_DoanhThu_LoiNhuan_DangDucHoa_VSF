using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class OrderLifecycleTests
{
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IReconciliationRepository> _mockReconRepo;
    private readonly Mock<IDynamicFeeEngine> _mockFeeEngine;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly OrderService _orderService;

    public OrderLifecycleTests()
    {
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockReconRepo = new Mock<IReconciliationRepository>();
        _mockFeeEngine = new Mock<IDynamicFeeEngine>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        // Set up mock UnitOfWork to directly invoke the transaction action
        _mockUnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());

        _orderService = new OrderService(
            _mockOrderRepo.Object,
            _mockProductRepo.Object,
            _mockReconRepo.Object,
            _mockFeeEngine.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task UpdateOrderStatus_FromPendingToShipped_TransitionsSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            ExternalOrderId = "ORD-001",
            Channel = ChannelType.SHOPEE,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            Status = OrderStatus.PENDING
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var cmd = new UpdateOrderStatusCommand(orderId, OrderProgressStatus.SHIPPED, "operator@shop.vn");

        // Act
        var updated = await _orderService.UpdateOrderStatusAsync(cmd);

        // Assert
        Assert.Equal(OrderStatus.SHIPPED, updated.Status);
        Assert.Contains(updated.StatusHistory, sh => sh.ToStatus == OrderStatus.SHIPPED && sh.FromStatus == OrderStatus.PENDING);
        _mockOrderRepo.Verify(r => r.UpdateAsync(order, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_FromShippedToDelivered_FreezesFeesAndCreatesReconciliation()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            ExternalOrderId = "ORD-002",
            Channel = ChannelType.TIKTOK,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            Status = OrderStatus.SHIPPED
        };
        order.SetFinancials(500000m, 50000m); // Gross Revenue: 450,000

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var feeSnapshot = new OrderFeeSnapshot
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            FeeScheduleId = Guid.NewGuid(),
            TotalPlatformFees = 36500m,
            ProjectedSettlement = 413500m,
            SnapshotAt = DateTime.UtcNow
        };

        _mockFeeEngine
            .Setup(e => e.CalculateAndFreezeFeeAsync(order, It.IsAny<CancellationToken>()))
            .ReturnsAsync(feeSnapshot);

        var cmd = new UpdateOrderStatusCommand(orderId, OrderProgressStatus.DELIVERED, "delivery_driver@logistics.vn");

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(cmd);

        // Assert
        Assert.Equal(OrderStatus.DELIVERED, result.Status);
        Assert.NotNull(result.DeliveredAt);
        Assert.NotNull(result.FeeSnapshot);
        Assert.Equal(413500m, result.FeeSnapshot.ProjectedSettlement);

        Assert.NotNull(result.ReconciliationRecord);
        Assert.Equal(ReconciliationStatus.PENDING_SETTLEMENT, result.ReconciliationRecord.Status);
        Assert.Equal(413500m, result.ReconciliationRecord.ProjectedSettlement);

        _mockOrderRepo.Verify(r => r.AddFeeSnapshotAsync(feeSnapshot, It.IsAny<CancellationToken>()), Times.Once);
        _mockReconRepo.Verify(r => r.AddAsync(It.Is<ReconciliationRecord>(rec => rec.OrderId == orderId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateOrderStatus_PendingToDeliveredDirectly_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.PENDING
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var cmd = new UpdateOrderStatusCommand(orderId, OrderProgressStatus.DELIVERED, "operator@shop.vn");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orderService.UpdateOrderStatusAsync(cmd)
        );
    }

    [Fact]
    public async Task CancelOrder_PendingOrder_CancelsSuccessfully()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.PENDING
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var cmd = new CancelOrderCommand(orderId, "Customer requested cancellation before fulfillment", "sales@shop.vn");

        // Act
        var cancelled = await _orderService.CancelOrderAsync(cmd);

        // Assert
        Assert.Equal(OrderStatus.CANCELLED, cancelled.Status);
        Assert.NotNull(cancelled.CancelledAt);
        Assert.Equal("Customer requested cancellation before fulfillment", cancelled.CancellationReason);
        Assert.Contains(cancelled.StatusHistory, sh => sh.ToStatus == OrderStatus.CANCELLED && sh.Reason == "Customer requested cancellation before fulfillment");
    }

    [Fact]
    public async Task CancelOrder_DeliveredOrder_ThrowsInvalidOperationException()
    {
        // Arrange
        var orderId = Guid.NewGuid();
        var order = new Order
        {
            Id = orderId,
            Status = OrderStatus.DELIVERED,
            DeliveredAt = DateTime.UtcNow
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var cmd = new CancelOrderCommand(orderId, "Trying to cancel delivered goods", "sales@shop.vn");

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _orderService.CancelOrderAsync(cmd)
        );
    }
}
