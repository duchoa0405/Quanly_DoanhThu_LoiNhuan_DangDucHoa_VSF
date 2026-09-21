using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.FeeSchedules;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/fee-schedules")]
public class FeeSchedulesController : BaseApiController
{
    private readonly IFeeScheduleService _feeScheduleService;

    public FeeSchedulesController(IFeeScheduleService feeScheduleService)
    {
        _feeScheduleService = feeScheduleService;
    }

    [HttpGet]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<FeeScheduleListResponse>> GetActiveSchedules(
        [FromQuery] ChannelType? channel,
        [FromQuery] PaymentMethod? paymentMethod,
        CancellationToken ct = default)
    {
        var schedules = await _feeScheduleService.GetActiveSchedulesAsync(channel, paymentMethod, ct);
        var dtos = schedules.Select(MapToResponse).ToList();
        return Ok(new FeeScheduleListResponse(dtos));
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireShopOwner)]
    public async Task<ActionResult<FeeScheduleResponse>> CreateScheduleVersion(
        [FromBody] CreateFeeScheduleRequest request,
        CancellationToken ct = default)
    {
        var cmd = new CreateFeeScheduleCommand(
            Channel: request.Channel,
            PaymentMethod: request.PaymentMethod,
            CommissionRate: request.CommissionRate,
            PaymentFeeRate: request.PaymentFeeRate,
            ServiceFeeRate: request.ServiceFeeRate,
            ServiceFeeCap: request.ServiceFeeCap,
            FixedFeePerOrder: request.FixedFeePerOrder,
            EffectiveFrom: request.EffectiveFrom,
            ActorIdentity: GetCurrentUserIdentity()
        );

        var created = await _feeScheduleService.CreateScheduleVersionAsync(cmd, ct);
        return CreatedAtAction(nameof(GetActiveSchedules), new { channel = created.Channel, paymentMethod = created.PaymentMethod }, MapToResponse(created));
    }

    private static FeeScheduleResponse MapToResponse(FeeSchedule s) =>
        new(
            Id: s.Id,
            Channel: s.Channel,
            PaymentMethod: s.PaymentMethod,
            CommissionRate: s.CommissionRate,
            PaymentFeeRate: s.PaymentFeeRate,
            ServiceFeeRate: s.ServiceFeeRate,
            ServiceFeeCap: s.ServiceFeeCap,
            FixedFeePerOrder: s.FixedFeePerOrder,
            EffectiveFrom: s.EffectiveFrom,
            EffectiveTo: s.EffectiveTo,
            IsActive: s.IsActive,
            CreatedAt: s.CreatedAt,
            UpdatedAt: s.UpdatedAt
        );
}
