using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Strategies;

namespace FashionWeb.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly FeeStrategyFactory _feeFactory;

    public OrderService(IOrderRepository orderRepo, FeeStrategyFactory feeFactory)
    {
        _orderRepo = orderRepo;
        _feeFactory = feeFactory;
    }

    public async Task<object> GetOrdersAsync(string? channel, string? status, int page, int pageSize)
    {
        var orders = await _orderRepo.GetAllAsync(channel, status, page, pageSize);
        return new { items = orders, page, pageSize };
    }

    public async Task<Order> CreateOrderAsync(object request)
    {
        // Concrete creation logic with snapshot
        var order = new Order();
        await _orderRepo.AddAsync(order);
        await _orderRepo.SaveChangesAsync();
        return order;
    }

    public async Task<Order> UpdateStatusAsync(Guid id, string newStatus)
    {
        var order = await _orderRepo.GetByIdAsync(id) 
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng ID: {id}");

        if (Enum.TryParse<OrderStatus>(newStatus, true, out var status))
        {
            order.Status = status;
            if (status == OrderStatus.Delivered)
            {
                order.DeliveredDate = DateTime.UtcNow;
                // Take immutable snapshot upon delivery
                var strategy = _feeFactory.GetStrategy(order.Channel.ToString());
                var fee = strategy.CalculateFees(order.SubtotalAmount, order.ShopVoucherDiscount);
                order.FeeSnapshot = new OrderFeeSnapshot
                {
                    OrderId = order.Id,
                    CommissionFeeAmount = fee.CommissionFee,
                    PaymentFeeAmount = fee.PaymentFee,
                    FixedFeeAmount = fee.FixedFee,
                    ServiceFeeAmount = fee.ServiceFee,
                    TotalPlatformFees = fee.TotalFees,
                    ExpectedNetPayout = fee.ExpectedNetPayout
                };
            }
            await _orderRepo.UpdateAsync(order);
            await _orderRepo.SaveChangesAsync();
        }
        return order;
    }

    public async Task CancelOrderAsync(Guid id, string reason)
    {
        var order = await _orderRepo.GetByIdAsync(id) 
            ?? throw new KeyNotFoundException($"Không tìm thấy đơn hàng ID: {id}");

        order.Status = OrderStatus.Cancelled;
        order.CancelledDate = DateTime.UtcNow;
        order.CancellationReason = reason;

        await _orderRepo.UpdateAsync(order);
        await _orderRepo.SaveChangesAsync();
    }
}
