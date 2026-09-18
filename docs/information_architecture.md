# Information Architecture - IA

## Fashion Revenue & Profit Management System
---

## PART 1: INFORMATION ARCHITECTURE (IA)

### 1. Introduction & Architectural Scope
The Information Architecture (IA) of the Fashion Revenue & Profit Management System standardizes commercial data structures, order lifecycles, and financial controls across three channels: TikTok Shop, Shopee, and In-Store POS.

The system is organized around 4 core business domains mapped directly to 4 operational workspaces:
1. Module 0 (Screen 4): Product Catalog & Cost Baseline
2. Module 1 (Screen 1): Multi-Channel Orders Management
3. Module 2 (Screen 2): Marketplace Fees & Wallet Settlement
4. Module 3 (Screen 3): Revenue & Profit Dashboard

> [!NOTE]
> **Canonical Profit Definition:**
> **Contribution Profit** represents order/channel profitability after deducting platform fees and direct Cost of Goods Sold (COGS), but before corporate overhead (rent, payroll, marketing OPEX) and taxes.
> - $\text{Gross Revenue} = \text{Subtotal} - \text{Shop Voucher}$
> - $\text{COGS} = \sum (\text{Quantity} \times \text{Unit Cost Snapshot})$
> - $\text{Projected Settlement (Net Realized Revenue)} = \text{Gross Revenue} - \text{Total Platform Fees}$
> - $\text{Contribution Profit} = \text{Projected Settlement} - \text{COGS}$
> - $\text{Contribution Margin \%} = (\text{Contribution Profit} / \text{Gross Revenue}) \times 100$

---

### 2. Core Business Data Domains

All business data is organized into 4 cohesive domains:

---

#### 2.1. Domain 0: Catalog & Cost Baseline (Screen 4)
* **Purpose:** Maintain master product models, sellable SKU variants, listed retail selling prices, and baseline unit costs for COGS calculation.
* **Field specifications:**
  * `product_id`: Master product style unique identifier (UUID).
  * `product_name`: Master apparel model name (e.g., Slim-fit Linen Shirt).
  * `category`: Apparel category (e.g., Shirts, Pants, Dresses).
  * `sku_id`: Unique SKU variant identifier (UUID).
  * `sku_code`: Unique Stock Keeping Unit code (e.g., SLS-WHT-M).
  * `color`: Color attribute (e.g., White, Navy).
  * `size`: Size attribute (e.g., S, M, L, XL).
  * `retail_price`: Listed catalog retail selling price in VND (>= 0).
  * `cost_price`: Baseline unit Cost of Goods Sold (COGS) in VND (>= 0). *Restricted visibility: visible only to Shop Owner and Finance Manager.*
  * `is_active`: Boolean status controlling catalog sales availability.

---

#### 2.2. Domain 1: Multi-Channel Orders & Line Items (Screen 1)
* **Purpose:** Capture, standardize, and govern order transactions across multi-channel sources.
* **Field specifications:**
  * `order_id`: Unique internal order identifier (UUID / ORD-YYYY-XXXX).
  * `external_order_id`: Channel order ID or POS receipt number (e.g., TT-889922, SP-991122).
  * `channel`: Sales channel (`TIKTOK`, `SHOPEE`, `POS`).
  * `payment_method`: Payment method (`CASH`, `POS_CARD_QR`, `MARKETPLACE_WALLET`).
  * `order_items`: Array of line items, each containing:
    * `sku_code_snapshot`: Immutable snapshot of SKU code at purchase time.
    * `product_name_snapshot`: Immutable snapshot of product title.
    * `quantity`: Purchased quantity (Integer >= 1).
    * `unit_price`: Agreed unit selling price at creation time (Decimal >= 0).
    * `unit_cost_snapshot`: Immutable frozen unit baseline cost from catalog (Decimal >= 0).
    * `line_total`: Line item selling total (= quantity * unit_price).
    * `line_cogs`: Line item merchandise cost (= quantity * unit_cost_snapshot).
  * `subtotal`: Order merchandise subtotal before vouchers (= sum(line_total)).
  * `shop_voucher`: Merchant-funded discount (Decimal >= 0, <= subtotal).
  * `gross_revenue`: Net customer payable amount (= subtotal - shop_voucher).
  * `total_cogs`: Aggregated order COGS (= sum(line_cogs)).
  * `order_status`: Order state (`PENDING`, `SHIPPED`, `DELIVERED`, `CANCELLED`). Only `DELIVERED` orders are recognized as official revenue and profit.
  * `cancellation_reason`: Mandatory explanation note when transitioned to `CANCELLED`.
  * `delivered_at`: Timestamp of successful delivery used for period accounting.

---

