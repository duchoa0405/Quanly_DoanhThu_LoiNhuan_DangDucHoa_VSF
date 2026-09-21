using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(oi => oi.Id);

        builder.Property(oi => oi.Id).HasColumnName("id");
        builder.Property(oi => oi.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(oi => oi.ProductVariantId).HasColumnName("product_variant_id").IsRequired();
        builder.Property(oi => oi.SkuCodeSnapshot).HasColumnName("sku_code_snapshot").HasMaxLength(100).IsRequired();
        builder.Property(oi => oi.ProductNameSnapshot).HasColumnName("product_name_snapshot").HasMaxLength(255).IsRequired();
        builder.Property(oi => oi.Quantity).HasColumnName("quantity").IsRequired();
        builder.Property(oi => oi.UnitPrice).HasColumnName("unit_price").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(oi => oi.UnitCostSnapshot).HasColumnName("unit_cost_snapshot").HasColumnType("numeric(15,2)").HasDefaultValue(0.00m).IsRequired();
        builder.Property(oi => oi.LineTotal).HasColumnName("line_total").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(oi => oi.TotalCost).HasColumnName("total_cost").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(oi => oi.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();

        builder.HasOne(oi => oi.ProductVariant)
            .WithMany()
            .HasForeignKey(oi => oi.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(oi => oi.OrderId);
        builder.HasIndex(oi => oi.ProductVariantId);
    }
}
