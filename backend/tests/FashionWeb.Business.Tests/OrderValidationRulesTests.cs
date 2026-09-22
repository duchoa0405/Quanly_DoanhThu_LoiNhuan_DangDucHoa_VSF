using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.Validators;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class OrderValidationRulesTests
{
    [Theory]
    [InlineData(ChannelType.TIKTOK, PaymentMethod.CASH)]
    [InlineData(ChannelType.TIKTOK, PaymentMethod.POS_CARD_QR)]
    [InlineData(ChannelType.SHOPEE, PaymentMethod.CASH)]
    [InlineData(ChannelType.SHOPEE, PaymentMethod.POS_CARD_QR)]
    [InlineData(ChannelType.POS, PaymentMethod.MARKETPLACE_WALLET)]
    public void ValidateChannelPaymentCompatibility_IncompatibleCombinations_ThrowsValidationException(
        ChannelType channel, PaymentMethod paymentMethod)
    {
        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateChannelPaymentCompatibility(channel, paymentMethod));

        Assert.Contains("incompatible", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(ChannelType.TIKTOK, PaymentMethod.MARKETPLACE_WALLET)]
    [InlineData(ChannelType.SHOPEE, PaymentMethod.MARKETPLACE_WALLET)]
    [InlineData(ChannelType.POS, PaymentMethod.CASH)]
    [InlineData(ChannelType.POS, PaymentMethod.POS_CARD_QR)]
    public void ValidateChannelPaymentCompatibility_CompatibleCombinations_Succeeds(
        ChannelType channel, PaymentMethod paymentMethod)
    {
        // Should not throw
        OrderValidationRules.ValidateChannelPaymentCompatibility(channel, paymentMethod);
    }

    [Fact]
    public void ValidateItems_EmptyItemsList_ThrowsValidationException()
    {
        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateItems(new List<CreateOrderItemCommand>()));

        Assert.Contains("at least one order item", ex.Message);
    }

    [Fact]
    public void ValidateItems_DuplicateVariantIds_ThrowsValidationException()
    {
        var variantId = Guid.NewGuid();
        var items = new List<CreateOrderItemCommand>
        {
            new(variantId, 1, 100000m),
            new(variantId, 2, 100000m)
        };

        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateItems(items));

        Assert.Contains("Duplicate product variant", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    public void ValidateItems_ZeroOrNegativeQuantity_ThrowsValidationException(int quantity)
    {
        var items = new List<CreateOrderItemCommand>
        {
            new(Guid.NewGuid(), quantity, 100000m)
        };

        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateItems(items));

        Assert.Contains("Quantity must be greater than zero", ex.Message);
    }

    [Theory]
    [InlineData(-0.01)]
    [InlineData(-100)]
    public void ValidateItems_NegativeUnitPrice_ThrowsValidationException(decimal unitPrice)
    {
        var items = new List<CreateOrderItemCommand>
        {
            new(Guid.NewGuid(), 1, unitPrice)
        };

        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateItems(items));

        Assert.Contains("unit price cannot be negative", ex.Message);
    }

    [Fact]
    public void ValidateVariantActive_InactiveVariant_ThrowsValidationException()
    {
        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            SkuCode = "INACTIVE-SKU",
            IsActive = false
        };

        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateVariantActive(variant));

        Assert.Contains("is inactive", ex.Message);
    }

    [Fact]
    public void ValidateVariantActive_InactiveParentProduct_ThrowsValidationException()
    {
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = "Archived Jacket",
            IsActive = false
        };

        var variant = new ProductVariant
        {
            Id = Guid.NewGuid(),
            SkuCode = "JACKET-BLK-M",
            IsActive = true,
            Product = product
        };

        var ex = Assert.Throws<ValidationException>(() =>
            OrderValidationRules.ValidateVariantActive(variant));

        Assert.Contains("is inactive", ex.Message);
    }

    [Fact]
    public async Task CreateOrderAsync_VoucherExceedsSubtotal_ThrowsValidationException()
    {
        // Arrange
        var mockOrderRepo = new Mock<IOrderRepository>();
        var mockProdRepo = new Mock<IProductRepository>();
        var mockReconRepo = new Mock<IReconciliationRepository>();
        var mockFeeEngine = new Mock<IDynamicFeeEngine>();
        var mockUow = new Mock<IUnitOfWork>();

        var variantId = Guid.NewGuid();
        var variant = new ProductVariant
        {
            Id = variantId,
            SkuCode = "TEE-WHITE-S",
            RetailPrice = 100000m,
            CostPrice = 40000m,
            IsActive = true,
            Product = new Product { Name = "Basic Tee", IsActive = true }
        };

        mockProdRepo.Setup(r => r.GetVariantsByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductVariant> { variant });

        var service = new OrderService(
            mockOrderRepo.Object,
            mockProdRepo.Object,
            mockReconRepo.Object,
            mockFeeEngine.Object,
            mockUow.Object,
            TimeProvider.System
        );

        var cmd = new CreateOrderCommand(
            ExternalOrderId: "ORD-VOUCHER-FAIL",
            Channel: ChannelType.SHOPEE,
            PaymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            CustomerName: "Customer",
            CustomerPhone: "0900000000",
            ShopVoucher: 250000m, // Subtotal is 200,000 (2 * 100,000)
            Items: new List<CreateOrderItemCommand> { new(variantId, 2, 100000m) },
            ActorIdentity: "staff"
        );

        // Act & Assert: Voucher > Subtotal fails
        await Assert.ThrowsAsync<ValidationException>(() => service.CreateOrderAsync(cmd));
    }

    [Fact]
    public async Task CreateOrderAsync_PosChannel_CreatesDirectlyAsDelivered_WithFeeSnapshotAndReconciliation()
    {
        // Arrange
        var mockOrderRepo = new Mock<IOrderRepository>();
        var mockProdRepo = new Mock<IProductRepository>();
        var mockReconRepo = new Mock<IReconciliationRepository>();
        var mockFeeEngine = new Mock<IDynamicFeeEngine>();
        var mockUow = new Mock<IUnitOfWork>();

        mockUow.Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());

        var variantId = Guid.NewGuid();
        var variant = new ProductVariant
        {
            Id = variantId,
            SkuCode = "POS-ITEM-1",
            RetailPrice = 200000m,
            CostPrice = 80000m,
            IsActive = true,
            Product = new Product { Name = "POS Shirt", IsActive = true }
        };

        mockProdRepo.Setup(r => r.GetVariantsByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductVariant> { variant });

        var expectedSnapshot = new OrderFeeSnapshot
        {
            CommissionFeeAmount = 0m,
            PaymentFeeAmount = 3000m,
            TotalPlatformFees = 3000m,
            ProjectedSettlement = 197000m
        };

        mockFeeEngine.Setup(e => e.CalculateAndFreezeFeeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSnapshot);

        var service = new OrderService(
            mockOrderRepo.Object,
            mockProdRepo.Object,
            mockReconRepo.Object,
            mockFeeEngine.Object,
            mockUow.Object,
            TimeProvider.System
        );

        var cmd = new CreateOrderCommand(
            ExternalOrderId: "POS-COUNTER-001",
            Channel: ChannelType.POS,
            PaymentMethod: PaymentMethod.POS_CARD_QR,
            CustomerName: "Walk-in Guest",
            CustomerPhone: null,
            ShopVoucher: 0m,
            Items: new List<CreateOrderItemCommand> { new(variantId, 1, 200000m) },
            ActorIdentity: "pos_cashier"
        );

        // Act
        var result = await service.CreateOrderAsync(cmd);

        // Assert: Direct DELIVERED lifecycle
        Assert.Equal(OrderStatus.DELIVERED, result.Status);
        Assert.NotNull(result.DeliveredAt);
        Assert.NotNull(result.FeeSnapshot);
        Assert.Equal(3000m, result.FeeSnapshot.TotalPlatformFees);
        Assert.NotNull(result.ReconciliationRecord);
        Assert.Equal(ReconciliationStatus.PENDING_SETTLEMENT, result.ReconciliationRecord.Status);
        Assert.Equal(197000m, result.ReconciliationRecord.ProjectedSettlement);

        // Verified repository writes
        mockOrderRepo.Verify(r => r.AddAsync(result, It.IsAny<CancellationToken>()), Times.Once);
        mockOrderRepo.Verify(r => r.AddFeeSnapshotAsync(expectedSnapshot, It.IsAny<CancellationToken>()), Times.Once);
        mockReconRepo.Verify(r => r.AddAsync(It.IsAny<ReconciliationRecord>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateOrderAsync_OnlineChannel_CreatesAsPending_WithoutImmediateDeliveryArtifacts()
    {
        // Arrange
        var mockOrderRepo = new Mock<IOrderRepository>();
        var mockProdRepo = new Mock<IProductRepository>();
        var mockReconRepo = new Mock<IReconciliationRepository>();
        var mockFeeEngine = new Mock<IDynamicFeeEngine>();
        var mockUow = new Mock<IUnitOfWork>();

        var variantId = Guid.NewGuid();
        var variant = new ProductVariant
        {
            Id = variantId,
            SkuCode = "TIKTOK-ITEM-1",
            RetailPrice = 150000m,
            CostPrice = 60000m,
            IsActive = true,
            Product = new Product { Name = "TikTok Dress", IsActive = true }
        };

        mockProdRepo.Setup(r => r.GetVariantsByIdsAsync(It.IsAny<List<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductVariant> { variant });

        var service = new OrderService(
            mockOrderRepo.Object,
            mockProdRepo.Object,
            mockReconRepo.Object,
            mockFeeEngine.Object,
            mockUow.Object,
            TimeProvider.System
        );

        var cmd = new CreateOrderCommand(
            ExternalOrderId: "TT-ONLINE-001",
            Channel: ChannelType.TIKTOK,
            PaymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            CustomerName: "Online Shopper",
            CustomerPhone: "0912345678",
            ShopVoucher: 10000m,
            Items: new List<CreateOrderItemCommand> { new(variantId, 1, 150000m) },
            ActorIdentity: "sales_ops"
        );

        // Act
        var result = await service.CreateOrderAsync(cmd);

        // Assert: Online channel created in PENDING status
        Assert.Equal(OrderStatus.PENDING, result.Status);
        Assert.Null(result.DeliveredAt);
        Assert.Null(result.FeeSnapshot);
        Assert.Null(result.ReconciliationRecord);

        // Fee engine and reconciliation repo must NOT be invoked at creation time
        mockFeeEngine.Verify(e => e.CalculateAndFreezeFeeAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Never);
        mockReconRepo.Verify(r => r.AddAsync(It.IsAny<ReconciliationRecord>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
