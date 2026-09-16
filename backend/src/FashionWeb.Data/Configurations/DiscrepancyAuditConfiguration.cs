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

        builder.Property(d => d.DiscrepancyAmount).HasPrecision(18, 0);
        builder.Property(d => d.ChannelOrderCode).HasMaxLength(64);
    }
}
