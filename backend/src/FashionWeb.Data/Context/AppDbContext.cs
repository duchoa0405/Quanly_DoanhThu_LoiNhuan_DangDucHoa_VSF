using FashionWeb.Business.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductVariant> ProductVariants => Set<ProductVariant>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<FeeSchedule> FeeSchedules => Set<FeeSchedule>();
    public DbSet<OrderFeeSnapshot> OrderFeeSnapshots => Set<OrderFeeSnapshot>();
    public DbSet<ReconciliationRecord> ReconciliationRecords => Set<ReconciliationRecord>();
    public DbSet<DiscrepancyAudit> DiscrepancyAudits => Set<DiscrepancyAudit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