#### 2.3. Domain 2: Marketplace Fees & Wallet Settlement (Screen 2)
* **Purpose:** Automatically calculate channel fee deductions, project net settlement, and reconcile against bank payouts.
* **Field specifications:**
  * `order_id`: Foreign key linked to the corresponding order.
  * `commission_fee`: Marketplace commission fee:
    * TikTok Shop: 4.0% * subtotal
    * Shopee: 4.5% * subtotal
    * POS In-Store: 0 VND
  * `payment_fee`: Payment processing fee:
    * TikTok Shop: 3.0% * gross_revenue
    * Shopee: 4.0% * gross_revenue
    * POS Card/QR: 1.0% * gross_revenue; POS Cash: 0 VND
  * `service_fee`: Platform service fee:
    * TikTok Shop: 3,000 VND/order (fixed fee)
    * Shopee: 2.0% * subtotal (service fee, capped per fee schedule)
    * POS In-Store: 0 VND
  * `total_platform_fees`: Sum of all platform deductions (= commission + payment + service + fixed).
  * `projected_settlement`: Expected net cash payout (= gross_revenue - total_platform_fees). Represents Net Realized Revenue.
  * `actual_settlement`: Actual payout received in bank/wallet statement (entered manually by Finance).
  * `variance_amount`: Settlement discrepancy (= projected_settlement - actual_settlement).
  * `reconciliation_status`: Settlement status (`PENDING_SETTLEMENT`, `RECONCILED`, `DISCREPANCY`).
  * `reconciliation_notes`: Mandatory explanation note required when variance_amount != 0.

---

#### 2.4. Domain 3: Revenue & Profit Dashboard (Screen 3)
* **Purpose:** Provide real-time financial performance, channel profitability, and merchandise margin insights.
* **Field specifications:**
  * **5 Core Financial KPI Cards** (aggregated strictly from `DELIVERED` orders within selected period):
    1. `Gross Revenue`: sum(gross_revenue)
    2. `Total Platform Fees`: sum(total_platform_fees)
    3. `Net Realized Revenue`: sum(projected_settlement)
    4. `Total COGS`: sum(total_cogs)
    5. `Contribution Profit`: sum(projected_settlement - total_cogs)
  * **Secondary Operational Metric:** Delivered Orders count.
  * **Channel Contribution Breakdown:** Gross Revenue, Fees, and Contribution Profit by channel (`TIKTOK`, `SHOPEE`, `POS`).
  * **Top 5 SKU Leaderboard:** Top 5 products ranked by Gross Revenue, Total COGS, Contribution Profit, and Contribution Margin %.
  * **CSV Audit Export Dataset:** Detailed transaction ledger including COGS and Contribution Profit metrics for financial auditing.

---

### 3. Data Integrity & Financial Control Matrix

| Business Rule | Detailed Data Constraint | Financial Control Objective |
|---|---|---|
| **Anti-Phantom Revenue & Profit** | Only `DELIVERED` orders are aggregated into KPIs and charts. `PENDING`, `SHIPPED`, and `CANCELLED` orders contribute 0. | Prevents unearned revenue/profit recognition and distorted business metrics. |
| **Immutable Purchase & Cost Snapshots** | Once order is placed, SKU code, product title, unit price, and unit cost snapshot are frozen. Master catalog price/cost edits never alter historical orders. | Guarantees audit compliance and historical margin reporting integrity. |
| **Voucher Constraint** | `shop_voucher` must be >= 0 and <= `subtotal`. | Prevents input errors resulting in negative order totals. |
| **Mandatory Discrepancy Note** | When reconciling, if `variance_amount != 0`, `reconciliation_notes` is strictly required. | Enforces operational accountability for carrier surcharges or platform fee mismatches. |
| **Instant In-Store Fulfillment** | POS orders transition directly to `DELIVERED` upon cash/card payment. | Reflects immediate in-store take-home fulfillment and instant revenue recognition. |
| **Cost Visibility Segregation** | Baseline unit cost and Contribution Profit are restricted from Sales & Ops staff. | Protects proprietary wholesale sourcing costs and commercial profit margins. |

---

## PART 2: SCREENS HIERARCHY

### 1. Screen Hierarchy Overview
The screen hierarchy follows the 3-click rule and enforces strict role separation:

* **Level 0 (Global Shell & Topbar):** Persistent top-level navigation, branding, and role context.
* **Level 1 (Main Screens):** 4 Independent workspaces aligned with core business functions.
* **Level 2 (View Panels & Tables):** Structured presentation panels, filters, and data tables.
* **Level 3 (Modals & Drawers):** Focused modal overlays for transactional workflows.
* **Level 4 (Interactive Feedback):** Instant UI toast confirmations for completed actions.

---

### 2. Hierarchical Workspace Specifications

#### 2.1. Level 0: Global Shell & Topbar
* Persistent topbar containing: Brand title, active user context with assigned role, and quick role switcher.
* Navigation tab bar providing seamless switching across the 4 Level 1 screens:
  1. `Orders Management`
  2. `Fees & Settlement`
  3. `Revenue & Profit Dashboard`
  4. `Product Catalog & Cost`

