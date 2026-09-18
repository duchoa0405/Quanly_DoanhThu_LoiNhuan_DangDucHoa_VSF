# Database Constraints, Integrity Rules & Indexing Specification

> **System:** Fashion Revenue & Profit Management System  
> **Database:** PostgreSQL 16+  
> **Document Role:** Physical Database Integrity Specification (Phase P05 - Database Design)  
> **Source of Truth for Relational Schema:** [`schema.dbml`](./schema.dbml)  
> **Companion Document for Business Rationale:** [`database-design_en.md`](./database-design_en.md)

---

## 1. Architectural Scope & Purpose

This specification establishes the authoritative **physical integrity constraints, state-machine validation rules, immutability policies, indexing architecture, and transaction boundaries** for the 9 core tables within the **Fashion Revenue & Profit Management System**:

1. `products` (Master product models)
2. `product_variants` (Sellable SKUs with retail price & baseline unit cost)
3. `orders` (Multi-channel commercial sales orders)
4. `order_items` (Order line items with immutable unit cost snapshots)
5. `order_status_history` (Append-only lifecycle audit trail)
6. `fee_schedules` (Versioned platform fee policy matrix)
7. `order_fee_snapshots` (Immutable platform fee deductions frozen upon delivery)
8. `reconciliation_records` (Manual wallet payout reconciliation ledger)
9. `discrepancy_audits` (Investigation log for reconciliation variances)

### 1.1 Strict Boundary Definition: Database vs. Application Layer
- **Database Layer (PostgreSQL 16+):** Enforces **valid stored states**, data invariants, relational cardinality, snapshot immutability, mathematical integrity, and concurrent transaction isolation.
- **Application Layer (ASP.NET Core Backend API):** Enforces **legal state transitions** (e.g., validating that an order transitions only from `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`), orchestrates Strategy Pattern fee evaluations, and mediates user authentication and role-based authorization.

---

## 2. Table Constraints & Integrity Rules (DDL Specifications)

### 2.1 Table: `products`
Master apparel catalog definition. Variations are modeled in `product_variants`.

```sql
-- Core Table Constraints
ALTER TABLE products
    ADD CONSTRAINT chk_products_name_not_empty 
        CHECK (trim(name) <> '');

-- Default Constraint
ALTER TABLE products 
    ALTER COLUMN is_active SET DEFAULT true;
```

- `name` (`varchar(255)`, NOT NULL): Must not be blank or whitespace-only.
- `category` (`varchar(100)`, NULL): Optional string attribute classifying apparel (e.g., `Shirts`, `Pants`). A separate categories table is deliberately omitted from MVP scope.
- `is_active` (`boolean`, NOT NULL DEFAULT true): Controls catalog visibility. Soft-delete flag; hard deletions of master products are prohibited if child variants exist.

---

### 2.2 Table: `product_variants`
Specific sellable SKU variant with physical attributes, retail selling price, and baseline unit cost.

```sql
-- Uniqueness & Non-Empty SKU
ALTER TABLE product_variants
    ADD CONSTRAINT uq_product_variants_sku_code 
        UNIQUE (sku_code);

ALTER TABLE product_variants
    ADD CONSTRAINT chk_product_variants_sku_not_empty 
        CHECK (trim(sku_code) <> '');

-- Non-Negative Pricing & Cost Baseline
ALTER TABLE product_variants
    ADD CONSTRAINT chk_product_variants_retail_price_non_negative 
        CHECK (retail_price >= 0);

ALTER TABLE product_variants
    ADD CONSTRAINT chk_product_variants_cost_price_non_negative 
        CHECK (cost_price >= 0);

-- Foreign Key Constraint (RESTRICT Deletion)
ALTER TABLE product_variants
    ADD CONSTRAINT fk_product_variants_product_id 
        FOREIGN KEY (product_id) REFERENCES products(id) 
        ON DELETE RESTRICT;
```

- `sku_code` (`varchar(100)`, NOT NULL): Unique stock-keeping unit code across the entire catalog.
  > [!NOTE]
  > `UNIQUE(sku_code)` automatically creates a unique B-tree index in PostgreSQL. An additional explicit index on `sku_code` is redundant and omitted.
- `retail_price` (`numeric(15, 2)`, NOT NULL): Standard customer selling price in VND. Must be $\ge 0$.
- `cost_price` (`numeric(15, 2)`, NOT NULL DEFAULT 0): Baseline unit Cost of Goods Sold (COGS) in VND manually maintained by `Shop Owner` or `Finance Manager`. Must be $\ge 0$.
- `product_id`: `ON DELETE RESTRICT` prevents accidental deletion of a master product if active variants exist.

