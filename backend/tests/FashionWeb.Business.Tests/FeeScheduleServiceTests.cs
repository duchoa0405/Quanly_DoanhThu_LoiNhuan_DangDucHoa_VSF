using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class FeeScheduleServiceTests
{
    private readonly Mock<IFeeScheduleRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly FeeScheduleService _service;

    public FeeScheduleServiceTests()
    {
        _mockRepository = new Mock<IFeeScheduleRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _mockUnitOfWork
            .Setup(u => u.ExecuteTransactionAsync(It.IsAny<Func<Task>>(), It.IsAny<CancellationToken>()))
            .Returns<Func<Task>, CancellationToken>((action, _) => action());

        _service = new FeeScheduleService(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task CreateScheduleVersion_ValidParameters_DeactivatesPriorAndInsertsNew()
    {
        var prior = new FeeSchedule
        {
            Id = Guid.NewGuid(),
            Channel = ChannelType.SHOPEE,
            PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate = 0.04m,
            IsActive = true
        };

        _mockRepository
            .Setup(r => r.GetActiveScheduleAsync(ChannelType.SHOPEE, PaymentMethod.MARKETPLACE_WALLET, It.IsAny<CancellationToken>()))
            .ReturnsAsync(prior);

        var effectiveFrom = new DateOnly(2026, 10, 1);
        var cmd = new CreateFeeScheduleCommand(
            Channel: ChannelType.SHOPEE,
            PaymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate: 0.05m,
            PaymentFeeRate: 0.04m,
            ServiceFeeRate: 0.02m,
            ServiceFeeCap: 25000m,
            FixedFeePerOrder: 0m,
            EffectiveFrom: effectiveFrom,
            ActorIdentity: "admin@shop.vn"
        );

        var result = await _service.CreateScheduleVersionAsync(cmd);

        Assert.NotNull(result);
        Assert.True(result.IsActive);
        Assert.Equal(0.05m, result.CommissionRate);
        Assert.False(prior.IsActive);
        Assert.Equal(effectiveFrom, prior.EffectiveTo);
        _mockRepository.Verify(r => r.AddAsync(It.IsAny<FeeSchedule>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateScheduleVersion_NegativeRates_ThrowsValidationException()
    {
        var cmd = new CreateFeeScheduleCommand(
            Channel: ChannelType.TIKTOK,
            PaymentMethod: PaymentMethod.MARKETPLACE_WALLET,
            CommissionRate: -0.01m,
            PaymentFeeRate: 0.03m,
            ServiceFeeRate: 0m,
            ServiceFeeCap: null,
            FixedFeePerOrder: 3000m,
            EffectiveFrom: new DateOnly(2026, 10, 1),
            ActorIdentity: "admin@shop.vn"
        );

        await Assert.ThrowsAsync<ValidationException>(() => _service.CreateScheduleVersionAsync(cmd));
    }
}
