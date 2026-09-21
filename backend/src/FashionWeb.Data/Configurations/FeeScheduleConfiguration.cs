using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class FeeScheduleConfiguration : IEntityTypeConfiguration<FeeSchedule>
{
    public void Configure(EntityTypeBuilder<FeeSchedule> builder)
    {
        builder.ToTable("fee_schedules");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Id).HasColumnName("id");
        builder.Property(f => f.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(f => f.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(f => f.CommissionRate).HasColumnName("commission_rate").HasColumnType("numeric(6,4)").HasDefaultValue(0.0000m).IsRequired();
        builder.Property(f => f.PaymentFeeRate).HasColumnName("payment_fee_rate").HasColumnType("numeric(6,4)").HasDefaultValue(0.0000m).IsRequired();
        builder.Property(f => f.ServiceFeeRate).HasColumnName("service_fee_rate").HasColumnType("numeric(6,4)").HasDefaultValue(0.0000m).IsRequired();
        builder.Property(f => f.ServiceFeeCap).HasColumnName("service_fee_cap").HasColumnType("numeric(15,2)");
        builder.Property(f => f.FixedFeePerOrder).HasColumnName("fixed_fee_per_order").HasColumnType("numeric(15,2)").HasDefaultValue(0.00m).IsRequired();

        builder.Property(f => f.EffectiveFrom).HasColumnName("effective_from").HasColumnType("date").IsRequired();
        builder.Property(f => f.EffectiveTo).HasColumnName("effective_to").HasColumnType("date");
        builder.Property(f => f.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);

        builder.Property(f => f.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.HasIndex(f => new { f.Channel, f.PaymentMethod })
            .IsUnique()
            .HasFilter("\"is_active\" = true");
    }
}
