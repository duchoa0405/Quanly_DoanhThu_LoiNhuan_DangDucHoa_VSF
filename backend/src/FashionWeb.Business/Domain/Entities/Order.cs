using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ExternalOrderId { get; set; } = string.Empty;
    public ChannelType Channel { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.PENDING;
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }

    public decimal Subtotal { get; set; }
    public decimal ShopVoucher { get; set; }
    public decimal GrossRevenue { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancellationReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public List<OrderStatusHistory> StatusHistory { get; set; } = new();
    public OrderFeeSnapshot? FeeSnapshot { get; set; }
    public ReconciliationRecord? ReconciliationRecord { get; set; }

    public void SetFinancials(decimal subtotal, decimal shopVoucher)
    {
        if (subtotal < 0)
            throw new ArgumentException("Subtotal cannot be negative.", nameof(subtotal));
        if (shopVoucher < 0)
            throw new ArgumentException("Shop voucher discount cannot be negative.", nameof(shopVoucher));
        if (shopVoucher > subtotal)
            throw new ArgumentException("Shop voucher cannot exceed subtotal.", nameof(shopVoucher));

        Subtotal = subtotal;
        ShopVoucher = shopVoucher;
        GrossRevenue = subtotal - shopVoucher;
        UpdatedAt = DateTime.UtcNow;
    }

    public void TransitionToShipped(string actorIdentity)
    {
        if (Status != OrderStatus.PENDING)
            throw new InvalidOperationException($"Cannot transition to SHIPPED from status {Status}.");

        var prior = Status;
        Status = OrderStatus.SHIPPED;
        UpdatedAt = DateTime.UtcNow;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.SHIPPED,
            ChangedBy = actorIdentity,
            ChangedAt = DateTime.UtcNow
        });
    }

    public void TransitionToDelivered(string actorIdentity)
    {
        if (Status != OrderStatus.SHIPPED)
            throw new InvalidOperationException($"Cannot transition to DELIVERED from status {Status}. Order must be in SHIPPED status.");

        var prior = Status;
        Status = OrderStatus.DELIVERED;
        DeliveredAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.DELIVERED,
            ChangedBy = actorIdentity,
            ChangedAt = DateTime.UtcNow
        });
    }

    public void Cancel(string reason, string actorIdentity)
    {
        if (Status == OrderStatus.DELIVERED)
            throw new InvalidOperationException("Cannot cancel an order that has already been DELIVERED.");
        if (Status == OrderStatus.CANCELLED)
            throw new InvalidOperationException("Order is already CANCELLED.");

        var prior = Status;
        Status = OrderStatus.CANCELLED;
        CancelledAt = DateTime.UtcNow;
        CancellationReason = reason;
        UpdatedAt = DateTime.UtcNow;

        StatusHistory.Add(new OrderStatusHistory
        {
            OrderId = Id,
            FromStatus = prior,
            ToStatus = OrderStatus.CANCELLED,
            Reason = reason,
            ChangedBy = actorIdentity,
            ChangedAt = DateTime.UtcNow
        });
    }
}
