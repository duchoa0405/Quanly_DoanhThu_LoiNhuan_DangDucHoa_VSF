using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class FeeSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChannelType Channel { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal PaymentFeeRate { get; set; }
    public decimal ServiceFeeRate { get; set; }
    public decimal? ServiceFeeCap { get; set; }
    public decimal FixedFeePerOrder { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public void Deactivate(DateOnly effectiveTo)
    {
        IsActive = false;
        EffectiveTo = effectiveTo;
        UpdatedAt = DateTime.UtcNow;
    }
}
