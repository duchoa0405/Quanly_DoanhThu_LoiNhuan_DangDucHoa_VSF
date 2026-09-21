using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class DiscrepancyServiceTests
{
    private readonly Mock<IDiscrepancyRepository> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DiscrepancyService _service;

    public DiscrepancyServiceTests()
    {
        _mockRepository = new Mock<IDiscrepancyRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new DiscrepancyService(_mockRepository.Object, _mockUnitOfWork.Object, TimeProvider.System);
    }

    [Fact]
    public async Task ResolveDiscrepancy_ValidNotes_MarksResolvedAndPersists()
    {
        var auditId = Guid.NewGuid();
        var audit = new DiscrepancyAudit
        {
            Id = auditId,
            ReconciliationId = Guid.NewGuid(),
            DiscrepancyType = DiscrepancyType.COMMISSION_RATE_MISMATCH,
            ExplanationNote = "Platform underpaid commission"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(auditId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(audit);

        var cmd = new ResolveDiscrepancyCommand(auditId, "Platform issued refund voucher to balance variance", "finance@shop.vn");

        var resolved = await _service.ResolveDiscrepancyAsync(cmd);

        Assert.True(resolved.IsResolved);
        Assert.NotNull(resolved.ResolvedAt);
        Assert.Equal("finance@shop.vn", resolved.ResolvedBy);
        Assert.Equal("Platform issued refund voucher to balance variance", resolved.ResolutionNotes);
        _mockRepository.Verify(r => r.UpdateAsync(audit, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveDiscrepancy_BlankNotes_ThrowsValidationException()
    {
        var cmd = new ResolveDiscrepancyCommand(Guid.NewGuid(), "   ", "finance@shop.vn");
        await Assert.ThrowsAsync<ValidationException>(() => _service.ResolveDiscrepancyAsync(cmd));
    }

    [Fact]
    public async Task ResolveDiscrepancy_NotFound_ThrowsNotFoundException()
    {
        var auditId = Guid.NewGuid();
        _mockRepository
            .Setup(r => r.GetByIdAsync(auditId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscrepancyAudit?)null);

        var cmd = new ResolveDiscrepancyCommand(auditId, "Resolved note", "finance@shop.vn");
        await Assert.ThrowsAsync<NotFoundException>(() => _service.ResolveDiscrepancyAsync(cmd));
    }

    [Fact]
    public async Task ResolveDiscrepancy_AlreadyResolved_ThrowsBusinessRuleException()
    {
        var auditId = Guid.NewGuid();
        var audit = new DiscrepancyAudit
        {
            Id = auditId,
            ResolvedAt = DateTime.UtcNow,
            ResolvedBy = "admin@shop.vn"
        };

        _mockRepository
            .Setup(r => r.GetByIdAsync(auditId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(audit);

        var cmd = new ResolveDiscrepancyCommand(auditId, "Attempting double resolve", "finance@shop.vn");
        await Assert.ThrowsAsync<BusinessRuleException>(() => _service.ResolveDiscrepancyAsync(cmd));
    }
}
