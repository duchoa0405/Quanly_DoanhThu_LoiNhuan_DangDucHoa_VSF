# Information Architecture - IA

## Multi-Channel Revenue Management System FASHION-WEB
---

## PART 1: INFORMATION ARCHITECTURE (IA)

### 1. Introduction & Architectural Scope
The Information Architecture (IA) of the FASHION-WEB system standardizes data structures, lifecycle flows, and cash flow control rules across three channels: TikTok Shop, Shopee, and In-Store POS.

The system is organized around 3 core business domains mapped directly to 3 operational screens:
1. Module 1 (Screen 1): Multi-Channel Orders Management
2. Module 2 (Screen 2): Marketplace Fees & Wallet Settlement
3. Module 3 (Screen 3): Revenue Dashboard & Analytics

SKU details, quantities, and unit prices are captured directly within line items of each order, eliminating the need for a separate SKU catalog screen.

---

### 2. Core Business Data Domains

All business data in FASHION-WEB is organized into 3 domains:

![Three Core Business Data Domains](screenshots/Multi-Channel%20Revenue-domain.png)

---

#### 2.1. Domain 1: Multi-Channel Orders & Line Items (Screen 1)
* Purpose: Capture, standardize, and govern order transactions across channels.
* Field specifications:
  * `order_id`: Unique internal order identifier (e.g., ORD-2026-001).
  * `external_order_id`: Channel order ID or POS receipt number (e.g., TT-889922, SP-991122).
  * `channel`: Sales channel (`TIKTOK`, `SHOPEE`, `POS`).
  * `payment_method`: Payment method (`CASH`, `POS_CARD_QR`, `MARKETPLACE_WALLET`).
  * `order_items`: Array of line items, each containing:
    * `sku_code`: SKU variant code (e.g., AT-COT-DEN-L).
    * `product_name`: Product title summary.
    * `quantity`: Quantity purchased (Integer >= 1).
    * `unit_price`: Selling price at creation time (Decimal > 0).
    * `line_total`: Line item total (= quantity * unit_price).
  * `subtotal`: Order subtotal before voucher (= sum(line_total)).
  * `shop_voucher`: Shop-funded discount (Decimal >= 0, <= subtotal).
  * `gross_revenue`: Gross customer payment (= subtotal - shop_voucher).
  * `order_status`: Order state (`PENDING`, `SHIPPED`, `DELIVERED`, `CANCELLED`). Only `DELIVERED` orders are recognized as official revenue.
  * `cancel_reason`: Mandatory cancellation note when transitioned to `CANCELLED`.
  * `delivered_at`: Timestamp of successful delivery used for period accounting.

---

#### 2.2. Domain 2: Marketplace Fees & Wallet Settlement (Screen 2)
* Purpose: Automatically calculate channel fee deductions, project net settlement, and reconcile against bank payouts.
* Field specifications:
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
    * TikTok Shop: 2,000 VND/order (fixed fee)
    * Shopee: 2.0% * subtotal (Freeship Xtra, capped at 20,000 VND/order)
    * POS In-Store: 0 VND
  * `total_platform_fees`: Sum of all platform deductions (= commission_fee + payment_fee + service_fee).
  * `projected_net_settlement`: Expected net cash payout (= gross_revenue - total_platform_fees).
  * `actual_settlement_amount`: Actual payout received in bank statement (entered by finance).
  * `variance`: Settlement difference (= projected_net_settlement - actual_settlement_amount).
  * `reconciliation_status`: Settlement status (`PENDING_SETTLEMENT`, `RECONCILED`, `DISCREPANCY`).
  * `reconciliation_notes`: Mandatory explanation note required when variance != 0.

---

#### 2.3. Domain 3: Analytics & Executive Reporting (Screen 3)
* Purpose: Provide real-time financial metrics and channel performance insights.
* Field specifications:
  * 3 High-level KPI Cards (aggregated strictly from `DELIVERED` orders within selected period):
    1. Gross Revenue: sum(gross_revenue)
    2. Total Platform Fees: sum(total_platform_fees)
    3. Net Realized Revenue: sum(projected_net_settlement)
  * Channel Breakdown: Revenue and percentage contribution per channel (`TIKTOK`, `SHOPEE`, `POS`).
  * Top 5 SKU Leaderboard: Top 5 products ranked descending by sales volume and revenue.
  * CSV Export Dataset: Detailed transaction ledger with reconciliation metrics for audit.

---

### 3. Data Lifecycle Flow

Financial data flows sequentially across 5 operational phases:

![End-to-End Data Lifecycle Flow](screenshots/Multi-Channel%20Revenue-endtoend.png)

---

### 4. Data Integrity & Control Matrix

| Business Rule | Detailed Data Constraint | Financial Control Objective |
|---|---|---|
| Anti-Phantom Revenue | Only DELIVERED orders are aggregated into KPIs and charts. PENDING, SHIPPED, and CANCELLED orders are 100% excluded. | Prevents unearned revenue recognition and incorrect tax liabilities. |
| Immutability of Finalized Orders | Once DELIVERED, order line items, prices, and quantities are locked from modification. | Ensures audit compliance and financial ledger integrity. |
| Voucher Constraint | shop_voucher must be >= 0 and <= subtotal. | Prevents input errors resulting in negative order totals. |
| Mandatory Discrepancy Note | When reconciling, if variance != 0, reconciliation_notes is strictly required. | Enforces accountability for carrier surcharges or platform penalties. |
| Instant In-Store Fulfillment | POS orders transition directly to DELIVERED upon payment, skipping the SHIPPED state. | Reflects immediate in-store take-home fulfillment. |

