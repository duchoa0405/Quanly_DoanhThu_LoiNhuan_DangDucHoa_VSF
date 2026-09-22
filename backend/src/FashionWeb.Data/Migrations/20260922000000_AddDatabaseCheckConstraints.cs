using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FashionWeb.Data.Migrations;

/// <inheritdoc />
public partial class AddDatabaseCheckConstraints : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // 1. Orders table invariants
        migrationBuilder.Sql(@"
            ALTER TABLE orders 
                ADD CONSTRAINT CK_orders_subtotal_non_negative CHECK (subtotal >= 0),
                ADD CONSTRAINT CK_orders_shop_voucher_non_negative CHECK (shop_voucher >= 0),
                ADD CONSTRAINT CK_orders_shop_voucher_lte_subtotal CHECK (shop_voucher <= subtotal),
                ADD CONSTRAINT CK_orders_gross_revenue_equation CHECK (gross_revenue = subtotal - shop_voucher),
                ADD CONSTRAINT CK_orders_channel_payment CHECK (
                    (channel IN ('TIKTOK', 'SHOPEE') AND payment_method = 'MARKETPLACE_WALLET') OR
                    (channel = 'POS' AND payment_method IN ('CASH', 'POS_CARD_QR'))
                ),
                ADD CONSTRAINT CK_orders_lifecycle_delivered CHECK (status != 'DELIVERED' OR delivered_at IS NOT NULL),
                ADD CONSTRAINT CK_orders_lifecycle_cancelled CHECK (status != 'CANCELLED' OR cancelled_at IS NOT NULL);
        ");

        // 2. Product variants pricing constraints
        migrationBuilder.Sql(@"
            ALTER TABLE product_variants
                ADD CONSTRAINT CK_product_variants_cost_price CHECK (cost_price >= 0),
                ADD CONSTRAINT CK_product_variants_retail_price CHECK (retail_price >= 0);
        ");

        // 3. Order items quantity and pricing integrity
        migrationBuilder.Sql(@"
            ALTER TABLE order_items
                ADD CONSTRAINT CK_order_items_quantity CHECK (quantity > 0),
                ADD CONSTRAINT CK_order_items_unit_price CHECK (unit_price >= 0),
                ADD CONSTRAINT CK_order_items_unit_cost CHECK (unit_cost_snapshot >= 0),
                ADD CONSTRAINT CK_order_items_line_total CHECK (line_total = quantity * unit_price),
                ADD CONSTRAINT CK_order_items_total_cost CHECK (total_cost = quantity * unit_cost_snapshot);
        ");

        // 4. Fee schedules rate range [0, 1] and effective date integrity
        migrationBuilder.Sql(@"
            ALTER TABLE fee_schedules
                ADD CONSTRAINT CK_fee_schedules_commission_rate CHECK (commission_rate >= 0 AND commission_rate <= 1.0),
                ADD CONSTRAINT CK_fee_schedules_payment_fee_rate CHECK (payment_fee_rate >= 0 AND payment_fee_rate <= 1.0),
                ADD CONSTRAINT CK_fee_schedules_service_fee_rate CHECK (service_fee_rate >= 0 AND service_fee_rate <= 1.0),
                ADD CONSTRAINT CK_fee_schedules_fixed_fee CHECK (fixed_fee_per_order >= 0),
                ADD CONSTRAINT CK_fee_schedules_service_cap CHECK (service_fee_cap IS NULL OR service_fee_cap >= 0),
                ADD CONSTRAINT CK_fee_schedules_effective_dates CHECK (effective_to IS NULL OR effective_to >= effective_from);
        ");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(@"
            ALTER TABLE orders 
                DROP CONSTRAINT IF EXISTS CK_orders_subtotal_non_negative,
                DROP CONSTRAINT IF EXISTS CK_orders_shop_voucher_non_negative,
                DROP CONSTRAINT IF EXISTS CK_orders_shop_voucher_lte_subtotal,
                DROP CONSTRAINT IF EXISTS CK_orders_gross_revenue_equation,
                DROP CONSTRAINT IF EXISTS CK_orders_channel_payment,
                DROP CONSTRAINT IF EXISTS CK_orders_lifecycle_delivered,
                DROP CONSTRAINT IF EXISTS CK_orders_lifecycle_cancelled;

            ALTER TABLE product_variants
                DROP CONSTRAINT IF EXISTS CK_product_variants_cost_price,
                DROP CONSTRAINT IF EXISTS CK_product_variants_retail_price;

            ALTER TABLE order_items
                DROP CONSTRAINT IF EXISTS CK_order_items_quantity,
                DROP CONSTRAINT IF EXISTS CK_order_items_unit_price,
                DROP CONSTRAINT IF EXISTS CK_order_items_unit_cost,
                DROP CONSTRAINT IF EXISTS CK_order_items_line_total,
                DROP CONSTRAINT IF EXISTS CK_order_items_total_cost;

            ALTER TABLE fee_schedules
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_commission_rate,
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_payment_fee_rate,
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_service_fee_rate,
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_fixed_fee,
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_service_cap,
                DROP CONSTRAINT IF EXISTS CK_fee_schedules_effective_dates;
        ");
    }
}
