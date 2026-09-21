using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepository;
    private readonly IAnalyticsCsvExporter _csvExporter;

    public AnalyticsService(IAnalyticsRepository analyticsRepository, IAnalyticsCsvExporter csvExporter)
    {
        _analyticsRepository = analyticsRepository ?? throw new ArgumentNullException(nameof(analyticsRepository));
        _csvExporter = csvExporter ?? throw new ArgumentNullException(nameof(csvExporter));
    }

    public async Task<FinancialKpiResult> GetKpisAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepository.QueryKpisAsync(filter, ct);
    }

    public async Task<List<FinancialTrendPointResult>> GetTrendAsync(TrendFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepository.QueryTrendAsync(filter, ct);
    }

    public async Task<List<ChannelBreakdownResult>> GetChannelBreakdownAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepository.QueryChannelBreakdownAsync(filter, ct);
    }

    public async Task<List<TopSkuResult>> GetTopSkusAsync(TopSkuFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepository.QueryTopSkusAsync(filter, ct);
    }

    public async Task<PagedResult<DrilldownOrderResult>> GetDrilldownAsync(DrilldownFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepository.QueryDrilldownAsync(filter, ct);
    }

    public async Task<byte[]> ExportCsvAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        var orders = await _analyticsRepository.QueryRawExportDataAsync(filter, ct);
        return _csvExporter.ExportOrders(orders);
    }
}
