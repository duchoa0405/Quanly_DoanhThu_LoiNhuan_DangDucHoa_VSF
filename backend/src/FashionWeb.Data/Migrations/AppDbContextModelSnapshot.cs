using System;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace FashionWeb.Data.Migrations;

[DbContext(typeof(AppDbContext))]
partial class AppDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasAnnotation("ProductVersion", "8.0.0")
            .HasAnnotation("Relational:MaxIdentifierLength", 63);

        NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.DiscrepancyAudit", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<string>("DiscrepancyType")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("discrepancy_type");

                b.Property<string>("ExplanationNote")
                    .IsRequired()
                    .HasColumnType("text")
                    .HasColumnName("explanation_note");

                b.Property<Guid>("ReconciliationId")
                    .HasColumnType("uuid")
                    .HasColumnName("reconciliation_record_id");

                b.Property<string>("ResolutionNotes")
                    .HasColumnType("text")
                    .HasColumnName("resolution_notes");

                b.Property<DateTime?>("ResolvedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("resolved_at");

                b.Property<string>("ResolvedBy")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("resolved_by");

                b.HasKey("Id");

                b.HasIndex("ReconciliationId");

                b.ToTable("discrepancy_audits", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.FeeSchedule", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("Channel")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("channel");

                b.Property<decimal>("CommissionRate")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(6,4)")
                    .HasDefaultValue(0.0000m)
                    .HasColumnName("commission_rate");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<DateOnly>("EffectiveFrom")
                    .HasColumnType("date")
                    .HasColumnName("effective_from");

                b.Property<DateOnly?>("EffectiveTo")
                    .HasColumnType("date")
                    .HasColumnName("effective_to");

                b.Property<decimal>("FixedFeePerOrder")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(15,2)")
                    .HasDefaultValue(0.00m)
                    .HasColumnName("fixed_fee_per_order");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("boolean")
                    .HasDefaultValue(true)
                    .HasColumnName("is_active");

                b.Property<string>("PaymentMethod")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("payment_method");

                b.Property<decimal>("PaymentFeeRate")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(6,4)")
                    .HasDefaultValue(0.0000m)
                    .HasColumnName("payment_fee_rate");

                b.Property<decimal?>("ServiceFeeCap")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("service_fee_cap");

                b.Property<decimal>("ServiceFeeRate")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(6,4)")
                    .HasDefaultValue(0.0000m)
                    .HasColumnName("service_fee_rate");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("updated_at");

                b.HasKey("Id");

                b.HasIndex("Channel", "PaymentMethod")
                    .IsUnique()
                    .HasFilter("\"is_active\" = true");

                b.ToTable("fee_schedules", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.Order", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<DateTime?>("CancelledAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("cancelled_at");

                b.Property<string>("CancellationReason")
                    .HasColumnType("text")
                    .HasColumnName("cancellation_reason");

                b.Property<string>("Channel")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("channel");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<string>("CustomerName")
                    .HasMaxLength(255)
                    .HasColumnType("character varying(255)")
                    .HasColumnName("customer_name");

                b.Property<string>("CustomerPhone")
                    .HasMaxLength(20)
                    .HasColumnType("character varying(20)")
                    .HasColumnName("customer_phone");

                b.Property<DateTime?>("DeliveredAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("delivered_at");

                b.Property<string>("ExternalOrderId")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("external_order_id");

                b.Property<decimal>("GrossRevenue")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("gross_revenue");

                b.Property<DateTime>("OrderDate")
                    .HasColumnType("timestamptz")
                    .HasColumnName("order_date");

                b.Property<string>("PaymentMethod")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("payment_method");

                b.Property<decimal>("ShopVoucher")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(15,2)")
                    .HasDefaultValue(0.00m)
                    .HasColumnName("shop_voucher");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("status");

                b.Property<decimal>("Subtotal")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("subtotal");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("updated_at");

                b.HasKey("Id");

                b.HasIndex("Channel", "ExternalOrderId")
                    .IsUnique();

                b.ToTable("orders", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderFeeSnapshot", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<decimal>("CommissionFeeAmount")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("commission_fee_amount");

                b.Property<decimal>("CommissionFeeRate")
                    .HasColumnType("numeric(6,4)")
                    .HasColumnName("commission_rate");

                b.Property<Guid>("FeeScheduleId")
                    .HasColumnType("uuid")
                    .HasColumnName("fee_schedule_id");

                b.Property<decimal>("FixedFeeAmount")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("fixed_fee_amount");

                b.Property<Guid>("OrderId")
                    .HasColumnType("uuid")
                    .HasColumnName("order_id");

                b.Property<decimal>("PaymentFeeAmount")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("payment_fee_amount");

                b.Property<decimal>("PaymentFeeRate")
                    .HasColumnType("numeric(6,4)")
                    .HasColumnName("payment_fee_rate");

                b.Property<decimal>("ProjectedSettlement")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("projected_settlement");

                b.Property<decimal>("ServiceFeeAmount")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("service_fee_amount");

                b.Property<decimal?>("ServiceFeeCapSnapshot")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("service_fee_cap_snapshot");

                b.Property<decimal>("ServiceFeeRate")
                    .HasColumnType("numeric(6,4)")
                    .HasColumnName("service_fee_rate");

                b.Property<DateTime>("SnapshotAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("snapshot_at");

                b.Property<decimal>("TotalPlatformFees")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("total_platform_fees");

                b.HasKey("Id");

                b.HasIndex("FeeScheduleId");

                b.HasIndex("OrderId")
                    .IsUnique();

                b.ToTable("order_fee_snapshots", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderItem", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<decimal>("LineTotal")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("line_total");

                b.Property<Guid>("OrderId")
                    .HasColumnType("uuid")
                    .HasColumnName("order_id");

                b.Property<string>("ProductNameSnapshot")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("character varying(255)")
                    .HasColumnName("product_name_snapshot");

                b.Property<Guid>("ProductVariantId")
                    .HasColumnType("uuid")
                    .HasColumnName("product_variant_id");

                b.Property<int>("Quantity")
                    .HasColumnType("integer")
                    .HasColumnName("quantity");

                b.Property<string>("SkuCodeSnapshot")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("sku_code_snapshot");

                b.Property<decimal>("TotalCost")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("total_cost");

                b.Property<decimal>("UnitCostSnapshot")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(15,2)")
                    .HasDefaultValue(0.00m)
                    .HasColumnName("unit_cost_snapshot");

                b.Property<decimal>("UnitPrice")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("unit_price");

                b.HasKey("Id");

                b.HasIndex("OrderId");

                b.HasIndex("ProductVariantId");

                b.ToTable("order_items", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderStatusHistory", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<DateTime>("ChangedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("changed_at");

                b.Property<string>("ChangedBy")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("changed_by");

                b.Property<string>("FromStatus")
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("from_status");

                b.Property<Guid>("OrderId")
                    .HasColumnType("uuid")
                    .HasColumnName("order_id");

                b.Property<string>("Reason")
                    .HasColumnType("text")
                    .HasColumnName("reason");

                b.Property<string>("ToStatus")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("to_status");

                b.HasKey("Id");

                b.HasIndex("OrderId");

                b.ToTable("order_status_history", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.Product", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("Category")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("category");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("boolean")
                    .HasDefaultValue(true)
                    .HasColumnName("is_active");

                b.Property<string>("Name")
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnType("character varying(255)")
                    .HasColumnName("name");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("updated_at");

                b.HasKey("Id");

                b.ToTable("products", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.ProductVariant", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<string>("Color")
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("color");

                b.Property<decimal>("CostPrice")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("numeric(15,2)")
                    .HasDefaultValue(0.00m)
                    .HasColumnName("cost_price");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<bool>("IsActive")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("boolean")
                    .HasDefaultValue(true)
                    .HasColumnName("is_active");

                b.Property<Guid>("ProductId")
                    .HasColumnType("uuid")
                    .HasColumnName("product_id");

                b.Property<decimal>("RetailPrice")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("retail_price");

                b.Property<string>("Size")
                    .HasMaxLength(20)
                    .HasColumnType("character varying(20)")
                    .HasColumnName("size");

                b.Property<string>("SkuCode")
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("sku_code");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("updated_at");

                b.HasKey("Id");

                b.HasIndex("ProductId");

                b.HasIndex("SkuCode")
                    .IsUnique();

                b.ToTable("product_variants", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.ReconciliationRecord", b =>
            {
                b.Property<Guid>("Id")
                    .ValueGeneratedOnAdd()
                    .HasColumnType("uuid")
                    .HasColumnName("id");

                b.Property<decimal?>("ActualSettlement")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("actual_settlement");

                b.Property<DateTime>("CreatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("created_at");

                b.Property<Guid>("OrderId")
                    .HasColumnType("uuid")
                    .HasColumnName("order_id");

                b.Property<decimal>("ProjectedSettlement")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("projected_settlement");

                b.Property<string>("ReconciliationNotes")
                    .HasColumnType("text")
                    .HasColumnName("reconciliation_notes");

                b.Property<DateTime?>("ReconciledAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("reconciled_at");

                b.Property<string>("ReconciledBy")
                    .HasMaxLength(100)
                    .HasColumnType("character varying(100)")
                    .HasColumnName("reconciled_by");

                b.Property<string>("Status")
                    .IsRequired()
                    .HasMaxLength(50)
                    .HasColumnType("character varying(50)")
                    .HasColumnName("status");

                b.Property<DateTime?>("UpdatedAt")
                    .HasColumnType("timestamptz")
                    .HasColumnName("updated_at");

                b.Property<decimal?>("VarianceAmount")
                    .HasColumnType("numeric(15,2)")
                    .HasColumnName("variance_amount");

                b.HasKey("Id");

                b.HasIndex("OrderId")
                    .IsUnique();

                b.ToTable("reconciliation_records", (string)null);
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderFeeSnapshot", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.FeeSchedule", "FeeSchedule")
                    .WithMany()
                    .HasForeignKey("FeeScheduleId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.HasOne("FashionWeb.Business.Domain.Entities.Order", "Order")
                    .WithOne("FeeSnapshot")
                    .HasForeignKey("FashionWeb.Business.Domain.Entities.OrderFeeSnapshot", "OrderId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("FeeSchedule");

                b.Navigation("Order");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderItem", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.Order", "Order")
                    .WithMany("Items")
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.HasOne("FashionWeb.Business.Domain.Entities.ProductVariant", "ProductVariant")
                    .WithMany()
                    .HasForeignKey("ProductVariantId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Order");

                b.Navigation("ProductVariant");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.OrderStatusHistory", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.Order", "Order")
                    .WithMany("StatusHistory")
                    .HasForeignKey("OrderId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("Order");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.ProductVariant", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.Product", "Product")
                    .WithMany("Variants")
                    .HasForeignKey("ProductId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Product");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.ReconciliationRecord", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.Order", "Order")
                    .WithOne("ReconciliationRecord")
                    .HasForeignKey("FashionWeb.Business.Domain.Entities.ReconciliationRecord", "OrderId")
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired();

                b.Navigation("Order");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.DiscrepancyAudit", b =>
            {
                b.HasOne("FashionWeb.Business.Domain.Entities.ReconciliationRecord", "ReconciliationRecord")
                    .WithMany("Audits")
                    .HasForeignKey("ReconciliationId")
                    .OnDelete(DeleteBehavior.Cascade)
                    .IsRequired();

                b.Navigation("ReconciliationRecord");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.Order", b =>
            {
                b.Navigation("FeeSnapshot");

                b.Navigation("Items");

                b.Navigation("ReconciliationRecord");

                b.Navigation("StatusHistory");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.Product", b =>
            {
                b.Navigation("Variants");
            });

        modelBuilder.Entity("FashionWeb.Business.Domain.Entities.ReconciliationRecord", b =>
            {
                b.Navigation("Audits");
            });
#pragma warning restore 612, 618
    }
}
