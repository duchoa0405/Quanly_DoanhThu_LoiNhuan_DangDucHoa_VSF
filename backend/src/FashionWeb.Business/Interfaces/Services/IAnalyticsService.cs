using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface IAnalyticsService
{
    Task<FinancialKpiResult> GetKpisAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<List<FinancialTrendPointResult>> GetTrendAsync(TrendFilter filter, CancellationToken ct = default);
    Task<List<ChannelBreakdownResult>> GetChannelBreakdownAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<List<TopSkuResult>> GetTopSkusAsync(TopSkuFilter filter, CancellationToken ct = default);
    Task<PagedResult<DrilldownOrderResult>> GetDrilldownAsync(DrilldownFilter filter, CancellationToken ct = default);
    Task<byte[]> ExportCsvAsync(AnalyticsFilter filter, CancellationToken ct = default);
}
