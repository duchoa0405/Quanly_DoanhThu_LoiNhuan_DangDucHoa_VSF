using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string ChannelOrderCode { get; set; } = string.Empty;
    public ChannelType Channel { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public PaymentMethod PaymentMethod { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;

    public decimal SubtotalAmount { get; set; }
    public decimal ShopVoucherDiscount { get; set; }
    public decimal PlatformVoucherSubsidy { get; set; }
    public decimal ShippingFeeActual { get; set; }
    public decimal NetCustomerPayment { get; set; }

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? DeliveredDate { get; set; }
    public DateTime? CancelledDate { get; set; }
    public string? CancellationReason { get; set; }

    public List<OrderItem> Items { get; set; } = new();
    public OrderFeeSnapshot? FeeSnapshot { get; set; }
}
