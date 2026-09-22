using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.Orders;
using FashionWeb.Api.Mappings;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/orders")]
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IDynamicFeeEngine _feeEngine;

    public OrdersController(IOrderService orderService, IDynamicFeeEngine feeEngine)
    {
        _orderService = orderService;
        _feeEngine = feeEngine;
    }

    [HttpGet]
    [Authorize(Policy = Policies.RequireOrderViewer)]
    public async Task<ActionResult<PagedOrderListResponse>> ListOrders(
        [FromQuery] ChannelType? channel,
        [FromQuery] OrderStatus? status,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var filter = new OrderQueryFilter(channel, status, from, to, search, page, pageSize);
        var paged = await _orderService.ListOrdersAsync(filter, ct);
        var items = paged.Items.Select(OrderContractMapper.MapToOrderListItemResponse).ToList();

        return Ok(new PagedOrderListResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages));
    }

    [HttpPost]
    [Authorize(Policy = Policies.RequireSalesOps)]
    public async Task<ActionResult<OrderResponse>> CreateOrder([FromBody] CreateOrderRequest request, CancellationToken ct)
    {
        var cmd = new CreateOrderCommand(
            ExternalOrderId: request.ExternalOrderId,
            Channel: request.Channel,
            PaymentMethod: request.PaymentMethod,
            CustomerName: request.CustomerName,
            CustomerPhone: request.CustomerPhone,
            ShopVoucher: request.ShopVoucher,
            Items: request.Items.Select(i => new CreateOrderItemCommand(
                ProductVariantId: i.ProductVariantId,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice
            )).ToList(),
            ActorIdentity: GetCurrentUserIdentity()
        );

        var created = await _orderService.CreateOrderAsync(cmd, ct);
        return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, OrderContractMapper.MapToOrderResponse(created));
    }

    [HttpGet("summary")]
    [Authorize(Policy = Policies.RequireOrderViewer)]
    public async Task<ActionResult<OrderSummaryResponse>> GetOrderSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var summary = await _orderService.GetSummaryAsync(from, to, channel, ct);
        return Ok(new OrderSummaryResponse(
            TotalOrders: summary.TotalOrders,
            DeliveredOrders: summary.DeliveredOrders,
            GrossRevenue: summary.GrossRevenue,
            InTransitOrders: summary.InTransitOrders,
            CancelledOrders: summary.CancelledOrders
        ));
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.RequireOrderViewer)]
    public async Task<ActionResult<OrderDetailResponse>> GetOrderById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderByIdAsync(id, ct);
        if (order == null)
            return NotFound(new ProblemDetails { Title = "Order Not Found", Detail = $"Order with ID '{id}' was not found.", Status = 404 });

        return Ok(OrderContractMapper.MapToOrderDetailResponse(order, CanViewCostAndProfit()));
    }

    [HttpPost("preview-fee")]
    [Authorize(Policy = Policies.RequireOrderViewer)]
    public async Task<ActionResult<FeeBreakdownResponse>> FeePreview([FromBody] FeePreviewRequest request, CancellationToken ct)
    {
        var preview = await _feeEngine.CalculateFeePreviewAsync(
            request.Channel,
            request.PaymentMethod,
            request.Subtotal,
            request.ShopVoucher,
            ct
        );

        return Ok(new FeeBreakdownResponse(
            Subtotal: preview.Subtotal,
            ShopVoucher: preview.ShopVoucher,
            GrossRevenue: preview.GrossRevenue,
            CommissionFee: preview.CommissionFee,
            PaymentFee: preview.PaymentFee,
            ServiceFee: preview.ServiceFee,
            FixedFee: preview.FixedFee,
            TotalPlatformFees: preview.TotalPlatformFees,
            ProjectedSettlement: preview.ProjectedSettlement
        ));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.RequireSalesOps)]
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(Guid id, [FromBody] UpdateOrderStatusRequest request, CancellationToken ct)
    {
        var cmd = new UpdateOrderStatusCommand(
            id,
            request.ToStatus,
            GetCurrentUserIdentity()
        );

        var updated = await _orderService.UpdateOrderStatusAsync(cmd, ct);
        return Ok(OrderContractMapper.MapToOrderResponse(updated));
    }

    [HttpPost("{id:guid}/cancel")]
    [Authorize(Policy = Policies.RequireSalesOps)]
    public async Task<ActionResult<OrderResponse>> CancelOrder(Guid id, [FromBody] CancelOrderRequest request, CancellationToken ct)
    {
        var cmd = new CancelOrderCommand(
            OrderId: id,
            CancellationReason: request.CancellationReason,
            ActorIdentity: GetCurrentUserIdentity()
        );

        var cancelled = await _orderService.CancelOrderAsync(cmd, ct);
        return Ok(OrderContractMapper.MapToOrderResponse(cancelled));
    }
}
