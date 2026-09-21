using FashionWeb.Api.Contracts.Analytics;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/analytics")]
public class AnalyticsController : BaseApiController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("kpis")]
    public async Task<ActionResult<FinancialKpiResponse>> GetKpis(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to, channel);
        var kpis = await _analyticsService.GetKpisAsync(filter, ct);

        return Ok(new FinancialKpiResponse(
            GrossRevenue: kpis.GrossRevenue,
            TotalPlatformFees: kpis.TotalPlatformFees,
            ProjectedSettlement: kpis.ProjectedSettlement,
            Cogs: kpis.Cogs,
            ContributionProfit: kpis.ContributionProfit,
            ContributionMarginPct: kpis.ContributionMarginPct,
            DeliveredOrderCount: kpis.DeliveredOrderCount
        ));
    }

    [HttpGet("trend")]
    [HttpGet("trends")]
    public async Task<ActionResult<FinancialTrendResponse>> GetTrend(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var filter = new TrendFilter(from, to, channel);
        var points = await _analyticsService.GetTrendAsync(filter, ct);

        var dtos = points.Select(p => new FinancialTrendPoint(
            Date: p.Date,
            GrossRevenue: p.GrossRevenue,
            ContributionProfit: p.ContributionProfit
        )).ToList();

        return Ok(new FinancialTrendResponse(dtos));
    }

    [HttpGet("channel-breakdown")]
    public async Task<ActionResult<ChannelBreakdownListResponse>> GetChannelBreakdown(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to, channel);
        var list = await _analyticsService.GetChannelBreakdownAsync(filter, ct);

        var dtos = list.Select(c => new ChannelBreakdownResponse(
            Channel: c.Channel,
            DeliveredOrders: c.DeliveredOrders,
            GrossRevenue: c.GrossRevenue,
            TotalPlatformFees: c.TotalPlatformFees,
            ContributionProfit: c.ContributionProfit,
            ContributionMarginPct: c.ContributionMarginPct
        )).ToList();

        return Ok(new ChannelBreakdownListResponse(dtos));
    }

    [HttpGet("top-skus")]
    public async Task<ActionResult<TopSkuListResponse>> GetTopSkus(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        [FromQuery] int limit = 5,
        CancellationToken ct = default)
    {
        var filter = new TopSkuFilter(from, to, channel, limit);
        var topSkus = await _analyticsService.GetTopSkusAsync(filter, ct);

        var dtos = topSkus.Select(s => new TopSkuResponse(
            SkuCode: s.SkuCode,
            ProductName: s.ProductName,
            DeliveredUnits: s.DeliveredUnits,
            GrossRevenue: s.GrossRevenue,
            Cogs: s.Cogs,
            ContributionProfit: s.ContributionProfit,
            ContributionMarginPct: s.ContributionMarginPct
        )).ToList();

        return Ok(new TopSkuListResponse(dtos));
    }

    [HttpGet("drilldown")]
    public async Task<ActionResult<PagedDrilldownOrderResponse>> GetDrilldown(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new DrilldownFilter(from, to, channel, page, pageSize);
        var paged = await _analyticsService.GetDrilldownAsync(filter, ct);

        var items = paged.Items.Select(d => new DrilldownOrderItem(
            Id: d.Id,
            ExternalOrderId: d.ExternalOrderId,
            Channel: d.Channel,
            DeliveredAt: d.DeliveredAt,
            GrossRevenue: d.GrossRevenue,
            TotalPlatformFees: d.TotalPlatformFees,
            ProjectedSettlement: d.ProjectedSettlement,
            Cogs: d.Cogs,
            ContributionProfit: d.ContributionProfit,
            ContributionMarginPct: d.ContributionMarginPct
        )).ToList();

        return Ok(new PagedDrilldownOrderResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages));
    }

    [HttpGet("export-csv")]
    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var filter = new AnalyticsFilter(from, to, channel);
        var csvBytes = await _analyticsService.ExportCsvAsync(filter, ct);

        return File(csvBytes, "text/csv", $"VSF_Delivered_Orders_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv");
    }
}
