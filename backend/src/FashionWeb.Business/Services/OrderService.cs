using FashionWeb.Business.Commands;
using FashionWeb.Business.Common;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Exceptions;
using FashionWeb.Business.Filters;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class OrderService : IOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IReconciliationRepository _reconciliationRepository;
    private readonly IDynamicFeeEngine _feeEngine;
    private readonly IUnitOfWork _unitOfWork;
    private readonly TimeProvider _timeProvider;

    public OrderService(
        IOrderRepository orderRepository,
        IProductRepository productRepository,
        IReconciliationRepository reconciliationRepository,
        IDynamicFeeEngine feeEngine,
        IUnitOfWork unitOfWork,
        TimeProvider timeProvider)
    {
        _orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _reconciliationRepository = reconciliationRepository ?? throw new ArgumentNullException(nameof(reconciliationRepository));
        _feeEngine = feeEngine ?? throw new ArgumentNullException(nameof(feeEngine));
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    }

    public async Task<Order> CreateOrderAsync(CreateOrderCommand command, CancellationToken ct = default)
    {
        ValidateCreateCommand(command);

        if (await _orderRepository.ExistsExternalOrderIdAsync(command.Channel, command.ExternalOrderId, ct))
        {
            throw new ConflictException(
                $"An order with external ID '{command.ExternalOrderId}' already exists for channel '{command.Channel}'.");
        }

        var variantMap = await LoadRequiredVariantsAsync(command.Items, ct);
        var order = BuildOrderAggregate(command, variantMap);

        await _orderRepository.AddAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return order;
    }

    public async Task<OrderDetailResult?> GetOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetOrderDetailByIdAsync(id, ct);
        if (order == null)
            return null;

        decimal? cogs = null;
        decimal? contributionProfit = null;

        if (order.Status == OrderStatus.DELIVERED)
        {
            cogs = order.Items.Sum(i => i.TotalCost);
            if (order.FeeSnapshot != null && cogs.HasValue)
            {
                contributionProfit = MoneyMath.Round(order.FeeSnapshot.ProjectedSettlement - cogs.Value);
            }
        }

        var itemResults = order.Items.Select(i => new OrderItemDetailResult(
            Id: i.Id,
            ProductVariantId: i.ProductVariantId,
            SkuCodeSnapshot: i.SkuCodeSnapshot,
            ProductNameSnapshot: i.ProductNameSnapshot,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            UnitCostSnapshot: i.UnitCostSnapshot,
            LineTotal: i.LineTotal,
            TotalCost: i.TotalCost
        )).ToList();

        var historyResults = order.StatusHistory.Select(h => new OrderStatusHistoryResult(
            Id: h.Id,
            FromStatus: h.FromStatus,
            ToStatus: h.ToStatus,
            Reason: h.Reason,
            ChangedBy: h.ChangedBy,
            ChangedAt: h.ChangedAt
        )).ToList();

        FeeSnapshotResult? feeSnapshotResult = null;
        if (order.FeeSnapshot != null)
        {
            feeSnapshotResult = new FeeSnapshotResult(
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
            );
        }

        return new OrderDetailResult(
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
            Items: itemResults,
            StatusHistory: historyResults,
            FeeSnapshot: feeSnapshotResult
        );
    }

    public async Task<PagedResult<Order>> ListOrdersAsync(OrderQueryFilter filter, CancellationToken ct = default)
    {
        return await _orderRepository.ListAsync(filter, ct);
    }

    public async Task<OrderSummaryResult> GetSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null, ChannelType? channel = null, CancellationToken ct = default)
    {
        return await _orderRepository.GetSummaryAsync(fromDate, toDate, channel, ct);
    }

    public async Task<Order> UpdateOrderStatusAsync(UpdateOrderStatusCommand command, CancellationToken ct = default)
    {
        var order = await _orderRepository.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new NotFoundException($"Order with ID '{command.OrderId}' was not found.");

        return command.ToStatus switch
        {
            OrderProgressStatus.SHIPPED => await ShipOrderAsync(order, command, ct),
            OrderProgressStatus.DELIVERED => await DeliverOrderAsync(order, command, ct),
            _ => throw new ValidationException($"Target order progress status '{command.ToStatus}' is not supported.")
        };
    }

    public async Task<Order> CancelOrderAsync(CancelOrderCommand command, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(command.CancellationReason))
            throw new ValidationException("Cancellation reason must be provided.");

        var order = await _orderRepository.GetOrderDetailByIdAsync(command.OrderId, ct);
        if (order == null)
            throw new NotFoundException($"Order with ID '{command.OrderId}' was not found.");

        order.Cancel(command.CancellationReason.Trim(), command.ActorIdentity, _timeProvider.GetUtcNow().UtcDateTime);
        await _orderRepository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return order;
    }

    private async Task<Order> ShipOrderAsync(Order order, UpdateOrderStatusCommand command, CancellationToken ct)
    {
        order.TransitionToShipped(command.ActorIdentity, _timeProvider.GetUtcNow().UtcDateTime);
        await _orderRepository.UpdateAsync(order, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        return order;
    }

    private async Task<Order> DeliverOrderAsync(Order order, UpdateOrderStatusCommand command, CancellationToken ct)
    {
        OrderFeeSnapshot feeSnapshot = null!;
        ReconciliationRecord reconRecord = null!;
        var now = _timeProvider.GetUtcNow().UtcDateTime;

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
                CreatedAt = now
            };

            await _orderRepository.UpdateAsync(order, ct);
            await _orderRepository.AddFeeSnapshotAsync(feeSnapshot, ct);
            await _reconciliationRepository.AddAsync(reconRecord, ct);
        }, ct);

        order.FeeSnapshot = feeSnapshot;
        order.ReconciliationRecord = reconRecord;
        return order;
    }

    private static void ValidateCreateCommand(CreateOrderCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.ExternalOrderId))
            throw new ValidationException("External order ID cannot be empty.");

        if (command.Items == null || command.Items.Count == 0)
            throw new ValidationException("An order must contain at least one order item.");

        if (command.ShopVoucher < 0m)
            throw new ValidationException("Shop voucher cannot be negative.");
    }

    private async Task<Dictionary<Guid, ProductVariant>> LoadRequiredVariantsAsync(
        List<CreateOrderItemCommand> items, CancellationToken ct)
    {
        var variantIds = items.Select(i => i.ProductVariantId).Distinct().ToList();
        var variants = await _productRepository.GetVariantsByIdsAsync(variantIds, ct);
        var variantMap = variants.ToDictionary(v => v.Id);

        foreach (var id in variantIds)
        {
            if (!variantMap.ContainsKey(id))
                throw new NotFoundException($"Product variant with ID '{id}' was not found in catalog.");
        }

        return variantMap;
    }

    private Order BuildOrderAggregate(CreateOrderCommand command, Dictionary<Guid, ProductVariant> variantMap)
    {
        var now = _timeProvider.GetUtcNow().UtcDateTime;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            ExternalOrderId = command.ExternalOrderId.Trim(),
            Channel = command.Channel,
            PaymentMethod = command.PaymentMethod,
            CustomerName = command.CustomerName?.Trim(),
            CustomerPhone = command.CustomerPhone?.Trim(),
            OrderDate = now,
            CreatedAt = now
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
        order.SetFinancials(subtotal, command.ShopVoucher, now);

        order.StatusHistory.Add(new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = order.Id,
            FromStatus = null,
            ToStatus = OrderStatus.PENDING,
            ChangedBy = command.ActorIdentity,
            ChangedAt = now
        });

        return order;
    }
}