---

### 2.3 Table: `orders`
Master multi-channel commercial sales orders.

```sql
-- External Order ID Integrity & Channel-Level Uniqueness
ALTER TABLE orders
    ALTER COLUMN external_order_id SET NOT NULL;

ALTER TABLE orders
    ADD CONSTRAINT chk_orders_external_id_not_empty 
        CHECK (trim(external_order_id) <> '');

ALTER TABLE orders
    ADD CONSTRAINT uq_orders_channel_external_id 
        UNIQUE (channel, external_order_id);

-- Monetary Integrity Checks
ALTER TABLE orders
    ADD CONSTRAINT chk_orders_subtotal_non_negative 
        CHECK (subtotal >= 0);

ALTER TABLE orders
    ADD CONSTRAINT chk_orders_voucher_non_negative 
        CHECK (shop_voucher >= 0);

ALTER TABLE orders
    ADD CONSTRAINT chk_orders_voucher_within_subtotal 
        CHECK (shop_voucher <= subtotal);

ALTER TABLE orders
    ADD CONSTRAINT chk_orders_gross_revenue_formula 
        CHECK (gross_revenue >= 0 AND gross_revenue = subtotal - shop_voucher);

-- Channel <-> Payment Method Compatibility Matrix
ALTER TABLE orders
    ADD CONSTRAINT chk_orders_channel_payment_compatibility 
        CHECK (
            (channel = 'POS' AND payment_method IN ('CASH', 'POS_CARD_QR')) OR
            (channel IN ('TIKTOK', 'SHOPEE') AND payment_method = 'MARKETPLACE_WALLET')
        );

-- Lifecycle State Integrity (Valid Stored State)
ALTER TABLE orders
    ADD CONSTRAINT chk_orders_status_timestamps_integrity 
        CHECK (
            (status = 'PENDING' AND delivered_at IS NULL AND cancelled_at IS NULL AND cancellation_reason IS NULL) OR
            (status = 'SHIPPED' AND delivered_at IS NULL AND cancelled_at IS NULL AND cancellation_reason IS NULL) OR
            (status = 'DELIVERED' AND delivered_at IS NOT NULL AND cancelled_at IS NULL AND cancellation_reason IS NULL) OR
            (status = 'CANCELLED' AND cancelled_at IS NOT NULL AND delivered_at IS NULL AND cancellation_reason IS NOT NULL AND trim(cancellation_reason) <> '')
        );
```

#### Channel $\leftrightarrow$ Payment Method Compatibility Matrix:
| Sales Channel | Permitted Payment Methods | Prohibited Combinations (Rejected by CHECK) |
|---|---|---|
| `POS` (In-Store Retail) | `CASH` (0% fee), `POS_CARD_QR` (1% fee) | `POS` + `MARKETPLACE_WALLET` |
| `TIKTOK` (Social Commerce) | `MARKETPLACE_WALLET` | `TIKTOK` + `CASH`, `TIKTOK` + `POS_CARD_QR` |
| `SHOPEE` (E-Commerce Platform) | `MARKETPLACE_WALLET` | `SHOPEE` + `CASH`, `SHOPEE` + `POS_CARD_QR` |

#### State Integrity vs. State Transitions:
- **Database Boundary:** The check constraint guarantees that any record stored with `DELIVERED` status **must** possess a valid `delivered_at` timestamp and zero cancellation artifacts, and any `CANCELLED` record **must** have `cancelled_at` and a non-empty `cancellation_reason`.
- **Application Boundary:** The ASP.NET Core service validates legal transitions (`PENDING -> SHIPPED`, `SHIPPED -> DELIVERED`, `PENDING/SHIPPED -> CANCELLED`). Guard clauses reject invalid state jumps (e.g., direct transition from `PENDING -> DELIVERED` without carrier handover, or cancelling a `DELIVERED` order).

---

### 2.4 Table: `order_items`
Order line items with immutable unit baseline cost snapshots.