---

## PART 2: SCREENS HIERARCHY

### 1. Screen Hierarchy Objectives & Principles
The screen hierarchy of FASHION-WEB follows the 3-click rule and enforces clear separation of operational roles:

* Level 0 (Global Shell & Topbar): Persistent top-level navigation, branding, and connectivity status.
* Level 1 (Main Screens): 3 Independent workspaces aligned with core business functions.
* Level 2 (View Panels & Tables): Structured presentation panels, filters, and data tables.
* Level 3 (Modals & Drawers): Focused modal overlays for transactional workflows.
* Level 4 (Interactive Feedback & Toasts): Instant UI confirmations for completed actions.

---

### 2. Screens Hierarchy Tree

![Screens Hierarchy Tree FASHION-WEB](screenshots/Multi-Channel%20Revenue-Screens%20Hierarchy%20Tree.png)


---

### 3. Hierarchical Specifications

The application interface consists of 5 interactive layers:

#### 3.1. Level 0: Global Shell & Topbar
* Persistent topbar containing: FASHION-WEB branding, workflow mode tag (`Workflow: Manual Entry & Assisted Audit`), and active user context with assigned role.
* Navigation tab bar providing zero-latency switching across the 3 primary Level 1 screens: `Orders Management`, `Fees & Settlement`, `Revenue Dashboard`.

#### 3.2. Level 1 & Level 2: 3 Main Screens & View Panels

##### Screen 1: Orders Management (SCR-01)
* Primary Users: Sales/Ops Staff, Shop Owner.
* Panel 1.1 (Toolbar & Filters): Status filter tabs (All, Pending, Shipped, Delivered, Cancelled), quick search (Order ID, SKU, customer name), and action button to open MOD-01.
* Panel 1.2 (Multi-Channel Orders Table): Master table listing Order ID, channel badge, creation date, customer, items summary, paid total, and current status. Row actions provide quick status transitions (Ship, Deliver) and button to open MOD-02 cancellation modal.

##### Screen 2: Fees & Settlement (SCR-02)
* Primary Users: Finance Manager, Shop Owner.
* Panel 2.1 (Settlement Counters): 3 KPI counters (Pending Settlement, Reconciled 100%, Discrepancy) with date-range and status filters.
* Panel 2.2 (Fee Breakdown & Reconciliation Table): Granular columns for commission fee, payment fee, service fee, total platform fees, projected net, actual received amount, and variance. Row action opens MOD-03 settlement modal.

##### Screen 3: Revenue Dashboard (SCR-03)
* Primary Users: Shop Owner, Finance Manager.
* Panel 3.1 (Global Filters & Actions): Time presets (Today, Last 7 Days, This Month, Custom Date Range), channel dropdown filter, and CSV export action.
* Panel 3.2 (Executive KPI Cards): Displays Gross Revenue (DELIVERED orders only), Total Platform Fees, and Net Realized Revenue.
* Panel 3.3 (Visual Analytics Panels): Donut Chart illustrating revenue contribution by channel (%) and Top 5 best-selling SKUs leaderboard.

#### 3.3. Level 3: 4 Transactional Modals
* MOD-01 (Create Order Modal): Form for channel selection, payment method, dynamic SKU line items (SKU, name, quantity, price), shop voucher input, and instant fee preview before saving.
* MOD-02 (Cancel Order Modal): Order summary, cancellation reason dropdown (unreachable customer, address error, out of stock), and explicit warning regarding revenue exclusion.
* MOD-03 (Wallet Settlement Modal): Compares projected net vs actual bank payout, computes variance, and enforces mandatory justification note when variance != 0.
* MOD-04 (Source Order Drilldown Modal): Triggered by clicking any KPI card or Donut chart slice, displaying constituent DELIVERED orders with full details and CSV export.

#### 3.4. Level 4: Interactive Feedback (Toast Notifications)
* Instant status confirmation toasts: T1 (Order created), T2 (Status transitioned), T3 (Settlement recorded / discrepancy flagged), and T4 (CSV report exported).

---

### 4. Role-Based Access Control Matrix (RBAC Matrix)

| Hierarchy Level | Screen / Modal Component | Sales & Ops Staff | Finance Manager | Shop Owner |
|---|---|:---:|:---:|:---:|
| Level 1 | Screen 1: Orders Management (SCR-01) | Full Access | View Only | Full Access |
| Level 3 | • Create Order Modal (MOD-01) | Allowed | Hidden | Allowed |
| Level 3 | • Cancel Order Modal (MOD-02) | Allowed | Hidden | Allowed |
| Level 1 | Screen 2: Fees & Settlement (SCR-02) | No Access (Hidden) | Full Access | Full Access |
| Level 3 | • Wallet Settlement Modal (MOD-03) | No Access | Allowed | Allowed & Approve |
| Level 1 | Screen 3: Revenue Dashboard (SCR-03) | No Access (Hidden) | Full View | Full View |
| Level 2 | • Export CSV Report (Panel 3.1) | No Access | Allowed | Allowed |
| Level 3 | • Source Order Drilldown Modal (MOD-04) | No Access | Allowed | Allowed |
