using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.Discrepancies;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/discrepancies")]
[Authorize(Policy = Policies.RequireFinanceManager)]
public class DiscrepanciesController : BaseApiController
{
    private readonly IDiscrepancyService _discrepancyService;

    public DiscrepanciesController(IDiscrepancyService discrepancyService)
    {
        _discrepancyService = discrepancyService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedDiscrepancyListResponse>> ListDiscrepancies(
        [FromQuery] ChannelType? channel,
        [FromQuery] DiscrepancyType? discrepancyType,
        [FromQuery] bool? isResolved,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new DiscrepancyQueryFilter(channel, discrepancyType, isResolved, from, to, search, page, pageSize);
        var paged = await _discrepancyService.ListDiscrepanciesAsync(filter, ct);

        var items = paged.Items.Select(d => new DiscrepancyResponse(
            Id: d.Id,
            ReconciliationId: d.ReconciliationId,
            OrderId: d.ReconciliationRecord?.OrderId ?? Guid.Empty,
            ExternalOrderId: d.ReconciliationRecord?.Order?.ExternalOrderId ?? string.Empty,
            Channel: d.ReconciliationRecord?.Order?.Channel ?? ChannelType.POS,
            DiscrepancyType: d.DiscrepancyType,
            ExplanationNote: d.ExplanationNote,
            VarianceAmount: d.ReconciliationRecord?.VarianceAmount ?? 0.00m,
            IsResolved: d.IsResolved,
            CreatedAt: d.CreatedAt
        )).ToList();

        return Ok(new PagedDiscrepancyListResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DiscrepancyDetailResponse>> GetDiscrepancyById(Guid id, CancellationToken ct)
    {
        var detail = await _discrepancyService.GetByIdAsync(id, ct);
        if (detail == null)
            return NotFound(new ProblemDetails { Title = "Discrepancy Not Found", Detail = $"Discrepancy audit with ID '{id}' was not found.", Status = 404 });

        return Ok(new DiscrepancyDetailResponse(
            Id: detail.Id,
            ReconciliationId: detail.ReconciliationId,
            OrderId: detail.OrderId,
            ExternalOrderId: detail.ExternalOrderId,
            Channel: detail.Channel,
            DiscrepancyType: detail.DiscrepancyType,
            ExplanationNote: detail.ExplanationNote,
            VarianceAmount: detail.VarianceAmount,
            IsResolved: detail.IsResolved,
            CreatedAt: detail.CreatedAt,
            ResolvedAt: detail.ResolvedAt,
            ResolvedBy: detail.ResolvedBy,
            ResolutionNotes: detail.ResolutionNotes
        ));
    }

    [HttpPatch("{id:guid}/resolve")]
    public async Task<ActionResult<DiscrepancyDetailResponse>> ResolveDiscrepancy(
        Guid id,
        [FromBody] ResolveDiscrepancyRequest request,
        CancellationToken ct)
    {
        var cmd = new ResolveDiscrepancyCommand(
            DiscrepancyId: id,
            ResolutionNotes: request.ResolutionNotes,
            ActorIdentity: GetCurrentUserIdentity()
        );

        await _discrepancyService.ResolveDiscrepancyAsync(cmd, ct);
        var detail = await _discrepancyService.GetByIdAsync(id, ct);

        if (detail == null)
            return NotFound(new ProblemDetails { Title = "Discrepancy Not Found", Detail = $"Discrepancy audit with ID '{id}' was not found.", Status = 404 });

        return Ok(new DiscrepancyDetailResponse(
            Id: detail.Id,
            ReconciliationId: detail.ReconciliationId,
            OrderId: detail.OrderId,
            ExternalOrderId: detail.ExternalOrderId,
            Channel: detail.Channel,
            DiscrepancyType: detail.DiscrepancyType,
            ExplanationNote: detail.ExplanationNote,
            VarianceAmount: detail.VarianceAmount,
            IsResolved: detail.IsResolved,
            CreatedAt: detail.CreatedAt,
            ResolvedAt: detail.ResolvedAt,
            ResolvedBy: detail.ResolvedBy,
            ResolutionNotes: detail.ResolutionNotes
        ));
    }
}
