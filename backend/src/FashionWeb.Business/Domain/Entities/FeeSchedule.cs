using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Domain.Entities;

public class FeeSchedule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public ChannelType Channel { get; set; }
    public decimal CommissionRate { get; set; }
    public decimal PaymentFeeRate { get; set; }
    public decimal FixedFeePerOrder { get; set; }
    public decimal ServiceFeeRate { get; set; }
    public decimal ServiceFeeCap { get; set; }
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public bool IsActive { get; set; } = true;
}
