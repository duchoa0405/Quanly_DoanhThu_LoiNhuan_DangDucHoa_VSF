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

        builder.Property(f => f.CommissionRate).HasPrecision(5, 4);
        builder.Property(f => f.PaymentFeeRate).HasPrecision(5, 4);
        builder.Property(f => f.FixedFeePerOrder).HasPrecision(18, 0);
        builder.Property(f => f.ServiceFeeRate).HasPrecision(5, 4);
        builder.Property(f => f.ServiceFeeCap).HasPrecision(18, 0);
    }
}
