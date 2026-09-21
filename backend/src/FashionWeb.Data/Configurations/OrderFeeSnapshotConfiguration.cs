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

        builder.Property(s => s.Id).HasColumnName("id");
        builder.Property(s => s.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(s => s.FeeScheduleId).HasColumnName("fee_schedule_id").IsRequired();

        builder.Property(s => s.CommissionFeeRate).HasColumnName("commission_rate").HasColumnType("numeric(6,4)").IsRequired();
        builder.Property(s => s.CommissionFeeAmount).HasColumnName("commission_fee_amount").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.PaymentFeeRate).HasColumnName("payment_fee_rate").HasColumnType("numeric(6,4)").IsRequired();
        builder.Property(s => s.PaymentFeeAmount).HasColumnName("payment_fee_amount").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.ServiceFeeRate).HasColumnName("service_fee_rate").HasColumnType("numeric(6,4)").IsRequired();
        builder.Property(s => s.ServiceFeeAmount).HasColumnName("service_fee_amount").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.ServiceFeeCapSnapshot).HasColumnName("service_fee_cap_snapshot").HasColumnType("numeric(15,2)");
        builder.Property(s => s.FixedFeeAmount).HasColumnName("fixed_fee_amount").HasColumnType("numeric(15,2)").IsRequired();

        builder.Property(s => s.TotalPlatformFees).HasColumnName("total_platform_fees").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.ProjectedSettlement).HasColumnName("projected_settlement").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(s => s.SnapshotAt).HasColumnName("snapshot_at").HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(s => s.OrderId).IsUnique();
        builder.HasIndex(s => s.FeeScheduleId);

        builder.HasOne(s => s.Order)
            .WithOne(o => o.FeeSnapshot)
            .HasForeignKey<OrderFeeSnapshot>(s => s.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.FeeSchedule)
            .WithMany()
            .HasForeignKey(s => s.FeeScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
