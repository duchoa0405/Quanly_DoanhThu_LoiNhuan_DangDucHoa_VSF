using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalOrderId { get; set; } = string.Empty;
    public ChannelType Channel { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; private set; } = OrderStatus.PENDING;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public decimal Subtotal { get; private set; }
    public decimal ShopVoucher { get; private set; }
    public decimal GrossRevenue { get; private set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }
    public string? CancellationReason { get; private set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    public OrderFeeSnapshot? FeeSnapshot { get; set; }
    public ReconciliationRecord? ReconciliationRecord { get; set; }

    public void SetFinancials(decimal subtotal, decimal shopVoucher, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative.", nameof(subtotal));
        if (shopVoucher < 0)
            throw new ArgumentException("Shop voucher discount cannot be negative.", nameof(shopVoucher));
        if (shopVoucher > subtotal)
            throw new ArgumentException("Shop voucher cannot exceed subtotal.", nameof(shopVoucher));

        Subtotal = subtotal;
        ShopVoucher = shopVoucher;
        GrossRevenue = subtotal - shopVoucher;
        UpdatedAt = timestamp;
    }

    public void MarkAsPending(string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        Status = OrderStatus.PENDING;
        UpdatedAt = timestamp;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = null,
            ToStatus = OrderStatus.PENDING,
            ChangedBy = actorIdentity,
            ChangedAt = timestamp
        });
    }

    public void MarkAsPosDelivered(string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        Status = OrderStatus.DELIVERED;
        DeliveredAt = timestamp;
        UpdatedAt = timestamp;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = null,
            ToStatus = OrderStatus.DELIVERED,
            Reason = "In-store POS counter checkout",
            ChangedBy = actorIdentity,
            ChangedAt = timestamp
        });
    }

    public void TransitionToShipped(string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        if (Status != OrderStatus.PENDING)
            throw new InvalidOperationException($"Cannot transition to SHIPPED from status {Status}.");

        var prior = Status;
        Status = OrderStatus.SHIPPED;
        UpdatedAt = timestamp;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.SHIPPED,
            ChangedBy = actorIdentity,
            ChangedAt = timestamp
        });
    }

    public void TransitionToDelivered(string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        if (Status != OrderStatus.SHIPPED)
            throw new InvalidOperationException($"Cannot transition to DELIVERED from status {Status}. Order must be in SHIPPED status.");

        var prior = Status;
        Status = OrderStatus.DELIVERED;
        DeliveredAt = timestamp;
        UpdatedAt = timestamp;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.DELIVERED,
            ChangedBy = actorIdentity,
            ChangedAt = timestamp
        });
    }

    public void Cancel(string reason, string actorIdentity, DateTime? now = null)
    {
        var timestamp = now ?? DateTime.UtcNow;
        if (Status == OrderStatus.DELIVERED)
            throw new InvalidOperationException("Cannot cancel an order that has already been DELIVERED.");
        if (Status == OrderStatus.CANCELLED)
            throw new InvalidOperationException("Order is already CANCELLED.");

        var prior = Status;
        Status = OrderStatus.CANCELLED;
        CancelledAt = timestamp;
        CancellationReason = reason;
        UpdatedAt = timestamp;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.CANCELLED,
            Reason = reason,
            ChangedBy = actorIdentity,
            ChangedAt = timestamp
        });
    }

    public static Order CreateTestInstance(
        Guid? id = null,
        OrderStatus status = OrderStatus.PENDING,
        decimal grossRevenue = 0m,
        ChannelType channel = ChannelType.SHOPEE,
        PaymentMethod paymentMethod = PaymentMethod.MARKETPLACE_WALLET,
        string externalOrderId = "TEST-ORD",
        DateTime? orderDate = null,
        DateTime? deliveredAt = null,
        decimal? subtotal = null,
        decimal? shopVoucher = null)
    {
        var order = new Order
        {
            Id = id ?? Guid.NewGuid(),
            Status = status,
            GrossRevenue = grossRevenue,
            Channel = channel,
            PaymentMethod = paymentMethod,
            ExternalOrderId = externalOrderId,
            OrderDate = orderDate ?? DateTime.UtcNow,
            DeliveredAt = deliveredAt
        };

        if (subtotal.HasValue)
        {
            order.Subtotal = subtotal.Value;
            order.ShopVoucher = shopVoucher ?? 0m;
            order.GrossRevenue = grossRevenue > 0m ? grossRevenue : (order.Subtotal - order.ShopVoucher);
        }

        return order;
    }
}