```sql
-- Quantity & Line Valuation Integrity
ALTER TABLE order_items
    ADD CONSTRAINT chk_order_items_quantity_positive 
        CHECK (quantity > 0);

ALTER TABLE order_items
    ADD CONSTRAINT chk_order_items_unit_price_non_negative 
        CHECK (unit_price >= 0);

ALTER TABLE order_items
    ADD CONSTRAINT chk_order_items_unit_cost_snapshot_non_negative 
        CHECK (unit_cost_snapshot >= 0);

ALTER TABLE order_items
    ADD CONSTRAINT chk_order_items_line_total_formula 
        CHECK (line_total >= 0 AND line_total = quantity * unit_price);

ALTER TABLE order_items
    ADD CONSTRAINT chk_order_items_total_cost_formula 
        CHECK (total_cost >= 0 AND total_cost = quantity * unit_cost_snapshot);

-- Foreign Key Constraints
ALTER TABLE order_items
    ADD CONSTRAINT fk_order_items_order_id 
        FOREIGN KEY (order_id) REFERENCES orders(id) 
        ON DELETE CASCADE;

ALTER TABLE order_items
    ADD CONSTRAINT fk_order_items_variant_id 
        FOREIGN KEY (product_variant_id) REFERENCES product_variants(id) 
        ON DELETE RESTRICT;
```

#### The Cost Snapshot Immutability Invariant:
1. When an order line is created, the system reads `product_variants.cost_price` and freezes it permanently into `order_items.unit_cost_snapshot`.
2. **Strict Invariant:** Future updates to master catalog baseline costs **must never mutate** historical order line snapshots:
   $$\Delta(\text{product\_variants.cost\_price}) \centernot\implies \Delta(\text{order\_items.unit\_cost\_snapshot})$$
3. **Line Immutability Rule:** After an order transitions to `SHIPPED`, `DELIVERED`, or `CANCELLED`, order line items cannot be modified or deleted. Line items are only cascaded if a draft order is aborted before status progression.

---

### 2.5 Table: `order_status_history`
Append-only chronological audit log capturing every lifecycle transition.

```sql
-- Actor Identity Constraint
ALTER TABLE order_status_history
    ADD CONSTRAINT chk_order_status_history_changed_by_not_empty 
        CHECK (trim(changed_by) <> '');

-- Meaningful Status Transition Constraint
ALTER TABLE order_status_history
    ADD CONSTRAINT chk_order_status_history_valid_transition 
        CHECK (
            (from_status IS NULL AND to_status = 'PENDING') OR
            (from_status <> to_status)
        );

-- Foreign Key Constraint
ALTER TABLE order_status_history
    ADD CONSTRAINT fk_order_status_history_order_id 
        FOREIGN KEY (order_id) REFERENCES orders(id) 
        ON DELETE CASCADE;
```

#### Append-Only Policy:
- **`INSERT`:** Permitted upon every lifecycle change.
- **`UPDATE`:** Strictly prohibited (enforced via database trigger or read-only service policies).
- **`DELETE`:** Prohibited except on cascade deletion of initial unfulfilled draft orders.

---

### 2.6 Table: `fee_schedules`
Configurable fee deduction policy matrix keyed by channel and payment method.

```sql
-- Decimal Percentage Rate Boundaries [0.0000 to 1.0000]
ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_commission_rate 
        CHECK (commission_rate BETWEEN 0 AND 1);

ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_payment_fee_rate 
        CHECK (payment_fee_rate BETWEEN 0 AND 1);

ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_service_fee_rate 
        CHECK (service_fee_rate BETWEEN 0 AND 1);

-- Monetary Cap & Fixed Charges
ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_service_fee_cap 
        CHECK (service_fee_cap IS NULL OR service_fee_cap >= 0);

ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_fixed_fee 
        CHECK (fixed_fee_per_order >= 0);

-- Date Validity Period
ALTER TABLE fee_schedules
    ADD CONSTRAINT chk_fee_schedules_effective_dates 
        CHECK (effective_to IS NULL OR effective_to >= effective_from);

-- Single Active Schedule Uniqueness per Channel + Payment Method
CREATE UNIQUE INDEX uq_fee_schedules_active_lookup 
    ON fee_schedules (channel, payment_method) 
    WHERE is_active = true;
```

#### Fee Schedule Versioning Invariant:
- Exact fee percentages (e.g., TikTok 4% comm + 3% pay + 3,000 ₫; Shopee 4.5% comm + 4% pay + 2% svc) are **configuration data**, not hardcoded CHECK constraints.
- When platform fee policies change, administrators **deactivate** the existing record (`is_active = false`, set `effective_to`) and **insert** a new active fee schedule record.
- Existing orders referencing historical `fee_schedule_id` retain their immutable rate bindings permanently.

