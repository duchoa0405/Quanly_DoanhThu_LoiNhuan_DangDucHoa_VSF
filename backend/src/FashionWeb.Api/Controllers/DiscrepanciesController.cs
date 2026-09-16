using FashionWeb.Api.Contracts.Discrepancies;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

public class DiscrepanciesController : BaseApiController
{
    private readonly IDiscrepancyService _discrepancyService;

    public DiscrepanciesController(IDiscrepancyService discrepancyService)
    {
        _discrepancyService = discrepancyService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDiscrepancies([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _discrepancyService.GetDiscrepanciesAsync(status, page, pageSize);
        return Ok(result);
    }

    [HttpPost("audit")]
    public async Task<IActionResult> CreateAudit([FromBody] CreateDiscrepancyRequest request)
    {
        var result = await _discrepancyService.CreateAuditAsync(request);
        return Ok(result);
    }

    [HttpPatch("{id}/approve")]
    public async Task<IActionResult> ApproveAudit(Guid id, [FromBody] ApproveDiscrepancyRequest request)
    {
        var result = await _discrepancyService.ApproveAuditAsync(id, request.ApproverId, request.ResolutionNotes);
        return Ok(result);
    }
}
