using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;
using FashionWeb.Business.Services;
using Moq;
using Xunit;

namespace FashionWeb.Business.Tests;

public class AnalyticsServiceTests
{
    private readonly Mock<IAnalyticsRepository> _mockRepository;
    private readonly Mock<IAnalyticsCsvExporter> _mockCsvExporter;
    private readonly AnalyticsService _service;

    public AnalyticsServiceTests()
    {
        _mockRepository = new Mock<IAnalyticsRepository>();
        _mockCsvExporter = new Mock<IAnalyticsCsvExporter>();
        _service = new AnalyticsService(_mockRepository.Object, _mockCsvExporter.Object);
    }

    [Fact]
    public async Task GetKpisAsync_DelegatesToRepositoryWithFilter()
    {
        var filter = new AnalyticsFilter(new DateTime(2026, 9, 1), new DateTime(2026, 9, 20), ChannelType.TIKTOK);
        var expected = new FinancialKpiResult(
            GrossRevenue: 1000000m,
            TotalPlatformFees: 73000m,
            ProjectedSettlement: 927000m,
            Cogs: 400000m,
            ContributionProfit: 527000m,
            ContributionMarginPct: 52.7m,
            DeliveredOrderCount: 5
        );

        _mockRepository
            .Setup(r => r.QueryKpisAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.GetKpisAsync(filter);

        Assert.Equal(expected.GrossRevenue, result.GrossRevenue);
        Assert.Equal(expected.ContributionProfit, result.ContributionProfit);
        Assert.Equal(expected.DeliveredOrderCount, result.DeliveredOrderCount);
    }

    [Fact]
    public async Task ExportCsvAsync_QueriesRawDataAndDelegatesToExporter()
    {
        var filter = new AnalyticsFilter(new DateTime(2026, 9, 1), new DateTime(2026, 9, 20));
        var rawData = new List<DrilldownOrderResult>
        {
            new(
                Id: Guid.NewGuid(),
                ExternalOrderId: "TT-001",
                Channel: ChannelType.TIKTOK,
                DeliveredAt: DateTime.UtcNow,
                GrossRevenue: 500000m,
                TotalPlatformFees: 38000m,
                ProjectedSettlement: 462000m,
                Cogs: 200000m,
                ContributionProfit: 262000m,
                ContributionMarginPct: 52.4m
            )
        };

        var dummyBytes = new byte[] { 0xEF, 0xBB, 0xBF, 0x48, 0x69 };

        _mockRepository
            .Setup(r => r.QueryRawExportDataAsync(filter, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rawData);

        _mockCsvExporter
            .Setup(e => e.ExportOrders(rawData))
            .Returns(dummyBytes);

        var result = await _service.ExportCsvAsync(filter);

        Assert.Equal(dummyBytes, result);
        _mockCsvExporter.Verify(e => e.ExportOrders(rawData), Times.Once);
    }
}