> [!WARNING]
> **P05-GAP-01 (Shopee Service Fee Cap Definition):**
> High-level requirements designate Shopee service fee as "percentage capped", but the exact monetary cap ceiling in VND is not yet formalized by business stakeholders.
> In accordance with MVP design principles, `service_fee_cap` is modeled as a configurable, nullable `numeric(15, 2)` column rather than hardcoding arbitrary assumptions (e.g. 20,000 VND).

---

### 2.7 Table: `order_fee_snapshots`
Immutable financial snapshot permanently frozen upon order delivery.

```sql
-- Rate Boundaries [0.0000 to 1.0000]
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_commission_rate 
        CHECK (commission_rate BETWEEN 0 AND 1);

ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_payment_rate 
        CHECK (payment_fee_rate BETWEEN 0 AND 1);

ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_service_rate 
        CHECK (service_fee_rate BETWEEN 0 AND 1);

-- Deduction Amount Non-Negativity
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_amounts_non_negative 
        CHECK (
            commission_fee_amount >= 0 AND
            payment_fee_amount >= 0 AND
            service_fee_amount >= 0 AND
            fixed_fee_amount >= 0
        );

-- Mathematical Formula Integrity
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_total_fees_formula 
        CHECK (
            total_platform_fees >= 0 AND
            total_platform_fees = commission_fee_amount + payment_fee_amount + service_fee_amount + fixed_fee_amount
        );

-- Projected Settlement Formula
-- (Validated against order gross revenue at generation time)
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT chk_order_fee_snapshots_projected_settlement_sanity 
        CHECK (projected_settlement IS NOT NULL);

-- Cardinality: 1:0..1 Relationship (One snapshot per order)
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT uq_order_fee_snapshots_order_id 
        UNIQUE (order_id);

-- Foreign Key Constraints
ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT fk_order_fee_snapshots_order_id 
        FOREIGN KEY (order_id) REFERENCES orders(id) 
        ON DELETE RESTRICT;

ALTER TABLE order_fee_snapshots
    ADD CONSTRAINT fk_order_fee_snapshots_fee_schedule_id 
        FOREIGN KEY (fee_schedule_id) REFERENCES fee_schedules(id) 
        ON DELETE RESTRICT;
```

#### Snapshot Immutability & Generation Timing:
- **Generation Trigger:** Generated **strictly upon order delivery** (`status = DELIVERED`) within the atomic delivery transaction.
- **Strict Prohibition:** Final fee snapshots are never generated for `PENDING`, `SHIPPED`, or `CANCELLED` orders. Interactive previews during order composition are computed dynamically by the backend without persisting snapshot records.
- **Append-Only:** Once inserted, updates and deletions are strictly rejected (`INSERT` allowed; `UPDATE` / `DELETE` prohibited).

---

### 2.8 Table: `reconciliation_records`
Financial reconciliation ledger matching expected payouts against actual wallet deposits.

```sql
-- Cardinality: 1:0..1 Relationship with Orders
ALTER TABLE reconciliation_records
    ADD CONSTRAINT uq_reconciliation_records_order_id 
        UNIQUE (order_id);

-- Non-Negative Payout Amount
ALTER TABLE reconciliation_records
    ADD CONSTRAINT chk_reconciliation_records_actual_settlement 
        CHECK (actual_settlement IS NULL OR actual_settlement >= 0);

-- Reconciliation Status Invariants
ALTER TABLE reconciliation_records
    ADD CONSTRAINT chk_reconciliation_records_status_integrity 
        CHECK (
            -- State 1: PENDING_SETTLEMENT
            (status = 'PENDING_SETTLEMENT' AND 
             actual_settlement IS NULL AND 
             variance_amount IS NULL AND 
             reconciled_at IS NULL AND 
             reconciled_by IS NULL)
            OR
            -- State 2: RECONCILED (Clean match)
            (status = 'RECONCILED' AND 
             actual_settlement IS NOT NULL AND 
             variance_amount = 0 AND 
             reconciled_at IS NOT NULL AND 
             reconciled_by IS NOT NULL)
            OR
            -- State 3: DISCREPANCY (Variance shortfall or excess)
            (status = 'DISCREPANCY' AND 
             actual_settlement IS NOT NULL AND 
             variance_amount <> 0 AND 
             reconciliation_notes IS NOT NULL AND 
             trim(reconciliation_notes) <> '' AND 
             reconciled_at IS NOT NULL AND 
             reconciled_by IS NOT NULL)
        );

-- Foreign Key Constraint
ALTER TABLE reconciliation_records
    ADD CONSTRAINT fk_reconciliation_records_order_id 
        FOREIGN KEY (order_id) REFERENCES orders(id) 
        ON DELETE RESTRICT;
```

