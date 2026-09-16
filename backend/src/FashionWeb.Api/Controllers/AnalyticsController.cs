using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

public class AnalyticsController : BaseApiController
{
    private readonly IAnalyticsService _analyticsService;

    public AnalyticsController(IAnalyticsService analyticsService)
    {
        _analyticsService = analyticsService;
    }

    [HttpGet("kpis")]
    public async Task<IActionResult> GetKpis([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var kpis = await _analyticsService.GetKpisAsync(fromDate, toDate);
        return Ok(kpis);
    }

    [HttpGet("trend")]
    public async Task<IActionResult> GetTrend([FromQuery] int days = 7)
    {
        var trend = await _analyticsService.GetDailyCashflowTrendAsync(days);
        return Ok(trend);
    }

    [HttpGet("channel-share")]
    public async Task<IActionResult> GetChannelShare()
    {
        var share = await _analyticsService.GetChannelShareAsync();
        return Ok(share);
    }

    [HttpGet("top-skus")]
    public async Task<IActionResult> GetTopSkus([FromQuery] int limit = 5)
    {
        var skus = await _analyticsService.GetTopSkusAsync(limit);
        return Ok(skus);
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv()
    {
        var csvBytes = await _analyticsService.ExportReconciliationCsvAsync();
        return File(csvBytes, "text/csv", $"Reconciliation_Report_{DateTime.UtcNow:yyyyMMdd}.csv");
    }
}
