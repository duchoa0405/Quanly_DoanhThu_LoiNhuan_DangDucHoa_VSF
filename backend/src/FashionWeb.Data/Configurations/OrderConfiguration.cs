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

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.ExternalOrderId).HasColumnName("external_order_id").HasMaxLength(100).IsRequired();
        builder.Property(o => o.Channel).HasColumnName("channel").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(o => o.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(o => o.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();

        builder.Property(o => o.Subtotal).HasColumnName("subtotal").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(o => o.ShopVoucher).HasColumnName("shop_voucher").HasColumnType("numeric(15,2)").HasDefaultValue(0.00m).IsRequired();
        builder.Property(o => o.GrossRevenue).HasColumnName("gross_revenue").HasColumnType("numeric(15,2)").IsRequired();

        builder.Property(o => o.CustomerName).HasColumnName("customer_name").HasMaxLength(255);
        builder.Property(o => o.CustomerPhone).HasColumnName("customer_phone").HasMaxLength(20);

        builder.Property(o => o.OrderDate).HasColumnName("order_date").HasColumnType("timestamptz").IsRequired();
        builder.Property(o => o.DeliveredAt).HasColumnName("delivered_at").HasColumnType("timestamptz");
        builder.Property(o => o.CancelledAt).HasColumnName("cancelled_at").HasColumnType("timestamptz");
        builder.Property(o => o.CancellationReason).HasColumnName("cancellation_reason").HasColumnType("text");

        builder.Property(o => o.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(o => o.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.HasIndex(o => new { o.Channel, o.ExternalOrderId }).IsUnique();

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(sh => sh.Order)
            .HasForeignKey(sh => sh.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.FeeSnapshot)
            .WithOne(fs => fs.Order)
            .HasForeignKey<OrderFeeSnapshot>(fs => fs.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.ReconciliationRecord)
            .WithOne(r => r.Order)
            .HasForeignKey<ReconciliationRecord>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
