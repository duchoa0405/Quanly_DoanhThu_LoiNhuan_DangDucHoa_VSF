using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.ChannelOrderCode).IsRequired().HasMaxLength(64);
        builder.HasIndex(o => o.ChannelOrderCode).IsUnique();

        builder.Property(o => o.SubtotalAmount).HasPrecision(18, 0);
        builder.Property(o => o.ShopVoucherDiscount).HasPrecision(18, 0);
        builder.Property(o => o.PlatformVoucherSubsidy).HasPrecision(18, 0);
        builder.Property(o => o.ShippingFeeActual).HasPrecision(18, 0);
        builder.Property(o => o.NetCustomerPayment).HasPrecision(18, 0);

        builder.HasMany(o => o.Items).WithOne().HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(o => o.FeeSnapshot).WithOne().HasForeignKey<OrderFeeSnapshot>(s => s.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
