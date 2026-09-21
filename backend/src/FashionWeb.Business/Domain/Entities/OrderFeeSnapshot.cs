namespace FashionWeb.Business.Domain.Entities;

public class OrderFeeSnapshot
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid FeeScheduleId { get; set; }

    public decimal CommissionFeeRate { get; set; }
    public decimal CommissionFeeAmount { get; set; }
    public decimal PaymentFeeRate { get; set; }
    public decimal PaymentFeeAmount { get; set; }
    public decimal ServiceFeeRate { get; set; }
    public decimal ServiceFeeAmount { get; set; }
    public decimal? ServiceFeeCapSnapshot { get; set; }
    public decimal FixedFeeAmount { get; set; }

    public decimal TotalPlatformFees { get; set; }
    public decimal ProjectedSettlement { get; set; }
    public DateTime SnapshotAt { get; set; } = DateTime.UtcNow;

    public Order? Order { get; set; }
    public FeeSchedule? FeeSchedule { get; set; }
}
