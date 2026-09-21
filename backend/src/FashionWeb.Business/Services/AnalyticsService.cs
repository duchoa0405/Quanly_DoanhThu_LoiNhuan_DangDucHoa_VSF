using System.Globalization;
using System.Text;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IAnalyticsRepository _analyticsRepo;

    public AnalyticsService(IAnalyticsRepository analyticsRepo)
    {
        _analyticsRepo = analyticsRepo;
    }

    public async Task<FinancialKpiResult> GetKpisAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepo.QueryKpisAsync(filter, ct);
    }

    public async Task<List<FinancialTrendPointResult>> GetTrendAsync(TrendFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepo.QueryTrendAsync(filter, ct);
    }

    public async Task<List<ChannelBreakdownResult>> GetChannelBreakdownAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepo.QueryChannelBreakdownAsync(filter, ct);
    }

    public async Task<List<TopSkuResult>> GetTopSkusAsync(TopSkuFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepo.QueryTopSkusAsync(filter, ct);
    }

    public async Task<PagedResult<DrilldownOrderResult>> GetDrilldownAsync(DrilldownFilter filter, CancellationToken ct = default)
    {
        return await _analyticsRepo.QueryDrilldownAsync(filter, ct);
    }

    public async Task<byte[]> ExportCsvAsync(AnalyticsFilter filter, CancellationToken ct = default)
    {
        var orders = await _analyticsRepo.QueryRawExportDataAsync(filter, ct);

        var sb = new StringBuilder();
        sb.AppendLine("Order Id,External Order Id,Channel,Delivered At,Gross Revenue,Total Platform Fees,Projected Settlement,COGS,Contribution Profit,Contribution Margin Pct");

        foreach (var o in orders)
        {
            var line = string.Format(
                CultureInfo.InvariantCulture,
                "\"{0}\",\"{1}\",\"{2}\",\"{3:yyyy-MM-dd HH:mm:ss}\",{4:F2},{5:F2},{6:F2},{7:F2},{8:F2},{9:F2}%",
                o.Id,
                o.ExternalOrderId,
                o.Channel,
                o.DeliveredAt,
                o.GrossRevenue,
                o.TotalPlatformFees,
                o.ProjectedSettlement,
                o.Cogs,
                o.ContributionProfit,
                o.ContributionMarginPct
            );
            sb.AppendLine(line);
        }

        var preamble = Encoding.UTF8.GetPreamble();
        var contentBytes = Encoding.UTF8.GetBytes(sb.ToString());
        var result = new byte[preamble.Length + contentBytes.Length];
        Buffer.BlockCopy(preamble, 0, result, 0, preamble.Length);
        Buffer.BlockCopy(contentBytes, 0, result, preamble.Length, contentBytes.Length);

        return result;
    }
}
