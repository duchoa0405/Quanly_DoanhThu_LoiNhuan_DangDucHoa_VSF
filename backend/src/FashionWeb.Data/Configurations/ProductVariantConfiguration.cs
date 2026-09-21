using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure(EntityTypeBuilder<ProductVariant> builder)
    {
        builder.ToTable("product_variants");
        builder.HasKey(pv => pv.Id);

        builder.Property(pv => pv.Id).HasColumnName("id");
        builder.Property(pv => pv.ProductId).HasColumnName("product_id").IsRequired();
        builder.Property(pv => pv.SkuCode).HasColumnName("sku_code").HasMaxLength(100).IsRequired();
        builder.Property(pv => pv.Color).HasColumnName("color").HasMaxLength(50);
        builder.Property(pv => pv.Size).HasColumnName("size").HasMaxLength(20);
        builder.Property(pv => pv.RetailPrice).HasColumnName("retail_price").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(pv => pv.CostPrice).HasColumnName("cost_price").HasColumnType("numeric(15,2)").HasDefaultValue(0.00m).IsRequired();
        builder.Property(pv => pv.IsActive).HasColumnName("is_active").IsRequired().HasDefaultValue(true);
        builder.Property(pv => pv.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(pv => pv.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.HasIndex(pv => pv.SkuCode).IsUnique();
        builder.HasIndex(pv => pv.ProductId);
    }
}
