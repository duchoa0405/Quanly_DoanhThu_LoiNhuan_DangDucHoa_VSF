using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.Analytics;
using FashionWeb.Api.Mappings;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/analytics")]
[Authorize(Policy = Policies.RequireFinanceManager)]
public class AnalyticsController : BaseApiController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<FinancialKpiResponse>> GetKpis(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] ChannelType? channel = null,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to, channel);
        var kpis = await _analyticsService.GetKpisAsync(filter, ct);

        return Ok(AnalyticsContractMapper.MapToFinancialKpiResponse(kpis));
    }

    [HttpGet("trend")]
    public async Task<ActionResult<FinancialTrendResponse>> GetTrend(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] ChannelType? channel = null,
        CancellationToken ct = default)
    {
        var filter = new TrendFilter(from, to, channel);
        var points = await _analyticsService.GetTrendAsync(filter, ct);

        return Ok(AnalyticsContractMapper.MapToFinancialTrendResponse(points));
    }

    [HttpGet("channel-breakdown")]
    public async Task<ActionResult<ChannelBreakdownListResponse>> GetChannelBreakdown(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to);
        var channels = await _analyticsService.GetChannelBreakdownAsync(filter, ct);

        return Ok(AnalyticsContractMapper.MapToChannelBreakdownListResponse(channels));
    }

    [HttpGet("top-skus")]
    public async Task<ActionResult<TopSkuListResponse>> GetTopSkus(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] ChannelType? channel = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] int limit = 10,
        CancellationToken ct = default)
    {
        var filter = new TopSkuFilter(from, to, channel, sortBy, limit);
        var skus = await _analyticsService.GetTopSkusAsync(filter, ct);

        return Ok(AnalyticsContractMapper.MapToTopSkuListResponse(skus));
    }

    [HttpGet("drilldown")]
    public async Task<ActionResult<PagedDrilldownOrderResponse>> GetDrilldown(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] ChannelType? channel = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new DrilldownFilter(from, to, channel, page, pageSize);
        var paged = await _analyticsService.GetDrilldownAsync(filter, ct);

        return Ok(AnalyticsContractMapper.MapToPagedDrilldownResponse(paged));
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] ChannelType? channel = null,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to, channel);
        var csvBytes = await _analyticsService.ExportCsvAsync(filter, ct);

        return File(csvBytes, "text/csv", $"VSF_Delivered_Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
