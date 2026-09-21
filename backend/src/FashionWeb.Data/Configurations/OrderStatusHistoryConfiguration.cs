using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class OrderStatusHistoryConfiguration : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.ToTable("order_status_history");
        builder.HasKey(osh => osh.Id);

        builder.Property(osh => osh.Id).HasColumnName("id");
        builder.Property(osh => osh.OrderId).HasColumnName("order_id").IsRequired();
        builder.Property(osh => osh.FromStatus).HasColumnName("from_status").HasConversion<string>().HasMaxLength(50);
        builder.Property(osh => osh.ToStatus).HasColumnName("to_status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(osh => osh.Reason).HasColumnName("reason").HasColumnType("text");
        builder.Property(osh => osh.ChangedBy).HasColumnName("changed_by").HasMaxLength(100).IsRequired();
        builder.Property(osh => osh.ChangedAt).HasColumnName("changed_at").HasColumnType("timestamptz").IsRequired();

        builder.HasIndex(osh => osh.OrderId);
    }
}
