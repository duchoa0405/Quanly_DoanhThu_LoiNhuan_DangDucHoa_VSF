using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.Settlements;
using FashionWeb.Api.Mappings;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/settlements")]
[Authorize(Policy = Policies.RequireFinanceManager)]
public class SettlementController : BaseApiController
{
    private readonly ISettlementService _settlementService;

    public SettlementController(ISettlementService settlementService)
    {
        _settlementService = settlementService;
    }

    [HttpGet]
    public async Task<ActionResult<PagedSettlementLedgerResponse>> GetSettlementLedger(
        [FromQuery] ChannelType? channel,
        [FromQuery] ReconciliationStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new SettlementQueryFilter(channel, status, from, to, search, page, pageSize);
        var paged = await _settlementService.GetLedgerAsync(filter, ct);

        var items = paged.Items.Select(SettlementContractMapper.MapToLedgerItemResponse).ToList();
        return Ok(new PagedSettlementLedgerResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<SettlementSummaryResponse>> GetSettlementSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var summary = await _settlementService.GetSummaryAsync(from, to, channel, ct);
        return Ok(new SettlementSummaryResponse(
            PendingSettlementCount: summary.PendingSettlementCount,
            ReconciledCount: summary.ReconciledCount,
            DiscrepancyCount: summary.DiscrepancyCount
        ));
    }

    [HttpPost("{orderId:guid}/reconcile")]
    public async Task<ActionResult<ReconciliationResponse>> ReconcileSettlement(
        [FromRoute] Guid orderId,
        [FromBody] ReconcileSettlementRequest request,
        CancellationToken ct = default)
    {
        var cmd = new ReconcileSettlementCommand(
            OrderId: orderId,
            ActualSettlement: request.ActualSettlement,
            Notes: request.Notes,
            DiscrepancyType: request.DiscrepancyType,
            ActorIdentity: GetCurrentUserIdentity()
        );

        var record = await _settlementService.ReconcileAsync(cmd, ct);
        return Ok(SettlementContractMapper.MapToReconciliationResponse(record));
    }
}
