using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionWeb.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "products",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_products", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "fee_schedules",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                commission_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false, defaultValue: 0.0000m),
                payment_fee_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false, defaultValue: 0.0000m),
                service_fee_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false, defaultValue: 0.0000m),
                service_fee_cap = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                fixed_fee_per_order = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0.00m),
                effective_from = table.Column<DateOnly>(type: "date", nullable: false),
                effective_to = table.Column<DateOnly>(type: "date", nullable: true),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_fee_schedules", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "orders",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                external_order_id = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                channel = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                payment_method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                subtotal = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                shop_voucher = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0.00m),
                gross_revenue = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                customer_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                customer_phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                order_date = table.Column<DateTime>(type: "timestamptz", nullable: false),
                delivered_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                cancelled_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                cancellation_reason = table.Column<string>(type: "text", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_orders", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "product_variants",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                product_id = table.Column<Guid>(type: "uuid", nullable: false),
                sku_code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                color = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                size = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                retail_price = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                cost_price = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0.00m),
                is_active = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_product_variants", x => x.id);
                table.ForeignKey(
                    name: "FK_product_variants_products_product_id",
                    column: x => x.product_id,
                    principalTable: "products",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_status_history",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                from_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                to_status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                reason = table.Column<string>(type: "text", nullable: true),
                changed_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                changed_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_status_history", x => x.id);
                table.ForeignKey(
                    name: "FK_order_status_history_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "order_fee_snapshots",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                fee_schedule_id = table.Column<Guid>(type: "uuid", nullable: false),
                commission_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                commission_fee_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                payment_fee_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                payment_fee_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                service_fee_rate = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                service_fee_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                service_fee_cap_snapshot = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                fixed_fee_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                total_platform_fees = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                projected_settlement = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                snapshot_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_fee_snapshots", x => x.id);
                table.ForeignKey(
                    name: "FK_order_fee_snapshots_fee_schedules_fee_schedule_id",
                    column: x => x.fee_schedule_id,
                    principalTable: "fee_schedules",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_order_fee_snapshots_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "reconciliation_records",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                projected_settlement = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                actual_settlement = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                variance_amount = table.Column<decimal>(type: "numeric(15,2)", nullable: true),
                status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                reconciliation_notes = table.Column<string>(type: "text", nullable: true),
                reconciled_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                reconciled_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false),
                updated_at = table.Column<DateTime>(type: "timestamptz", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_reconciliation_records", x => x.id);
                table.ForeignKey(
                    name: "FK_reconciliation_records_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "order_items",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                order_id = table.Column<Guid>(type: "uuid", nullable: false),
                product_variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                sku_code_snapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                product_name_snapshot = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                quantity = table.Column<int>(type: "integer", nullable: false),
                unit_price = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                unit_cost_snapshot = table.Column<decimal>(type: "numeric(15,2)", nullable: false, defaultValue: 0.00m),
                line_total = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                total_cost = table.Column<decimal>(type: "numeric(15,2)", nullable: false),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_order_items", x => x.id);
                table.ForeignKey(
                    name: "FK_order_items_orders_order_id",
                    column: x => x.order_id,
                    principalTable: "orders",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_order_items_product_variants_product_variant_id",
                    column: x => x.product_variant_id,
                    principalTable: "product_variants",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "discrepancy_audits",
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                reconciliation_record_id = table.Column<Guid>(type: "uuid", nullable: false),
                discrepancy_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                explanation_note = table.Column<string>(type: "text", nullable: false),
                resolution_notes = table.Column<string>(type: "text", nullable: true),
                resolved_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                resolved_at = table.Column<DateTime>(type: "timestamptz", nullable: true),
                created_at = table.Column<DateTime>(type: "timestamptz", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_discrepancy_audits", x => x.id);
                table.ForeignKey(
                    name: "FK_discrepancy_audits_reconciliation_records_reconciliation_record_id",
                    column: x => x.reconciliation_record_id,
                    principalTable: "reconciliation_records",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_discrepancy_audits_reconciliation_record_id",
            table: "discrepancy_audits",
            column: "reconciliation_record_id");

        migrationBuilder.CreateIndex(
            name: "IX_fee_schedules_channel_payment_method",
            table: "fee_schedules",
            columns: new[] { "channel", "payment_method" },
            unique: true,
            filter: "\"is_active\" = true");

        migrationBuilder.CreateIndex(
            name: "IX_order_fee_snapshots_fee_schedule_id",
            table: "order_fee_snapshots",
            column: "fee_schedule_id");

        migrationBuilder.CreateIndex(
            name: "IX_order_fee_snapshots_order_id",
            table: "order_fee_snapshots",
            column: "order_id",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_order_items_order_id",
            table: "order_items",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "IX_order_items_product_variant_id",
            table: "order_items",
            column: "product_variant_id");

        migrationBuilder.CreateIndex(
            name: "IX_order_status_history_order_id",
            table: "order_status_history",
            column: "order_id");

        migrationBuilder.CreateIndex(
            name: "IX_orders_channel_external_order_id",
            table: "orders",
            columns: new[] { "channel", "external_order_id" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_product_variants_product_id",
            table: "product_variants",
            column: "product_id");

        migrationBuilder.CreateIndex(
            name: "IX_product_variants_sku_code",
            table: "product_variants",
            column: "sku_code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_reconciliation_records_order_id",
            table: "reconciliation_records",
            column: "order_id",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "discrepancy_audits");

        migrationBuilder.DropTable(
            name: "order_fee_snapshots");

        migrationBuilder.DropTable(
            name: "order_items");

        migrationBuilder.DropTable(
            name: "order_status_history");

        migrationBuilder.DropTable(
            name: "reconciliation_records");

        migrationBuilder.DropTable(
            name: "fee_schedules");

        migrationBuilder.DropTable(
            name: "product_variants");

        migrationBuilder.DropTable(
            name: "orders");

        migrationBuilder.DropTable(
            name: "products");
    }
}
