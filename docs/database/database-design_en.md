# Database Design Specification: Target MVP Relational Schema
## Fashion Revenue & Profit Management System

---

## 1. Architectural Overview & Engineering Standards

### 1.1 Purpose & Scope
This document specifies the **Target MVP Database Architecture** for the **Fashion Revenue & Profit Management System**. 

> [!IMPORTANT]
> **Independent Target Specification:** The schema defined in [`schema.dbml`](file:///c:/AI_thuc_chien_khoa_3/VSF/Quanly_DoanhThu_LoiNhuan_DangDucHoa_VSF/docs/database/schema.dbml) and detailed herein represents an independent, clean-room target relational design optimized for ACID transaction integrity, multi-channel fee calculation, manual settlement reconciliation, and baseline **Contribution Profit** reporting. It strictly adheres to current MVP requirements without incorporating out-of-scope enterprise ERP or accounting abstractions.

### 1.2 Core Standards & Architectural Invariants
- **Target RDBMS:** **PostgreSQL 16+** with native transactional ACID guarantees.
- **Naming Conventions:** Strict `snake_case` across all tables, columns, indexes, and constraints.
- **Primary Key Standard:** Universal `uuid` (UUID v4 via `gen_random_uuid()`) for all business entity tables to prevent enumeration attacks and support distributed ingestion.
- **Monetary Precision:** All monetary amounts are typed as `numeric(15, 2)`. Floating-point types (`float`, `double`, `real`) are strictly prohibited to eliminate IEEE 754 rounding inaccuracies.
- **Timestamp Standard:** All temporal fields are stored as `timestamptz` in **UTC**. Conversion to local business timezone (`Asia/Ho_Chi_Minh`, UTC+7) is handled exclusively at presentation/query boundaries.
- **Fee Rate Conventions:** Percentage rates are stored as fractional decimals (`numeric(6, 4)`), where `0.0400` represents 4.00%, `0.0200` represents 2.00%, `0.0100` represents 1.00%, and `0.0000` represents 0.00%.
- **Core Scope:** Exactly **9 core transactional tables** organized into 4 cohesive functional domains.
- **Contribution Profit Architecture (Direction 2):**
  - MVP profit analysis centers exclusively on **Contribution Profit**:
    ```text
    Gross Revenue = Subtotal - Shop Voucher
    Total Platform Fees = Commission Fee + Payment Fee + Service Fee + Fixed Fee
    Projected Settlement (Net Realized Revenue) = Gross Revenue - Total Platform Fees
    COGS = Σ(Quantity × Unit Cost Snapshot)
    Contribution Profit = Projected Settlement - COGS = Gross Revenue - Total Platform Fees - COGS
    Contribution Margin % = (Contribution Profit / Gross Revenue) × 100 (when Gross Revenue > 0)
    ```
  - **Canonical Boundary Note:** *"Contribution Profit represents order/channel profitability after marketplace fees and COGS, but before corporate operating expenses and taxes."*
  - **Strict Naming Prohibition:** Contribution Profit is **never** referred to as *Net Profit*, *Net Income*, or *Operating Profit*.
  - **Simplified Baseline Cost Model:** `product_variants.cost_price` represents a manually maintained baseline unit merchandise cost. Automated inventory valuation engines (FIFO, LIFO, Moving Weighted Average), warehouse receiving workflows, purchase orders, and corporate OPEX (rent, payroll, marketing, general ledger, tax, depreciation) are strictly excluded from MVP.

---

## 2. Target Entity-Relationship Diagram (ERD)

The following diagram reflects the canonical structure declared in [`schema.dbml`](file:///c:/AI_thuc_chien_khoa_3/VSF/Quanly_DoanhThu_LoiNhuan_DangDucHoa_VSF/docs/database/schema.dbml):

```mermaid
erDiagram
    %% GROUP 1: CATALOG & PRICING
    products ||--o{ product_variants : "has variants (1:N)"
    product_variants ||--o{ order_items : "referenced in (1:N)"

    %% GROUP 2: ORDERS & LIFECYCLE
    orders ||--|{ order_items : "contains items (1:1..N)"
    orders ||--o{ order_status_history : "lifecycle log (1:N)"

    %% GROUP 3: FEES & FINANCIAL SNAPSHOTS
    orders ||--o| order_fee_snapshots : "freezes fees (1:0..1)"
    fee_schedules ||--o{ order_fee_snapshots : "applies schedule (1:N)"

    %% GROUP 4: SETTLEMENT & RECONCILIATION
    orders ||--o| reconciliation_records : "reconciles order (1:0..1)"
    reconciliation_records ||--o{ discrepancy_audits : "tracks audits (1:N)"

    products {
        uuid id PK
        varchar_255 name
        varchar_100 category
        boolean is_active
        timestamptz created_at
        timestamptz updated_at
    }

    product_variants {
        uuid id PK
        uuid product_id FK
        varchar_100 sku_code UK
        varchar_50 color
        varchar_20 size
        numeric_15_2 retail_price
        numeric_15_2 cost_price
        boolean is_active
        timestamptz created_at
        timestamptz updated_at
    }

    orders {
        uuid id PK
        varchar_100 external_order_id
        channel_type channel
        order_status status
        payment_method payment_method
        numeric_15_2 subtotal
        numeric_15_2 shop_voucher
        numeric_15_2 gross_revenue
        varchar_255 customer_name
        varchar_20 customer_phone
        timestamptz order_date
        timestamptz delivered_at
        timestamptz cancelled_at
        text cancellation_reason
        timestamptz created_at
        timestamptz updated_at
    }

    order_items {
        uuid id PK
        uuid order_id FK
        uuid product_variant_id FK
        varchar_100 sku_code_snapshot
        varchar_255 product_name_snapshot
        integer quantity
        numeric_15_2 unit_price
        numeric_15_2 unit_cost_snapshot
        numeric_15_2 line_total
        numeric_15_2 total_cost
        timestamptz created_at
    }

    order_status_history {
        uuid id PK
        uuid order_id FK
        order_status from_status
        order_status to_status
        text reason
        varchar_100 changed_by
        timestamptz changed_at
    }

    fee_schedules {
        uuid id PK
        channel_type channel
        payment_method payment_method
        numeric_6_4 commission_rate
        numeric_6_4 payment_fee_rate
        numeric_6_4 service_fee_rate
        numeric_15_2 service_fee_cap
        numeric_15_2 fixed_fee_per_order
        date effective_from
        date effective_to
        boolean is_active
        timestamptz created_at
        timestamptz updated_at
    }

    order_fee_snapshots {
        uuid id PK
        uuid order_id FK,UK
        uuid fee_schedule_id FK
        numeric_6_4 commission_rate
        numeric_15_2 commission_fee_amount
        numeric_6_4 payment_fee_rate
        numeric_15_2 payment_fee_amount
        numeric_6_4 service_fee_rate
        numeric_15_2 service_fee_amount
        numeric_15_2 service_fee_cap_snapshot
        numeric_15_2 fixed_fee_amount
        numeric_15_2 total_platform_fees
        numeric_15_2 projected_settlement
        timestamptz snapshot_at
    }

    reconciliation_records {
        uuid id PK
        uuid order_id FK,UK
        numeric_15_2 projected_settlement
        numeric_15_2 actual_settlement
        numeric_15_2 variance_amount
        reconciliation_status status
        text reconciliation_notes
        timestamptz reconciled_at
        varchar_100 reconciled_by
        timestamptz created_at
        timestamptz updated_at
    }

    discrepancy_audits {
        uuid id PK
        uuid reconciliation_record_id FK
        varchar_50 discrepancy_type
        text explanation_note
        text resolution_notes
        varchar_100 resolved_by
        timestamptz resolved_at
        timestamptz created_at
    }
```

---

## 3. Database Enumerations

| Enum Name | Allowed Values | Description |
| :--- | :--- | :--- |
| `channel_type` | `TIKTOK`, `SHOPEE`, `POS` | Multi-channel sales origination source. |
| `payment_method` | `CASH`, `POS_CARD_QR`, `MARKETPLACE_WALLET` | Settlement instrument used by the customer. Differentiates 0% cash fee vs 1% card/QR fee at POS. |
| `order_status` | `PENDING`, `SHIPPED`, `DELIVERED`, `CANCELLED` | Core operational lifecycle status. Revenue is recognized strictly at `DELIVERED`. |
| `reconciliation_status`| `PENDING_SETTLEMENT`, `RECONCILED`, `DISCREPANCY` | Canonical accounting audit status between projected settlement and actual disbursement. |

> [!NOTE]
> **No RETURNED Status in Core MVP:** The operational status `RETURNED` / `REFUNDED` is excluded from the core state machine to prevent un-scoped return workflow complexity in MVP. It is deferred to Future Extensions.

---

## 4. Core Table Catalog

### 4.1 Group: Catalog & Pricing

#### Table: `products`
Root product entity representing the master fashion style/model.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Core Columns:** `name` (`varchar(255)`, not null), `category` (`varchar(100)`), `is_active` (`boolean`, default: `true`).
- **Audit Columns:** `created_at`, `updated_at` (`timestamptz`).

#### Table: `product_variants`
Specific sellable stock-keeping units (SKUs) defined by color and size combinations.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Foreign Key:** `product_id` -> `products(id)` ON DELETE RESTRICT.
- **Natural Key:** `sku_code` (`varchar(100)` unique, not null).
- **Pricing & Cost Baseline:**
  - `retail_price` (`numeric(15, 2)`, check `>= 0`): Listed catalog retail selling price in VND.
  - `cost_price` (`numeric(15, 2)`, check `>= 0`, default: `0.00`): Baseline unit Cost of Goods Sold (COGS) in VND manually maintained by Shop Owner / Finance Manager. Note: Non-restrictive; `retail_price` may be lower than `cost_price` during sales or clearance promotions.
- **Attributes:** `color` (`varchar(50)`), `size` (`varchar(20)`), `is_active` (`boolean`, default: `true`).
- **Audit Columns:** `created_at`, `updated_at` (`timestamptz`).
- **Uniqueness & Indexes:** Unique index on `sku_code`; index on `product_id`.

---

### 4.2 Group: Orders & Lifecycle Management

#### Table: `orders`
Master sales order entity recording multi-channel commercial transactions.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Channel Identity:**
  - `channel` (`channel_type`, not null).
  - `external_order_id` (`varchar(100)`): External order identifier from marketplace or POS receipt.
  - `UNIQUE (channel, external_order_id)` ensures idempotent order ingestion.
- **Financial Baseline:**
  - `subtotal` (`numeric(15, 2)`, not null): Sum of item quantities × unit selling prices.
  - `shop_voucher` (`numeric(15, 2)`, default: `0.00`): Merchant-funded discount.
  - `gross_revenue` (`numeric(15, 2)`, not null): Customer payable amount (`subtotal - shop_voucher`).
- **Lifecycle & Temporal Columns:**
  - `status` (`order_status`, default: `PENDING`).
  - `order_date` (`timestamptz`, not null).
  - `delivered_at` (`timestamptz`, nullable): Populated strictly upon customer delivery.
  - `cancelled_at` (`timestamptz`, nullable): Populated if order transitions to `CANCELLED`.
  - `cancellation_reason` (`text`, nullable): Mandatory explanation note upon cancellation.
- **Customer Information:** `customer_name` (`varchar(255)`), `customer_phone` (`varchar(20)`).
- **Audit Columns:** `created_at`, `updated_at` (`timestamptz`).

#### Table: `order_items`
Detailed line items for each order, maintaining an immutable historical audit snapshot.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Foreign Keys:**
  - `order_id` -> `orders(id)` ON DELETE CASCADE.
  - `product_variant_id` -> `product_variants(id)` ON DELETE RESTRICT.
- **Immutable Purchase Snapshots:**
  - `sku_code_snapshot` (`varchar(100)`, not null): SKU text at purchase time.
  - `product_name_snapshot` (`varchar(255)`, not null): Product title at purchase time.
  - `unit_cost_snapshot` (`numeric(15, 2)`, not null, default: `0.00`): Frozen baseline unit cost at order placement time in VND.
- **Line Calculations:**
  - `quantity` (`integer`, check `> 0`).
  - `unit_price` (`numeric(15, 2)`, check `>= 0`).
  - `line_total` (`numeric(15, 2)`, check `= quantity * unit_price`): Stored line item gross revenue total.
  - `total_cost` (`numeric(15, 2)`, check `= quantity * unit_cost_snapshot`): Stored line item COGS in VND.
- **Audit Columns:** `created_at` (`timestamptz`).

> [!IMPORTANT]
> **Baseline Cost Immutability:** Once an order item is recorded, `unit_cost_snapshot` and `total_cost` are frozen permanently. Subsequent modifications to `product_variants.cost_price` by catalog managers never retroactively alter historical orders. Order COGS is derived as `SUM(order_items.total_cost)`.
>
> **Cardinality Invariant (Order 1 -> 1..* OrderItems):** A commercial order must contain at least one line item. Because standard relational foreign keys cannot enforce non-empty child sets, this invariant is validated in the application command handler prior to transaction commit.

#### Table: `order_status_history`
Append-only chronological state machine audit log.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Foreign Key:** `order_id` -> `orders(id)` ON DELETE CASCADE.
- **Columns:** `from_status` (`order_status`), `to_status` (`order_status`), `reason` (`text`), `changed_by` (`varchar(100)`), `changed_at` (`timestamptz`).
- **Purpose:** Full audit traceability. `orders.status` tracks current state; `order_status_history` records historical transitions.

---

### 4.3 Group: Fees & Financial Snapshots

#### Table: `fee_schedules`
Configurable fee policy matrix keyed by channel and payment method.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Lookup Dimensions:**
  - `channel` (`channel_type`, not null).
  - `payment_method` (`payment_method`, not null).
- **Fee Configuration:**
  - `commission_rate` (`numeric(6, 4)`, default: `0.0000`): Marketplace commission rate (e.g., `0.0400` = 4.00%).
  - `payment_fee_rate` (`numeric(6, 4)`, default: `0.0000`): Payment gateway rate (e.g., `0.0100` = 1.00%).
  - `service_fee_rate` (`numeric(6, 4)`, default: `0.0000`): Percentage service fee (e.g., `0.0200` = 2.00% on Shopee).
  - `service_fee_cap` (`numeric(15, 2)`, nullable): Maximum monetary cap for service fees.
  - `fixed_fee_per_order` (`numeric(15, 2)`, default: `0.00`): Fixed processing charge per order (e.g., 3,000 VND on TikTok Shop).
- **Temporal Validity:**
  - `effective_from` (`date`, not null).
  - `effective_to` (`date`, nullable; null denotes ongoing validity).
  - `is_active` (`boolean`, default: `true`).
- **Channel Policy Representation:**
  - **TikTok Shop:** `commission_rate` (commission), `payment_fee_rate` (payment), `fixed_fee_per_order` (fixed service), `service_fee_rate = 0`, `service_fee_cap = null`.
  - **Shopee:** `commission_rate` (commission), `payment_fee_rate` (payment), `service_fee_rate` (2%), `service_fee_cap` (capped maximum), `fixed_fee_per_order = 0`.
  - **POS:** Differentiates `CASH` (0% fees across all categories) vs `POS_CARD_QR` (`payment_fee_rate = 0.0100`, others 0).

#### Table: `order_fee_snapshots`
Frozen financial and fee snapshot created when an order transitions to `DELIVERED`.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Cardinality:** Exactly `1:0..1` with `orders`.
  - `order_id` (`uuid`, UNIQUE, FK -> `orders(id)` ON DELETE RESTRICT).
  - `fee_schedule_id` (`uuid`, FK -> `fee_schedules(id)` ON DELETE RESTRICT).
- **Applied Rates & Calculated Amounts:**
  - `commission_rate` (`numeric(6, 4)`), `commission_fee_amount` (`numeric(15, 2)`).
  - `payment_fee_rate` (`numeric(6, 4)`), `payment_fee_amount` (`numeric(15, 2)`).
  - `service_fee_rate` (`numeric(6, 4)`), `service_fee_amount` (`numeric(15, 2)`).
  - `service_fee_cap_snapshot` (`numeric(15, 2)`, nullable).
  - `fixed_fee_amount` (`numeric(15, 2)`).
- **Aggregated Deductions & Net Settlement:**
  - `total_platform_fees` (`numeric(15, 2)`): Sum of all platform deductions.
  - `projected_settlement` (`numeric(15, 2)`): Net amount expected to be disbursed (`gross_revenue - total_platform_fees`). Represents Net Realized Revenue.
- **Audit Timestamp:** `snapshot_at` (`timestamptz`).

---

### 4.4 Group: Settlement & Reconciliation

#### Table: `reconciliation_records`
Financial settlement record comparing expected net payouts against actual channel/bank cash disbursements.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Cardinality:** Exactly `1:0..1` with `orders`.
  - `order_id` (`uuid`, UNIQUE, FK -> `orders(id)` ON DELETE RESTRICT).
- **Settlement Amounts:**
  - `projected_settlement` (`numeric(15, 2)`): Copied from `order_fee_snapshots.projected_settlement`.
  - `actual_settlement` (`numeric(15, 2)`, nullable): Net cash disbursed into merchant wallet by platform/bank.
  - `variance_amount` (`numeric(15, 2)`, nullable): Mathematical discrepancy (`projected_settlement - actual_settlement`).
- **Status & Explanations:**
  - `status` (`reconciliation_status`, default: `PENDING_SETTLEMENT`).
  - `reconciliation_notes` (`text`, nullable): Explanation notes. **Mandatory whenever `variance_amount != 0`**.
- **Audit Columns:**
  - `reconciled_at` (`timestamptz`), `reconciled_by` (`varchar(100)`).
  - `created_at`, `updated_at` (`timestamptz`).

#### Table: `discrepancy_audits`
Detailed investigation log tracking root cause analysis and resolution notes for reconciliation variances.
- **Primary Key:** `id` (`uuid`, default: `gen_random_uuid()`).
- **Foreign Key:** `reconciliation_record_id` -> `reconciliation_records(id)` ON DELETE CASCADE.
- **Investigation Fields:**
  - `discrepancy_type` (`varchar(50)`, not null): Classification (e.g., `COMMISSION_RATE_MISMATCH`, `PAYMENT_FEE_MISMATCH`, `SERVICE_FEE_MISMATCH`, `UNEXPECTED_PLATFORM_CHARGE`, `OTHER`).
  - `explanation_note` (`text`, not null): Detailed documentation of the root cause.
  - `resolution_notes` (`text`, nullable): Settlement/dispute resolution outcome.
  - `resolved_by` (`varchar(100)`, nullable), `resolved_at` (`timestamptz`, nullable).
- **Audit Timestamp:** `created_at` (`timestamptz`).

---

## 5. Relationship Matrix & Cardinality Standards

| Parent Table | Child Table | Relationship | Foreign Key Column | Delete Rule | Rationale |
| :--- | :--- | :---: | :--- | :--- | :--- |
| `products` | `product_variants` | `1:N` | `product_id` | `RESTRICT` | Master product cannot be deleted if variants exist. |
| `product_variants` | `order_items` | `1:N` | `product_variant_id` | `RESTRICT` | SKUs with historical transactions cannot be deleted. |
| `orders` | `order_items` | `1:1..N` | `order_id` | `CASCADE` | Items are intrinsic components of their parent order. |
| `orders` | `order_status_history` | `1:N` | `order_id` | `CASCADE` | Status history belongs exclusively to the order lifecycle. |
| `orders` | `order_fee_snapshots` | `1:0..1` | `order_id` (UK) | `RESTRICT` | Financial snapshot preserves immutable audit integrity. |
| `fee_schedules` | `order_fee_snapshots` | `1:N` | `fee_schedule_id` | `RESTRICT` | Fee schedule referenced by historical orders cannot be deleted. |
| `orders` | `reconciliation_records`| `1:0..1` | `order_id` (UK) | `RESTRICT` | Reconciliation ledger records must be preserved. |
| `reconciliation_records`| `discrepancy_audits` | `1:N` | `reconciliation_record_id` | `CASCADE` | Audit investigations cascade with parent reconciliation record. |

---

## 6. Financial Integrity Rules & Business Invariants

### 6.1 Canonical Gross Revenue
Gross Revenue is standardized across all sales channels as:
```text
gross_revenue = subtotal - shop_voucher
```
- `subtotal`: Sum of quantity × unit selling price before merchant voucher.
- `shop_voucher`: Merchant-funded discount.
- `gross_revenue`: Net customer payable amount. It serves as the baseline for payment fees.

### 6.2 Anti-Phantom Revenue Invariant
Revenue is recognized strictly upon successful delivery:
```sql
Recognized Gross Revenue = SUM(gross_revenue) 
WHERE status = 'DELIVERED' AND delivered_at IS NOT NULL;
```
- No separate mutable `recognized_revenue` column exists.
- Orders in `PENDING`, `SHIPPED`, or `CANCELLED` contribute 0 to recognized revenue.

### 6.3 Fee Calculation Formulas
When an order transitions to `DELIVERED`, fee calculation proceeds as follows:
1. **Commission Fee:**
   ```text
   commission_fee_amount = subtotal * commission_rate
   ```
2. **Payment Fee:**
   ```text
   payment_fee_amount = gross_revenue * payment_fee_rate
   ```
3. **Service Fee:**
   - For percentage-based service fee (e.g., Shopee):
     ```text
     raw_service_fee = subtotal * service_fee_rate
     service_fee_amount = min(raw_service_fee, service_fee_cap) -- if capped
     service_fee_amount = raw_service_fee                       -- if uncapped
     ```
   - For fixed service fee (e.g., TikTok Shop):
     ```text
     fixed_fee_amount = fixed_fee_per_order
     ```
4. **Total Platform Fees:**
   ```text
   total_platform_fees = commission_fee_amount + payment_fee_amount + service_fee_amount + fixed_fee_amount
   ```
5. **Projected Settlement (Net Realized Revenue):**
   ```text
   projected_settlement = gross_revenue - total_platform_fees
   ```

> [!CAUTION]
> **No Double Deduction of Voucher:**
> Because `gross_revenue` already deducts `shop_voucher`, subtracting `total_platform_fees` from `gross_revenue` correctly yields the net payout without double-deducting the merchant voucher.

### 6.4 Canonical Reconciliation Variance
Discrepancy detection between expected settlement and bank/wallet disbursement uses:
```text
variance_amount = projected_settlement - actual_settlement
```
- `variance_amount > 0`: **Shortfall / Underpayment** (platform disbursed less than expected; merchant loss). Flagged as `DISCREPANCY`.
- `variance_amount = 0`: **Clean Match** (`RECONCILED`).
- `variance_amount < 0`: **Overpayment / Reimbursement** (platform disbursed more than expected).
- **Audit Boundary:** Variance adjustments do not alter COGS or baseline product pricing.

### 6.5 Simplified COGS Model & Cost Immutability
Merchandise costs are calculated using a simplified baseline unit cost model:
```text
line_cogs (total_cost) = quantity * unit_cost_snapshot
order_cogs = SUM(order_items.total_cost)
```
- **Manual Baseline Maintenance:** `product_variants.cost_price` is maintained manually by Shop Owner / Finance Manager. It serves as a benchmark standard unit cost, not an automated inventory valuation engine.
- **Placement-Time Freezing:** When an order is placed, the variant's current `cost_price` is frozen into `order_items.unit_cost_snapshot`, and `order_items.total_cost` is computed.
- **Historical Immutability:** Any subsequent updates to `product_variants.cost_price` (e.g. supplier price changes) do NOT retroactively alter historical orders.
- **Delivery Recognition:** Like revenue, COGS is recognized in financial reports strictly when `orders.status = 'DELIVERED'`. Cancelled orders contribute 0 to recognized COGS.

### 6.6 Canonical Contribution Profit Model
Channel-level commercial profitability is modeled strictly as **Contribution Profit**:
```text
Contribution Profit = Projected Settlement - COGS
Contribution Profit = Gross Revenue - Total Platform Fees - COGS
Contribution Margin % = (Contribution Profit / Gross Revenue) * 100  (when Gross Revenue > 0)
```
- **Persist vs Derive Decision:** 
  - To prevent dual sources of truth and data synchronization discrepancies, `total_cogs` and `contribution_profit` are **derived dynamically** in queries and application read services from immutable `order_items.total_cost` snapshots and `order_fee_snapshots.projected_settlement`.
  - Storing duplicate aggregate columns on `orders` or `order_fee_snapshots` is avoided in MVP.
- **Strict Terminology & Accounting Boundary:**
  - *"Contribution Profit represents order/channel profitability after marketplace fees and COGS, but before corporate operating expenses and taxes."*
  - Calling Contribution Profit *Net Profit*, *Net Income*, or *Operating Profit* is **strictly prohibited**, as company-wide operating expenses (payroll, rent, warehouse storage, marketing campaigns, corporate tax, depreciation) are not deducted.

---

## 7. Persisted vs Derived Fields Summary

| Field | Source / Formula | Storage Strategy | Architectural Rationale |
| :--- | :--- | :---: | :--- |
| `orders.subtotal` | Sum of `order_items` line amounts | **Persisted** | Captures agreed merchandise subtotal upon order placement. |
| `orders.gross_revenue` | `subtotal - shop_voucher` | **Persisted** | Fundamental financial baseline for customer payment. |
| `order_items.line_total` | `quantity * unit_price` | **Persisted** | Line-item gross sales amount. |
| `order_items.unit_cost_snapshot` | Current `product_variants.cost_price` | **Persisted** | Immutable unit cost baseline frozen at purchase. |
| `order_items.total_cost` | `quantity * unit_cost_snapshot` | **Persisted** | Immutable line COGS. |
| `Order COGS (total_cogs)` | `SUM(order_items.total_cost)` | **Derived** | Single source of truth; avoids redundant aggregate storage. |
| `order_fee_snapshots.*_amount` | Applied rate × monetary base | **Persisted** | Immutable snapshot; protects against historical fee rule modifications. |
| `order_fee_snapshots.total_platform_fees` | Sum of platform fee components | **Persisted** | Aggregate fee deduction per order. |
| `order_fee_snapshots.projected_settlement`| `gross_revenue - total_platform_fees` | **Persisted** | Freezes expected payout for manual reconciliation. |
| `Contribution Profit` | `projected_settlement - total_cogs` | **Derived** | Canonical order/channel profitability; zero redundant fields. |
| `Contribution Margin %` | `(Contribution Profit / Gross Revenue) * 100` | **Derived** | Real-time margin efficiency indicator. |
| `reconciliation_records.actual_settlement`| Manual entry from bank/wallet statement | **Persisted** | Actual cash disbursed. |
| `reconciliation_records.variance_amount` | `projected_settlement - actual_settlement`| **Derived / Persisted** | Financial discrepancy; triggers mandatory explanation notes when non-zero. |

---

## 8. Data Retention, Soft Deletes & Auditing Policies

1. **Financial Transaction Immutability:**
   - Delivered orders and finalized `order_fee_snapshots` are read-only.
   - Physical hard deletion of delivered commercial records is strictly prohibited.
2. **Master Catalog Soft Deletes:**
   - `products` and `product_variants` use active flags (`is_active = false`) for operational deactivation, preserving foreign key integrity for historical orders.
3. **Restrict vs Cascade Deletions:**
   - Deletion of master records with historical references is blocked (`ON DELETE RESTRICT`).
   - Cascade deletion is restricted to strictly owned child components (`order_items`, `order_status_history`, `discrepancy_audits`).

---

## 9. Analytics & Reporting Strategy (No Duplicate Report Tables)

To avoid redundant data synchronization and stale aggregation tables, all analytical views query the transactional tables dynamically:
- **No Static Report Tables:** Tables such as `dashboard_kpi`, `daily_revenue`, or `channel_summary` are intentionally omitted. PostgreSQL 16+ indexes support sub-50ms analytical queries for MVP volume ($< 500{,}000$ orders).
- **Approved Analytics Terminology:**
  - *Gross Revenue & Contribution Profit by Channel*
  - *Platform Fee Burden & Breakdown*
  - *Settlement Variance Analysis*
  - *Top SKUs by Revenue and Contribution Profit*
  - *Contribution Margin % Trend*
- **Example Dynamic Query (Executive Financial & Profit Summary):**
  ```sql
  SELECT
      COALESCE(SUM(o.gross_revenue), 0) AS total_gross_revenue,
      COALESCE(SUM(s.total_platform_fees), 0) AS total_platform_fees,
      COALESCE(SUM(s.projected_settlement), 0) AS total_net_realized_revenue,
      COALESCE(SUM(items.total_cogs), 0) AS total_cogs,
      COALESCE(SUM(s.projected_settlement - items.total_cogs), 0) AS total_contribution_profit,
      CASE 
          WHEN SUM(o.gross_revenue) > 0 THEN 
              ROUND((SUM(s.projected_settlement - items.total_cogs) / SUM(o.gross_revenue)) * 100, 2)
          ELSE 0 
      END AS contribution_margin_pct,
      COUNT(o.id) AS delivered_orders_count
  FROM orders o
  JOIN order_fee_snapshots s ON s.order_id = o.id
  JOIN (
      SELECT order_id, SUM(total_cost) AS total_cogs
      FROM order_items
      GROUP BY order_id
  ) items ON items.order_id = o.id
  WHERE o.status = 'DELIVERED'
    AND o.delivered_at IS NOT NULL;
  ```

---

## 10. Traceability Matrix

The schema directly traces to approved business user stories:

| Requirement ID | Requirement Summary | Relational Schema Implementation | Invariants & Constraints |
| :--- | :--- | :--- | :--- |
| **US-CAT-01** | Maintain SKU Pricing & Cost Baseline | `products`, `product_variants` | `retail_price`, `cost_price` checked `>= 0`. Role-restricted; soft deletes via `is_active`. |
| **US-ORD-01** | Create Multi-Item Commercial Orders | `products`, `product_variants`, `orders`, `order_items` | `subtotal`, `shop_voucher`, `gross_revenue`; item cost snapshots. Minimum 1 item rule enforced in app. |
| **US-ORD-02** | Transition Order Lifecycle States | `orders`, `order_status_history`, `order_fee_snapshots` | `status` (`PENDING`, `SHIPPED`, `DELIVERED`, `CANCELLED`). Triggers revenue, fee, and profit recognition at `DELIVERED`. |
| **US-ORD-03** | Order Cancellation with Reason | `orders`, `order_status_history` | `cancelled_at`, mandatory `cancellation_reason`. Cancelled orders contribute 0 to Revenue, COGS, and Profit. |
| **US-FEE-01** | Channel & Payment Fee Schedules | `fee_schedules`, `order_fee_snapshots` | Keyed by `(channel, payment_method)`. Distinguishes POS Cash (0%) vs POS Card/QR (1%). |
| **US-SET-01** | Projected Settlement Freezing | `order_fee_snapshots` | Snapshot created upon delivery; freezes `total_platform_fees` and `projected_settlement`. |
| **US-SET-02** | Manual Payout Reconciliation & Variance | `reconciliation_records`, `discrepancy_audits` | `variance_amount = projected_settlement - actual_settlement`. Mandatory explanation when variance != 0. |
| **US-PROFIT-01**| Calculate COGS & Contribution Profit | `order_items.unit_cost_snapshot`, `order_items.total_cost` | Immutable cost freezing; dynamic derivation `projected_settlement - total_cogs`. Zero for non-delivered. |
| **US-DASH-01** | Executive Financial KPIs | `orders`, `order_fee_snapshots`, `order_items` | Dynamic aggregation of Gross Revenue, Platform Fees, Net Settlement, COGS, and Contribution Profit. |
| **US-DASH-02** | Revenue & Profit Trend Analysis | `orders`, `order_fee_snapshots`, `order_items` | Temporal aggregation of Revenue, Net Settlement, and Contribution Profit grouped by date/channel. |
| **US-DASH-03** | Top SKUs by Revenue & Profit | `order_items`, `product_variants`, `products` | Dynamic aggregation of SKU line revenue, COGS, and contribution profit for delivered orders. |
| **US-DASH-04** | Order Detail Export Capability | `orders`, `order_items`, `order_fee_snapshots` | Transactional data joins filtered by date range. |

---

## 11. Cross-Plan Action Items & Future Extensions

### 11.1 Cross-Plan Synchronizations
1. **Catalog Subsystem Alignment (P03/P04):**
   - P03 Backend Component Architecture incorporates `ProductRepository` and `CatalogService`.
   - P04 Frontend Component Architecture incorporates `/catalog` route, `CatalogPage`, and `ProductSelector`.
2. **Contribution Profit Subsystem Alignment (P03/P04):**
   - P03 `OrderService` freezes baseline unit cost into `OrderItem.UnitCostSnapshot`.
   - P03 `AnalyticsService` aggregates COGS and derives Contribution Profit.
   - P04 displays 5 core financial KPI cards and contribution margin indicators.
3. **Manual Settlement Alignment (P04):**
   - Reconciliations in MVP are performed manually by Finance entering actual disbursed amounts.

### 11.2 Future Scope (Post-MVP)
The following capabilities are excluded from the MVP core schema:
- **`statement_imports`:** Batch CSV/Excel file parser tables for bulk automated settlement reconciliation.
- **Refund & Return Workflows:** Dedicated `RETURNED` / `REFUNDED` statuses, reverse fee adjustments, and return shipping deductions.
- **Advanced Inventory Valuation Engines:** FIFO, LIFO, and Moving Weighted Average valuation engines, automated purchase orders, and warehouse receiving ledgers.
- **Enterprise General Ledger & Net Income:** Store rental leases, payroll/salaries, marketing campaign OPEX, corporate income taxes, and asset depreciation.
- **`users` & RBAC:** Persistent identity and role management.

### 11.3 Deferred to P06 (Database Implementation)
- Detailed partial indexes, B-tree configuration, and storage tuning.
- Trigger implementations (`moddatetime` automated timestamp updates).
- Materialized views and concurrent refresh strategies for high-volume read scale.

---

## 12. Financial Glossary

- **Gross Revenue:** The total customer payable amount for an order (`subtotal - shop_voucher`).
- **Platform Commission:** The percentage fee charged by the marketplace based on the order merchandise subtotal.
- **Payment Fee:** The payment processing charge assessed against the gross revenue collected.
- **Service Fee:** Additional platform fees, either percentage-based (with optional monetary cap) or flat per order.
- **Projected Settlement:** The net amount expected to be disbursed into the merchant wallet (`gross_revenue - total_platform_fees`). Represents Net Realized Revenue.
- **Actual Settlement:** The net cash amount disbursed by the platform or bank statement.
- **Settlement Variance:** The mathematical discrepancy between expected and actual payouts (`projected_settlement - actual_settlement`).
- **Net Realized Revenue:** Equivalent to Projected Settlement; represents commercial net revenue realized after platform fee deductions.
- **COGS (Cost of Goods Sold):** Direct baseline merchandise cost associated with delivered order items, computed as $\sum (\text{Quantity} \times \text{Unit Cost Snapshot})$.
- **Contribution Profit:** Order or channel-level profitability after deducting marketplace fees and direct merchandise COGS from gross revenue (`Gross Revenue - Total Platform Fees - COGS` or `Projected Settlement - COGS`).
- **Contribution Margin %:** Contribution Profit expressed as a percentage of Gross Revenue (`(Contribution Profit / Gross Revenue) * 100`).
- **Strict Naming Distinction:** Contribution Profit must **never** be labeled *Net Profit*, *Net Income*, or *Operating Profit*, as corporate OPEX, overhead, and taxes are not deducted.