#### Mathematical Invariants:
- **Canonical Variance Formula:**
  $$\text{variance\_amount} = \text{projected\_settlement} - \text{actual\_settlement}$$
  *(Positive variance = payout shortfall / money withheld; Negative variance = unexpected platform overpayment).*
- **State Guarantee:**
  - An order with `variance_amount != 0` can **never** be labeled `RECONCILED`.
  - An order with `variance_amount == 0` can **never** be labeled `DISCREPANCY`.
  - An order labeled `DISCREPANCY` **must** have non-blank `reconciliation_notes`.

---

### 2.9 Table: `discrepancy_audits`
Root-cause investigation log for reconciliation variances.

```sql
-- Non-Empty Classification & Explanation
ALTER TABLE discrepancy_audits
    ADD CONSTRAINT chk_discrepancy_audits_type_not_empty 
        CHECK (trim(discrepancy_type) <> '');

ALTER TABLE discrepancy_audits
    ADD CONSTRAINT chk_discrepancy_audits_explanation_not_empty 
        CHECK (trim(explanation_note) <> '');

-- Permitted Discrepancy Classification Types
ALTER TABLE discrepancy_audits
    ADD CONSTRAINT chk_discrepancy_audits_classification 
        CHECK (discrepancy_type IN (
            'COMMISSION_RATE_MISMATCH',
            'PAYMENT_FEE_MISMATCH',
            'SERVICE_FEE_MISMATCH',
            'UNEXPECTED_PLATFORM_CHARGE',
            'OTHER'
        ));

-- Resolution Pair Integrity
ALTER TABLE discrepancy_audits
    ADD CONSTRAINT chk_discrepancy_audits_resolution_pair 
        CHECK (
            (resolved_at IS NULL AND resolved_by IS NULL) OR
            (resolved_at IS NOT NULL AND resolved_by IS NOT NULL AND 
             resolution_notes IS NOT NULL AND trim(resolution_notes) <> '')
        );

-- Foreign Key Constraint
ALTER TABLE discrepancy_audits
    ADD CONSTRAINT fk_discrepancy_audits_recon_id 
        FOREIGN KEY (reconciliation_record_id) REFERENCES reconciliation_records(id) 
        ON DELETE CASCADE;
```

---

## 3. Database Constraint Verification Matrix

The following matrix documents test cases demonstrating invalid states that are rejected authoritatively by PostgreSQL constraint enforcement:

| Test Case ID | Target Table | Proposed Invalid Input | Enforcing Constraint | Result | Business Invariant Protected |
|---|---|---|---|:---:|---|
| **TC-CHK-01** | `product_variants` | `cost_price = -15000` | `chk_product_variants_cost_price_non_negative` | **REJECT** | Negative merchandise unit costs forbidden. |
| **TC-CHK-02** | `order_items` | `quantity = 0` | `chk_order_items_quantity_positive` | **REJECT** | Zero or negative line quantities prohibited. |
| **TC-CHK-03** | `orders` | `shop_voucher = 300000, subtotal = 250000` | `chk_orders_voucher_within_subtotal` | **REJECT** | Merchant voucher cannot exceed order subtotal. |
| **TC-CHK-04** | `orders` | `channel = 'TIKTOK', payment_method = 'CASH'` | `chk_orders_channel_payment_compatibility` | **REJECT** | Social commerce channels do not accept cash. |
| **TC-CHK-05** | `orders` | Duplicate `(channel = 'SHOPEE', external_order_id = 'SP1029')` | `uq_orders_channel_external_id` | **REJECT** | Prevents duplicate order capture per channel. |
| **TC-CHK-06** | `fee_schedules` | Two active rows for `(channel = 'POS', payment_method = 'CASH')` with `is_active = true` | `uq_fee_schedules_active_lookup` | **REJECT** | Enforces single active fee schedule rule. |
| **TC-CHK-07** | `orders` | `status = 'DELIVERED', delivered_at = NULL` | `chk_orders_status_timestamps_integrity` | **REJECT** | Anti-phantom revenue: Delivered orders must have delivery timestamp. |
| **TC-CHK-08** | `orders` | `status = 'CANCELLED', cancellation_reason = NULL` | `chk_orders_status_timestamps_integrity` | **REJECT** | Cancellations require mandatory justification. |
| **TC-CHK-09** | `reconciliation_records` | `status = 'RECONCILED', variance_amount = -25000` | `chk_reconciliation_records_status_integrity` | **REJECT** | Unbalanced payouts cannot be flagged clean. |
| **TC-CHK-10** | `reconciliation_records` | `status = 'DISCREPANCY', reconciliation_notes = ''` | `chk_reconciliation_records_status_integrity` | **REJECT** | Variances mandate audit explanation notes. |
| **TC-CHK-11** | `discrepancy_audits` | `discrepancy_type = 'RETURN_FEE_DISPUTE'` | `chk_discrepancy_audits_classification` | **REJECT** | Out-of-scope return dispute types blocked in MVP. |

