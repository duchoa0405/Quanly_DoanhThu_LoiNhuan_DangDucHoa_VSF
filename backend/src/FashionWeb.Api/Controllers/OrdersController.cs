using FashionWeb.Api.Contracts.Orders;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Strategies;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly FeeStrategyFactory _feeFactory;

    public OrdersController(IOrderService orderService, FeeStrategyFactory feeFactory)
    {
        _orderService = orderService;
        _feeFactory = feeFactory;
    }

    [HttpPost("preview-fee")]
    public IActionResult PreviewFee([FromBody] FeePreviewRequest request)
    {
        var strategy = _feeFactory.GetStrategy(request.ChannelCode);
        var breakdown = strategy.CalculateFees(request.Subtotal, request.ShopVoucher);
        return Ok(breakdown);
    }

    [HttpGet]
    public async Task<IActionResult> GetOrders([FromQuery] string? channel, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _orderService.GetOrdersAsync(channel, status, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var result = await _orderService.CreateOrderAsync(request);
        return CreatedAtAction(nameof(GetOrders), new { id = result.Id }, result);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var result = await _orderService.UpdateStatusAsync(id, request.NewStatus);
        return Ok(result);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
    {
        await _orderService.CancelOrderAsync(id, request.Reason);
        return NoContent();
    }
}
