using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class OrderFeeSnapshotConfiguration : IEntityTypeConfiguration<OrderFeeSnapshot>
{
    public void Configure(EntityTypeBuilder<OrderFeeSnapshot> builder)
    {
        builder.ToTable("order_fee_snapshots");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.CommissionFeeRate).HasPrecision(5, 4);
        builder.Property(s => s.CommissionFeeAmount).HasPrecision(18, 0);
        builder.Property(s => s.PaymentFeeRate).HasPrecision(5, 4);
        builder.Property(s => s.PaymentFeeAmount).HasPrecision(18, 0);
        builder.Property(s => s.FixedFeeAmount).HasPrecision(18, 0);
        builder.Property(s => s.ServiceFeeAmount).HasPrecision(18, 0);
        builder.Property(s => s.TotalPlatformFees).HasPrecision(18, 0);
        builder.Property(s => s.ExpectedNetPayout).HasPrecision(18, 0);
    }
}