---

## 4. Automated Timestamp Trigger Strategy

To guarantee accurate modification audit trails across mutable entities without relying on client-side clocks, PostgreSQL trigger automation is specified:

```sql
-- Generic Timestamp Trigger Function
CREATE OR REPLACE FUNCTION fn_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
    NEW.updated_at = clock_timestamp();
    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

-- Apply Trigger to Mutable Entities
CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();

CREATE TRIGGER trg_product_variants_updated_at
    BEFORE UPDATE ON product_variants
    FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();

CREATE TRIGGER trg_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();

CREATE TRIGGER trg_fee_schedules_updated_at
    BEFORE UPDATE ON fee_schedules
    FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();

CREATE TRIGGER trg_reconciliation_records_updated_at
    BEFORE UPDATE ON reconciliation_records
    FOR EACH ROW EXECUTE FUNCTION fn_set_updated_at();
```

> [!NOTE]
> **Immutable Entities Excluded from Update Triggers:**
> `order_items`, `order_status_history`, `order_fee_snapshots`, and `discrepancy_audits` are append-only audit records and do not require `updated_at` modification triggers.

---

## 5. Indexing Architecture & Strategy Matrix

### 5.1 Strategic Indexing Principles
1. **No Redundant Indexes:** If a column already possesses a `UNIQUE` constraint, PostgreSQL automatically builds a backing unique B-tree index. Duplicate secondary indexes on the exact same column (e.g. `product_variants.sku_code`) are strictly eliminated.
2. **Purpose-Driven Indexing:** Indexes exist exclusively to serve foreign key navigation, lookup uniqueness, high-frequency filtering (`WHERE`), range ordering (`ORDER BY`), and multi-table financial analytical joins.
3. **No Unjustified Specialized Indexes:** Hash, BRIN, GIN, and GiST indexes are excluded from MVP as current workloads consist strictly of standard relational lookups and temporal range queries.
4. **Performance Target:** Analytical queries for MVP volume ($< 500{,}000$ commercial orders) target responsive execution to be formally validated by subsequent performance benchmarks.

### 5.2 Index Specification Matrix

| Index Name | Target Table | Indexed Columns | Partial Predicate | Target Query / Workflow | Architectural Rationale |
|---|---|---|---|---|---|
| `uq_product_variants_sku` | `product_variants` | `(sku_code)` | *None* | Product SKU lookup during order entry | Enforces global uniqueness and fast B-tree lookup. |
| `idx_product_variants_pid` | `product_variants` | `(product_id)` | *None* | `GET /api/catalog/products/{id}/variants` | Foreign key index; prevents sequential scan on parent join. |
| `uq_orders_channel_ext` | `orders` | `(channel, external_order_id)` | *None* | Order creation duplicate prevention | Uniqueness per channel; accelerated webhook/manual ID lookup. |
| `idx_orders_status_date` | `orders` | `(status, order_date DESC)` | *None* | Screen 1 Orders Master Table query | High-density status tab filtering with chronological order display. |
| `idx_orders_delivered_channel` | `orders` | `(delivered_at, channel)` | `WHERE status = 'DELIVERED'` | Screen 3 Executive Financial Dashboard | Highly selective partial index; isolates revenue-recognized orders. |
| `idx_order_items_oid` | `order_items` | `(order_id)` | *None* | Order detail loading & financial join | Foreign key index; accelerates line item aggregation per order. |
| `idx_order_items_pvid` | `order_items` | `(product_variant_id)` | *None* | Top SKU leaderboard aggregation | Foreign key index; supports fast grouping of units sold by SKU. |
| `idx_status_hist_order_date` | `order_status_history` | `(order_id, changed_at DESC)` | *None* | Order lifecycle history timeline | Composite index optimizing chronological audit display for single order. |
| `uq_fee_schedules_active` | `fee_schedules` | `(channel, payment_method)` | `WHERE is_active = true` | Dynamic Fee Engine preview & delivery lookup | Enforces single active schedule rule; instantaneous index seek. |
| `idx_fee_schedules_history` | `fee_schedules` | `(channel, payment_method, effective_from DESC)` | *None* | Fee schedule audit & historical lookup | Chronological policy lookup across historical rate revisions. |
| `uq_fee_snapshots_order_id` | `order_fee_snapshots` | `(order_id)` | *None* | Settlement ledger loading | 1:0..1 uniqueness enforcement and instant 1-to-1 order join. |
| `idx_fee_snapshots_schedule` | `order_fee_snapshots` | `(fee_schedule_id)` | *None* | Policy revision impact analysis | Foreign key index; PostgreSQL does not automatically index FKs. |
| `uq_recon_records_order_id` | `reconciliation_records` | `(order_id)` | *None* | Settlement detail loading | 1:0..1 uniqueness enforcement and order join. |
| `idx_recon_records_pending` | `reconciliation_records` | `(created_at)` | `WHERE status = 'PENDING_SETTLEMENT'` | Screen 2 Pending Settlement queue | Accelerated queue filtering for unreconciled orders. |
| `idx_recon_records_variance`| `reconciliation_records` | `(updated_at)` | `WHERE status = 'DISCREPANCY'` | Screen 2 Discrepancy investigation queue | Accelerated retrieval of orders requiring dispute review notes. |
| `idx_disc_audits_recon_id` | `discrepancy_audits` | `(reconciliation_record_id)`| *None* | Slide-over discrepancy drawer display | Foreign key index; links audit cases to parent reconciliation record. |
| `idx_disc_audits_unresolved`| `discrepancy_audits` | `(reconciliation_record_id, created_at DESC)` | `WHERE resolved_at IS NULL` | Open dispute review alerts | Partial index isolating active unresolved disputes. |

