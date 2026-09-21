using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public OrderStatus? FromStatus { get; set; }
    public OrderStatus ToStatus { get; set; }
    public string? Reason { get; set; }
    public string ChangedBy { get; set; } = string.Empty;
    public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
}
