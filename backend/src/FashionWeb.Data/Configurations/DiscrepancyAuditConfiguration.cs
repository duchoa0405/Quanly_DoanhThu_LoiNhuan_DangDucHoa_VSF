using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FashionWeb.Data.Configurations;

public class DiscrepancyAuditConfiguration : IEntityTypeConfiguration<DiscrepancyAudit>
{
    public void Configure(EntityTypeBuilder<DiscrepancyAudit> builder)
    {
        builder.ToTable("discrepancy_audits");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Id).HasColumnName("id");
        builder.Property(d => d.ReconciliationId).HasColumnName("reconciliation_record_id").IsRequired();

        builder.Property(d => d.DiscrepancyType).HasColumnName("discrepancy_type").HasConversion<string>().HasMaxLength(50).IsRequired();
        builder.Property(d => d.ExplanationNote).HasColumnName("explanation_note").HasColumnType("text").IsRequired();
        builder.Property(d => d.ResolutionNotes).HasColumnName("resolution_notes").HasColumnType("text");

        builder.Property(d => d.ResolvedBy).HasColumnName("resolved_by").HasMaxLength(100);
        builder.Property(d => d.ResolvedAt).HasColumnName("resolved_at").HasColumnType("timestamptz");

        builder.Property(d => d.CreatedAt).HasColumnName("created_at").HasColumnType("timestamptz").IsRequired();

        builder.Ignore(d => d.IsResolved);

        builder.HasIndex(d => d.ReconciliationId);
    }
}
