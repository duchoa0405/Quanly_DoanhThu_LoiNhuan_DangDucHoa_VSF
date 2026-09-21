using FashionWeb.Api.Contracts.Orders;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/orders")]
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

        var items = paged.Items.Select(o => new OrderListItemResponse(
            Id: o.Id,
            ExternalOrderId: o.ExternalOrderId,
            Channel: o.Channel,
            PaymentMethod: o.PaymentMethod,
            Status: o.Status,
            OrderDate: o.OrderDate,
            CustomerName: o.CustomerName,
            CustomerPhone: o.CustomerPhone,
            Subtotal: o.Subtotal,
            ShopVoucher: o.ShopVoucher,
            GrossRevenue: o.GrossRevenue,
            ItemCount: o.Items.Sum(i => i.Quantity),
            ItemsSummary: o.Items.Select(i => new OrderItemSummary(i.SkuCodeSnapshot, i.Quantity)).ToList(),
            DeliveredAt: o.DeliveredAt,
            CancelledAt: o.CancelledAt,
            CreatedAt: o.CreatedAt
        )).ToList();

        return Ok(new PagedOrderListResponse(items, paged.Page, paged.PageSize, paged.TotalItems, paged.TotalPages));
    }

    [HttpPost]
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
        return CreatedAtAction(nameof(GetOrderById), new { id = created.Id }, MapToOrderResponse(created));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<OrderSummaryResponse>> GetOrderSummary(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] ChannelType? channel,
        CancellationToken ct = default)
    {
        var summary = await _orderService.GetSummaryAsync(from, to, channel, ct);
        return Ok(new OrderSummaryResponse(
            TotalOrders: summary.TotalOrders,
            PendingOrders: summary.PendingOrders,
            ShippedOrders: summary.ShippedOrders,
            DeliveredOrders: summary.DeliveredOrders,
            CancelledOrders: summary.CancelledOrders,
            RecognizedGrossRevenue: summary.RecognizedGrossRevenue
        ));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<OrderDetailResponse>> GetOrderById(Guid id, CancellationToken ct)
    {
        var order = await _orderService.GetOrderByIdAsync(id, ct);
        if (order == null)
            return NotFound(new ProblemDetails { Title = "Order Not Found", Detail = $"Order with ID '{id}' was not found.", Status = 404 });

        decimal? cogs = order.Status == OrderStatus.DELIVERED ? order.Items.Sum(i => i.TotalCost) : null;
        decimal? contributionProfit = (order.Status == OrderStatus.DELIVERED && order.FeeSnapshot != null && cogs.HasValue)
            ? order.FeeSnapshot.ProjectedSettlement - cogs.Value
            : null;

        var dto = new OrderDetailResponse(
            Id: order.Id,
            ExternalOrderId: order.ExternalOrderId,
            Channel: order.Channel,
            PaymentMethod: order.PaymentMethod,
            Status: order.Status,
            Subtotal: order.Subtotal,
            ShopVoucher: order.ShopVoucher,
            GrossRevenue: order.GrossRevenue,
            CustomerName: order.CustomerName,
            CustomerPhone: order.CustomerPhone,
            OrderDate: order.OrderDate,
            DeliveredAt: order.DeliveredAt,
            CancelledAt: order.CancelledAt,
            CancellationReason: order.CancellationReason,
            CreatedAt: order.CreatedAt,
            UpdatedAt: order.UpdatedAt,
            Cogs: cogs,
            ContributionProfit: contributionProfit,
            Items: order.Items.Select(i => new OrderItemResponse(
                Id: i.Id,
                ProductVariantId: i.ProductVariantId,
                SkuCodeSnapshot: i.SkuCodeSnapshot,
                ProductNameSnapshot: i.ProductNameSnapshot,
                Quantity: i.Quantity,
                UnitPrice: i.UnitPrice,
                UnitCostSnapshot: i.UnitCostSnapshot,
                LineTotal: i.LineTotal,
                TotalCost: i.TotalCost
            )).ToList(),
            StatusHistory: order.StatusHistory.Select(sh => new OrderStatusHistoryResponse(
                Id: sh.Id,
                FromStatus: sh.FromStatus,
                ToStatus: sh.ToStatus,
                Reason: sh.Reason,
                ChangedBy: sh.ChangedBy,
                ChangedAt: sh.ChangedAt
            )).ToList(),
            FeeSnapshot: order.FeeSnapshot != null ? new FeeSnapshotResponse(
                Id: order.FeeSnapshot.Id,
                CommissionRate: order.FeeSnapshot.CommissionFeeRate,
                CommissionFeeAmount: order.FeeSnapshot.CommissionFeeAmount,
                PaymentFeeRate: order.FeeSnapshot.PaymentFeeRate,
                PaymentFeeAmount: order.FeeSnapshot.PaymentFeeAmount,
                ServiceFeeRate: order.FeeSnapshot.ServiceFeeRate,
                ServiceFeeAmount: order.FeeSnapshot.ServiceFeeAmount,
                ServiceFeeCapSnapshot: order.FeeSnapshot.ServiceFeeCapSnapshot,
                FixedFeeAmount: order.FeeSnapshot.FixedFeeAmount,
                TotalPlatformFees: order.FeeSnapshot.TotalPlatformFees,
                ProjectedSettlement: order.FeeSnapshot.ProjectedSettlement,
                SnapshotAt: order.FeeSnapshot.SnapshotAt
            ) : null
        );

        return Ok(dto);
    }

    [HttpPost("preview-fee")]
    [HttpPost("fee-preview")]
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
    public async Task<ActionResult<OrderResponse>> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        var cmd = new UpdateOrderStatusCommand(id, request.ToStatus, GetCurrentUserIdentity());
        var updated = await _orderService.UpdateOrderStatusAsync(cmd, ct);
        return Ok(MapToOrderResponse(updated));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<OrderResponse>> CancelOrder(
        Guid id,
        [FromBody] CancelOrderRequest request,
        CancellationToken ct)
    {
        var cmd = new CancelOrderCommand(id, request.CancellationReason, GetCurrentUserIdentity());
        var cancelled = await _orderService.CancelOrderAsync(cmd, ct);
        return Ok(MapToOrderResponse(cancelled));
    }

    private static OrderResponse MapToOrderResponse(Order o) =>
        new(
            Id: o.Id,
            ExternalOrderId: o.ExternalOrderId,
            Channel: o.Channel,
            PaymentMethod: o.PaymentMethod,
            Status: o.Status,
            Subtotal: o.Subtotal,
            ShopVoucher: o.ShopVoucher,
            GrossRevenue: o.GrossRevenue,
            CustomerName: o.CustomerName,
            CustomerPhone: o.CustomerPhone,
            OrderDate: o.OrderDate,
            DeliveredAt: o.DeliveredAt,
            CancelledAt: o.CancelledAt,
            CancellationReason: o.CancellationReason,
            CreatedAt: o.CreatedAt,
            UpdatedAt: o.UpdatedAt
        );
}
