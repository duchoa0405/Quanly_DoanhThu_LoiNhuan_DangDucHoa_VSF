using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IProductRepository _productRepo;
    private readonly IReconciliationRepository _reconRepo;
    private readonly IDynamicFeeEngine _feeEngine;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepo,
        IProductRepository productRepo,
        IReconciliationRepository reconRepo,
        IDynamicFeeEngine feeEngine,
        IUnitOfWork unitOfWork)
    {
        _orderRepo = orderRepo;
        _productRepo = productRepo;
        _reconRepo = reconRepo;
        _feeEngine = feeEngine;
        _unitOfWork = unitOfWork;
    }

    public async Task<Order> CreateOrderAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalOrderId))
            throw new ArgumentException("External order ID cannot be empty.", nameof(command));

        if (command.Items == null || command.Items.Count == 0)
            throw new ArgumentException("An order must contain at least one order item.", nameof(command));

        if (await _orderRepo.ExistsExternalOrderIdAsync(command.Channel, command.ExternalOrderId, ct))
            throw new InvalidOperationException($"An order with external ID '{command.ExternalOrderId}' already exists for channel '{command.Channel}'.");

        var variantIds = command.Items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _productRepo.GetVariantsByIdsAsync(variantIds, ct);
        var variantMap = variants.ToDictionary(v => v.Id);

        foreach (var id in variantIds)
        {
            if (!variantMap.ContainsKey(id))
                throw new KeyNotFoundException($"Product variant with ID '{id}' was not found in catalog.");
        }

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ExternalOrderId = command.ExternalOrderId.Trim(),
            Channel = command.Channel,
            PaymentMethod = command.PaymentMethod,
            Status = OrderStatus.PENDING,
            CustomerName = command.CustomerName?.Trim(),
            CustomerPhone = command.CustomerPhone?.Trim(),
            OrderDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var item in command.Items)
        {
            var variant = variantMap[item.ProductVariantId];
            var orderItem = new OrderItem
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                ProductVariantId = variant.Id
            };

            orderItem.FreezeSnapshot(
                skuCode: variant.SkuCode,
                productName: variant.Product?.Name ?? variant.SkuCode,
                quantity: item.Quantity,
                unitPrice: item.UnitPrice,
                unitCost: variant.CostPrice
            );

            order.Items.Add(orderItem);
        }

        var subtotal = order.Items.Sum(i => i.LineTotal);
        order.SetFinancials(subtotal, command.ShopVoucher);

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = null,
            ToStatus = OrderStatus.PENDING,
            ChangedBy = command.ActorIdentity,
            ChangedAt = DateTime.UtcNow
        });

        await _orderRepo.AddAsync(order, ct);
        return order;
    }

    public async Task<Order?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _orderRepo.GetOrderDetailByIdAsync(id, ct);
    }

    public async Task<PagedResult<Order>> ListOrdersAsync(OrderQueryFilter filter, CancellationToken ct = default)
    {
        return await _orderRepo.ListAsync(filter, ct);
    }

    public async Task<OrderSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default)
    {
        return await _orderRepo.GetSummaryAsync(fromDate, toDate, channel, ct);
    }

    public async Task<Order> UpdateOrderStatusAsync(UpdateOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepo.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{command.OrderId}' was not found.");

        if (command.ToStatus == OrderProgressStatus.SHIPPED)
        {
            order.TransitionToShipped(command.ActorIdentity);
            await _orderRepo.UpdateAsync(order, ct);
            return order;
        }

        if (command.ToStatus == OrderProgressStatus.DELIVERED)
        {
            OrderFeeSnapshot feeSnapshot = null!;
            ReconciliationRecord reconRecord = null!;

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                order.TransitionToDelivered(command.ActorIdentity);
                feeSnapshot = await _feeEngine.CalculateAndFreezeFeeAsync(order, ct);

                reconRecord = new ReconciliationRecord
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProjectedSettlement = feeSnapshot.ProjectedSettlement,
                    Status = ReconciliationStatus.PENDING_SETTLEMENT,
                    CreatedAt = DateTime.UtcNow
                };

                await _orderRepo.UpdateAsync(order, ct);
                await _orderRepo.AddFeeSnapshotAsync(feeSnapshot, ct);
                await _reconRepo.AddAsync(reconRecord, ct);
            }, ct);

            order.FeeSnapshot = feeSnapshot;
            order.ReconciliationRecord = reconRecord;
            return order;
        }

        throw new NotSupportedException($"Target order progress status '{command.ToStatus}' is not supported.");
    }

    public async Task<Order> CancelOrderAsync(CancelOrderCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.CancellationReason))
            throw new ArgumentException("Cancellation reason must be provided.", nameof(command));

        var order = await _orderRepo.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new KeyNotFoundException($"Order with ID '{command.OrderId}' was not found.");

        order.Cancel(command.CancellationReason.Trim(), command.ActorIdentity);
        await _orderRepo.UpdateAsync(order, ct);
        return order;
    }
}
