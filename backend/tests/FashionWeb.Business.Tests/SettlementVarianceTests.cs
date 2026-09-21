using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class SettlementVarianceTests
{
    private readonly Mock<IReconciliationRepository> _mockReconRepo;
    private readonly Mock<IDiscrepancyRepository> _mockDiscrepancyRepo;
    private readonly Mock<IOrderRepository> _mockOrderRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SettlementService _settlementService;

    public SettlementVarianceTests()
    {
        _mockReconRepo = new Mock<IReconciliationRepository>();
        _mockDiscrepancyRepo = new Mock<IDiscrepancyRepository>();
        _mockOrderRepo = new Mock<IOrderRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockUnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());

        _settlementService = new SettlementService(
            _mockReconRepo.Object,
            _mockDiscrepancyRepo.Object,
            _mockOrderRepo.Object,
            _mockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task ReconcileSettlement_ExactMatch_SetsReconciled_WithoutDiscrepancyAudit()
    {
        // Arrange: Projected 413,500, Actual 413,500 -> Variance 0
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.DELIVERED };
        var record = new ReconciliationRecord
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProjectedSettlement = 413500m,
            Status = ReconciliationStatus.PENDING_SETTLEMENT
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockReconRepo
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var cmd = new ReconcileSettlementCommand(
            OrderId: orderId,
            ActualSettlement: 413500m,
            Notes: null,
            DiscrepancyType: null,
            ActorIdentity: "accountant@shop.vn"
        );

        // Act
        var result = await _settlementService.ReconcileAsync(cmd);

        // Assert
        Assert.Equal(ReconciliationStatus.RECONCILED, result.Status);
        Assert.Equal(0.00m, result.VarianceAmount);
        Assert.Equal(413500m, result.ActualSettlement);
        Assert.NotNull(result.ReconciledAt);
        Assert.Equal("accountant@shop.vn", result.ReconciledBy);

        // Verify no discrepancy audit was saved
        _mockDiscrepancyRepo.Verify(d => d.AddAsync(It.IsAny<DiscrepancyAudit>(), It.IsAny<CancellationToken>()), Times.Never);
        _mockReconRepo.Verify(r => r.UpdateAsync(record, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReconcileSettlement_Shortfall_SetsDiscrepancy_AndSpawnsAuditRecord()
    {
        // Arrange: Projected 413,500, Actual 390,000 -> Variance = 413,500 - 390,000 = +23,500 (Platform underpaid)
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.DELIVERED };
        var record = new ReconciliationRecord
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProjectedSettlement = 413500m,
            Status = ReconciliationStatus.PENDING_SETTLEMENT
        };

        _mockOrderRepo
            .Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        _mockReconRepo
            .Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(record);

        var cmd = new ReconcileSettlementCommand(
            OrderId: orderId,
            ActualSettlement: 390000m,
            Notes: "Platform charged higher payment gateway fee without notice",
            DiscrepancyType: DiscrepancyType.PAYMENT_FEE_MISMATCH,
            ActorIdentity: "accountant@shop.vn"
        );

        // Act
        var result = await _settlementService.ReconcileAsync(cmd);

        // Assert
        Assert.Equal(ReconciliationStatus.DISCREPANCY, result.Status);
        Assert.Equal(23500m, result.VarianceAmount);
        Assert.Equal(390000m, result.ActualSettlement);

        _mockDiscrepancyRepo.Verify(d => d.AddAsync(
            It.Is<DiscrepancyAudit>(a =>
                a.ReconciliationId == record.Id &&
                a.DiscrepancyType == DiscrepancyType.PAYMENT_FEE_MISMATCH &&
                a.ExplanationNote == "Platform charged higher payment gateway fee without notice" &&
                !a.IsResolved
            ),
            It.IsAny<CancellationToken>()
        ), Times.Once);
    }

    [Fact]
    public async Task ReconcileSettlement_VarianceWithoutNotes_ThrowsArgumentException()
    {
        // Arrange: Variance exists but notes are missing
        var orderId = Guid.NewGuid();
        var order = new Order { Id = orderId, Status = OrderStatus.DELIVERED };
        var record = new ReconciliationRecord
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            ProjectedSettlement = 500000m,
            Status = ReconciliationStatus.PENDING_SETTLEMENT
        };

        _mockOrderRepo.Setup(r => r.GetOrderDetailByIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(order);
        _mockReconRepo.Setup(r => r.GetByOrderIdAsync(orderId, It.IsAny<CancellationToken>())).ReturnsAsync(record);

        var cmd = new ReconcileSettlementCommand(
            OrderId: orderId,
            ActualSettlement: 480000m, // Variance of 20,000
            Notes: null,               // Missing required note!
            DiscrepancyType: DiscrepancyType.COMMISSION_RATE_MISMATCH,
            ActorIdentity: "accountant@shop.vn"
        );

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _settlementService.ReconcileAsync(cmd)
        );
    }

    [Fact]
    public void DiscrepancyAudit_Resolve_SetsResolutionFieldsAndStatus()
    {
        // Arrange
        var audit = new DiscrepancyAudit
        {
            Id = Guid.NewGuid(),
            ReconciliationId = Guid.NewGuid(),
            DiscrepancyType = DiscrepancyType.OTHER,
            ExplanationNote = "Carrier returned surcharge disputed"
        };

        Assert.False(audit.IsResolved);

        // Act
        audit.Resolve("Platform credited back 20,000 VND voucher adjustment.", "finance_manager@shop.vn");

        // Assert
        Assert.True(audit.IsResolved);
        Assert.NotNull(audit.ResolvedAt);
        Assert.Equal("finance_manager@shop.vn", audit.ResolvedBy);
        Assert.Equal("Platform credited back 20,000 VND voucher adjustment.", audit.ResolutionNotes);
    }
}