#### 2.2. Level 1 & Level 2: 4 Main Workspaces

##### Screen 1: Orders Management (SCR-01)
* **Primary Users:** Sales & Ops Staff, Shop Owner.
* **Panel 1.1 (Toolbar & Filters):** Status filter tabs (`All`, `Pending`, `Shipped`, `Delivered`, `Cancelled`), quick search (Order ID, SKU, customer name), and action button to open MOD-01.
* **Panel 1.2 (Multi-Channel Orders Table):** Master table listing Order ID, channel badge, order date, customer, items summary, gross revenue, and current status. Row actions provide status transitions (`Ship`, `Deliver`) and button to open MOD-02 cancellation modal.

##### Screen 2: Fees & Settlement (SCR-02)
* **Primary Users:** Finance Manager, Shop Owner.
* **Panel 2.1 (Settlement Counters):** KPI counters (`Pending Settlement`, `Reconciled`, `Discrepancy`) with date-range and status filters.
* **Panel 2.2 (Fee Breakdown & Reconciliation Table):** Granular columns for commission fee, payment fee, service fee, total platform fees, projected settlement, actual received amount, and variance. Row action opens MOD-03 settlement modal.

##### Screen 3: Revenue & Profit Dashboard (SCR-03)
* **Primary Users:** Shop Owner, Finance Manager.
* **Panel 3.1 (Global Filters & Actions):** Time presets (Today, Last 7 Days, This Month, Custom Date Range), channel dropdown filter, and CSV export action.
* **Panel 3.2 (Executive Financial KPI Cards):** Displays Gross Revenue, Total Platform Fees, Net Realized Revenue, Total COGS, and Contribution Profit. Secondary badge displays Delivered Orders count.
* **Panel 3.3 (Visual Analytics Panels):** Donut Chart showing revenue contribution by channel (%) and Top 5 SKUs leaderboard displaying Gross Revenue, Total COGS, Contribution Profit, and Contribution Margin %.

##### Screen 4: Product Catalog & Cost Management (SCR-04)
* **Primary Users:** Shop Owner, Finance Manager. *(Hidden or read-only without cost for Sales & Ops)*.
* **Panel 4.1 (Catalog Toolbar & Search):** Search by product name or SKU, filter by active status, and action button to open MOD-05.
* **Panel 4.2 (SKU Master Table):** Lists Product Name, SKU code, Color, Size, Retail Price, Baseline Unit Cost, Active Status, and Edit Action.

#### 2.3. Level 3: Transactional Modals
* **MOD-01 (Create Order Modal):** Form for channel selection, payment method, dynamic SKU selector (auto-populates unit selling price; baseline cost is frozen in backend), shop voucher input, and fee preview.
* **MOD-02 (Cancel Order Modal):** Order summary, mandatory cancellation reason dropdown, and explicit warning regarding revenue/profit exclusion.
* **MOD-03 (Wallet Settlement Modal):** Compares projected settlement vs actual bank/wallet payout, computes variance, and enforces mandatory justification note when variance != 0.
* **MOD-04 (Source Order Drilldown Modal):** Triggered by clicking any KPI card, displaying constituent `DELIVERED` orders with financial breakdown and CSV export.
* **MOD-05 (Product & SKU Editor Modal):** Form to create or edit product model, SKU variants, retail selling price, and baseline unit cost (`cost_price`).

---

### 3. Role-Based Access Control Matrix (RBAC Matrix)

| Hierarchy Level | Screen / Modal Component | Sales & Ops Staff | Finance Manager | Shop Owner |
|---|---|:---:|:---:|:---:|
| **Level 1** | **Screen 1: Orders Management (SCR-01)** | Full Access | View Only | Full Access |
| Level 3 | • Create Order Modal (MOD-01) | Allowed (Cost Hidden) | Hidden | Allowed |
| Level 3 | • Cancel Order Modal (MOD-02) | Allowed | Hidden | Allowed |
| **Level 1** | **Screen 2: Fees & Settlement (SCR-02)** | No Access (Hidden) | Full Access | Full Access |
| Level 3 | • Wallet Settlement Modal (MOD-03) | No Access | Allowed | Allowed |
| **Level 1** | **Screen 3: Revenue & Profit Dashboard (SCR-03)** | No Access (Hidden) | Full View | Full View |
| Level 2 | • Export CSV Report (Panel 3.1) | No Access | Allowed | Allowed |
| Level 3 | • Source Order Drilldown Modal (MOD-04) | No Access | Allowed | Allowed |
| **Level 1** | **Screen 4: Product Catalog & Cost (SCR-04)** | No Access (Hidden) | Full Access | Full Access |
| Level 3 | • Product & SKU Editor Modal (MOD-05) | No Access | Allowed | Allowed |