---

## 6. Transaction Boundaries & ACID Sequences

To prevent partial updates, phantom revenue recognition, or unbacked snapshot states, all state-altering operations execute within atomic database transactions:

### 6.1 Transaction 1: Create Multi-Item Order (UC01)
```
BEGIN TRANSACTION;
  1. Read product_variants for each selected SKU;
  2. Verify variant is_active = true;
  3. Freeze current catalog cost into immutable unit_cost_snapshot;
  4. Compute line_total = quantity * unit_price;
  5. Compute total_cost = quantity * unit_cost_snapshot;
  6. Compute subtotal = SUM(line_total);
  7. Compute gross_revenue = subtotal - shop_voucher;
  8. INSERT INTO orders (id, channel, external_order_id, status='PENDING', ...);
  9. INSERT INTO order_items (id, order_id, product_variant_id, unit_cost_snapshot, ...);
 10. INSERT INTO order_status_history (order_id, from_status=NULL, to_status='PENDING', ...);
COMMIT;
-- If any insert or validation fails -> ROLLBACK.
```

### 6.2 Transaction 2: Mark Order as Delivered (UC03)
```
BEGIN TRANSACTION;
  1. SELECT status FROM orders WHERE id = target_id FOR UPDATE;
  2. Verify current status == 'SHIPPED';
  3. Lookup active fee schedule for order.channel and order.payment_method;
  4. Compute commission_fee, payment_fee, service_fee (with cap), fixed_fee;
  5. Compute total_platform_fees and projected_settlement = gross_revenue - total_platform_fees;
  6. UPDATE orders SET status = 'DELIVERED', delivered_at = NOW() WHERE id = target_id;
  7. INSERT INTO order_fee_snapshots (order_id, fee_schedule_id, total_platform_fees, projected_settlement, ...);
  8. INSERT INTO reconciliation_records (order_id, projected_settlement, status='PENDING_SETTLEMENT', ...);
  9. INSERT INTO order_status_history (order_id, from_status='SHIPPED', to_status='DELIVERED', ...);
COMMIT;
-- Guarantees that Delivered status, Fee Snapshot, and Reconciliation Ledger are created atomically.
```

### 6.3 Transaction 3: Cancel Order (UC04)
```
BEGIN TRANSACTION;
  1. SELECT status FROM orders WHERE id = target_id FOR UPDATE;
  2. Verify current status IN ('PENDING', 'SHIPPED');
  3. Validate mandatory cancellation_reason is provided;
  4. UPDATE orders SET status = 'CANCELLED', cancelled_at = NOW(), cancellation_reason = reason_text WHERE id = target_id;
  5. INSERT INTO order_status_history (order_id, from_status=prior_status, to_status='CANCELLED', reason=reason_text, ...);
COMMIT;
```

