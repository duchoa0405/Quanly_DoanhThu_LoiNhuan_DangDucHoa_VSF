using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class ReconciliationRecordConfiguration : IEntityTypeConfiguration<ReconciliationRecord>
{
    public void Configure(EntityTypeBuilder<ReconciliationRecord> builder)
    {
        builder.ToTable("reconciliation_records");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.OrderId).HasColumnName("order_id").IsRequired();

        builder.Property(r => r.ProjectedSettlement).HasColumnName("projected_settlement").HasColumnType("numeric(15,2)").IsRequired();
        builder.Property(r => r.ActualSettlement).HasColumnName("actual_settlement").HasColumnType("numeric(15,2)");
        builder.Property(r => r.VarianceAmount).HasColumnName("variance_amount").HasColumnType("numeric(15,2)");

        builder.Property(r => r.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(r => r.ReconciliationNotes).HasColumnName("reconciliation_notes").HasColumnType("text");

        builder.Property(r => r.ReconciledAt).HasColumnName("reconciled_at").HasColumnType("timestamptz");
        builder.Property(r => r.ReconciledBy).HasColumnName("reconciled_by").HasMaxLength(100);

        builder.Property(r => r.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();
        builder.Property(r => r.UpdatedAt).HasColumnName("updated_at").HasColumnType("timestamptz");

        builder.HasIndex(r => r.OrderId).IsUnique();

        builder.HasOne(r => r.Order)
            .WithOne(o => o.ReconciliationRecord)
            .HasForeignKey<ReconciliationRecord>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Audits)
            .WithOne(a => a.ReconciliationRecord)
            .HasForeignKey(a => a.ReconciliationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
