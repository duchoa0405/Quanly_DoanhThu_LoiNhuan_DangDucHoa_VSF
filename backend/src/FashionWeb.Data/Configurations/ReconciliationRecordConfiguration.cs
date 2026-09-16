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

        builder.Property(r => r.ExpectedPayout).HasPrecision(18, 0);
        builder.Property(r => r.ActualPayout).HasPrecision(18, 0);
    }
}