### 6.4 Transaction 4: Record Manual Settlement Payout & Reconcile (UC06 & UC07)
```
BEGIN TRANSACTION;
  1. SELECT id, projected_settlement, status FROM reconciliation_records WHERE id = target_recon_id FOR UPDATE;
  2. Validate current status == 'PENDING_SETTLEMENT' OR 'DISCREPANCY';
  3. Set actual_settlement = verified_cash_payout;
  4. Compute variance_amount = projected_settlement - actual_settlement;
  5. IF variance_amount == 0 THEN
         UPDATE reconciliation_records 
         SET actual_settlement = verified_cash_payout, variance_amount = 0, status = 'RECONCILED', 
             reconciled_at = NOW(), reconciled_by = current_user_id 
         WHERE id = target_recon_id;
     ELSE
         Validate mandatory reconciliation_notes is provided;
         UPDATE reconciliation_records 
         SET actual_settlement = verified_cash_payout, variance_amount = computed_variance, status = 'DISCREPANCY', 
             reconciliation_notes = notes_text, reconciled_at = NOW(), reconciled_by = current_user_id 
         WHERE id = target_recon_id;
         
         INSERT INTO discrepancy_audits (reconciliation_record_id, discrepancy_type, explanation_note, ...)
         VALUES (target_recon_id, selected_type, notes_text, ...);
     END IF;
COMMIT;
```

---

## 7. Concurrency Control Policy

- **Pessimistic Row Locking (`SELECT ... FOR UPDATE`):** Applied during lifecycle status progression (`Ship`, `Deliver`, `Cancel`) and manual reconciliation to prevent race conditions from concurrent duplicate submissions.
- **Delivery & Reconciliation Isolation:** Payout recording cannot execute concurrently on an order undergoing lifecycle transition.
- **Target Implementation:** Orchestrated authoritatively by EF Core transaction boundaries (`IDbContextTransaction`) within the ASP.NET Core application services.

---

## 8. Persisted vs. Derived Financial Data Matrix

To maintain single source of truth and eliminate data divergence, financial figures are categorized strictly into persisted storage vs. query-time derivation:

| Financial Metric | Database Classification | Formula / Derivation Method | Rationale |
|---|---|---|---|
| `orders.subtotal` | **Persisted** | $\sum (\text{Quantity} \times \text{Unit Selling Price})$ | Pre-discount sales total frozen upon order placement. |
| `orders.shop_voucher` | **Persisted** | Direct merchant input | Immutable coupon deduction recorded at order capture. |
| `orders.gross_revenue` | **Persisted** | `subtotal - shop_voucher` | Primary revenue baseline; stored to accelerate index queries. |
| `order_items.line_total` | **Persisted** | `quantity * unit_price` | Item-level customer payable figure. |
| `order_items.unit_cost_snapshot` | **Persisted** | Copied from `product_variants.cost_price` | Immutable baseline cost for permanent COGS auditability. |
| `order_items.total_cost` | **Persisted** | `quantity * unit_cost_snapshot` | Item-level Cost of Goods Sold snapshot. |
| **Total COGS** | **Derived** | $\sum (\text{order\_items.total\_cost})$ for Delivered orders | Aggregated at query time across delivered order items. |
| `order_fee_snapshots.total_platform_fees` | **Persisted** | $\sum (\text{Commission} + \text{Payment} + \text{Service} + \text{Fixed})$ | Immutable fee snapshot frozen upon delivery. |
| `order_fee_snapshots.projected_settlement` | **Persisted** | `gross_revenue - total_platform_fees` | Net realized revenue recognized upon delivery. |
| **Contribution Profit** | **Derived** | $\text{Projected Settlement} - \text{Total COGS}$ | Computed on-the-fly in analytics layer; never stored as static column. |
| **Contribution Margin %** | **Derived** | $(\text{Contribution Profit} / \text{Gross Revenue}) \times 100$ | Computed on-the-fly when Gross Revenue $> 0$. |
| `reconciliation_records.actual_settlement`| **Persisted** | Verified bank/wallet payout input | Real cash receipt entered by Finance Manager. |
| `reconciliation_records.variance_amount` | **Persisted** | `projected_settlement - actual_settlement` | Stored for fast indexing of pending and discrepancy queues. |

> [!CAUTION]
> **Prohibition of Net Profit Columns:**
> Creating any column or field named `net_profit`, `net_income`, or `operating_profit` is strictly prohibited. Contribution Profit is dynamically derived and excludes corporate operational expenses (rent, payroll, administrative overhead, marketing OPEX, and corporate income taxes).
