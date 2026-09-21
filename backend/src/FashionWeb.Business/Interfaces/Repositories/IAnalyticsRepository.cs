using FashionWeb.Business.Filters;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IAnalyticsRepository
{
    Task<FinancialKpiResult> QueryKpisAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<List<FinancialTrendPointResult>> QueryTrendAsync(TrendFilter filter, CancellationToken ct = default);
    Task<List<ChannelBreakdownResult>> QueryChannelBreakdownAsync(AnalyticsFilter filter, CancellationToken ct = default);
    Task<List<TopSkuResult>> QueryTopSkusAsync(TopSkuFilter filter, CancellationToken ct = default);
    Task<PagedResult<DrilldownOrderResult>> QueryDrilldownAsync(DrilldownFilter filter, CancellationToken ct = default);
    Task<List<DrilldownOrderResult>> QueryRawExportDataAsync(AnalyticsFilter filter, CancellationToken ct = default);
}
