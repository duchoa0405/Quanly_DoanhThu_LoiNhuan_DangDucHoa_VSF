# UI/UX Specifications & Design System

## FASHION-WEB - Multi-Channel Revenue & Settlement Management System

---

## 1. Design Philosophy & Numerical Typography Rules

* **Overall Style:** Enterprise B2B SaaS console design (inspired by Salesforce Lightning and FlowX architectures). Strictly minimalist, utilitarian, and free of superficial AI gradients, neon borders, or blurred dropshadows.
* **High Information Density:** Employs compact capsule filter pills, flat clean metric cards, and sharp 1px hairline border data tables.
* **Financial Numerical Typography Rule:**
  * **Crimson Red (`#c5221f`):** Exclusively applied to costs, deductions, and liabilities borne by the merchant (platform commission fees, payment processing fees, fixed/shipping service fees, shop vouchers, carrier weight penalties/negative variances) and loss counts (`3 orders` discrepancy, cancelled orders).
  * **Uniform Dark Neutral (`#0f172a`):** Applied to ALL other standard figures (Gross sales, net settlement payout, total order volume, delivered order count, in-transit count, average order value, SKU sales quantities). No green or blue text is used for standard numbers, ensuring corporate accounting rigor.
* **Interactive Code Artifact:** Fully functional standalone HTML/CSS/JS prototype available at [docs/stitch_prototype.html](stitch_prototype.html) ready for immediate insertion into Stitch.

---

## 2. Design System Tokens

### 2.1. Curated Enterprise Color Palette
| Token Name | HEX Value | Scope & Usage |
|---|---|---|
| App Canvas Background | `#f4f5f7` | Soft neutral background creating contrast with white cards |
| Surface Background | `#ffffff` | Metric cards, data tables, modals, toolbars |
| Hairline Borders | `#e2e8f0` / `#cbd5e1` | 1px clean dividing lines between panels |
| Primary Text | `#0f172a` | Titles and ALL standard numerical metrics |
| Secondary / Muted Text | `#475569` / `#64748b` | Column headers, field descriptions, hints |
| Primary Brand Blue | `#0052cc` | Primary buttons, active tabs, clickable order links |
| Success Completed Status | `#107c41` (Tint `#e6f4ea`) | DELIVERED status badge, 100% Reconciled badge |
| Warning Pending Status | `#b06000` (Tint `#fef7e0`) | PENDING status badge, Pending Settlement badge |
| Danger / Cost Status | `#c5221f` (Tint `#fce8e6`) | Merchant costs, platform fees, variance, cancelled orders |

### 2.2. Typography & Corner Radius
* **Typeface:** `Inter`, `-apple-system`, `Segoe UI`, `Roboto`, sans-serif.
* **Type Scale:** Screen Titles `22px` (Bold 700), Section Titles `13px - 14px` (SemiBold 600), Big KPI Numbers `24px` (Bold 700), Tables & Forms `12px`, Badges/Column Headers `11px` (Uppercase).
* **Corner Radius:** Buttons & Inputs `4px`, Cards & Table Containers `6px`, Capsule Filter Pills `9999px`.

---

## 3. Screen Layout Specifications

### 3.1. Global Shell (Level 0)
* **Top Header Bar (48px):** Brand badge `FSW`, omnisearch input `Ctrl+K` (Order ID, SKU, customer name), persona switcher (`Sales/Ops` | `Finance` | `Shop Owner`) with staff avatar.
* **Horizontal Navigation Bar (40px):** 3 primary tabs: `Orders Management (SCR-01)`, `Fees & Settlement (SCR-02)`, `Revenue Dashboard (SCR-03)`, with real-time multi-channel connection indicators.
* **Slim Sidebar Rail (54px):** Quick-access shortcuts for Orders, Settlement, Reports, and Settings.

---

### 3.2. Screen 1: Multi-Channel Orders Management (SCR-01)

![Screen 1: Multi-Channel Orders Management](screenshots/scr_1.png)

* **Top Action Bar:**
  * Primary Action: **`[+ Create New Order]`** (Brand Blue `#0052cc`, prominent button): Triggers modal MOD-01 for manual order entry during livestreams, hotlines, in-store POS, or before API integrations.
  * View Switchers: `[Customer Switch]`, `[Kanban View]`, `[Dashboard View]`, and `[Refresh]`.
* **Capsule Filter Bar:** Filter by Channel (`TikTok Shop`, `Shopee`, `In-Store POS`), Order Status (`Pending`, `Shipped`, `Delivered`, `Cancelled`), and Period.
* **5 Operational Metric Cards:** Total Orders (1,248), Delivered Orders (1,180), Gross Revenue (184.5M ₫), In Transit (42), Cancelled Orders (26). All numbers use uniform dark text `#0f172a`.
* **Operational Rhythm Strip:** 6 real-time progress indicators: Awaiting Prep (14), Courier Transit (42), Delivered (1,180), Completion Rate (94.5%), AOV (156k ₫), Cancellation Rate (2.1% with overdue order alert).
* **Charts & Priority Order Cards:**
  * *Left Column:* Vertical Bar Chart showing order distribution by channel (TikTok 560 orders - 45%, Shopee 480 orders - 38%, POS 208 orders - 17%).
  * *Right Column:* Highest-value order watchlist requiring fulfillment attention (Largest Orders).
* **Master Orders Data Table:** Order ID, Channel Badge, External ID, Timestamp, Customer, SKU Line Items, Customer Payment, Order Status, Inline Quick Actions (`[Ship]`, `[Delivered]`, `[Cancel]`).

---

### 3.3. Screen 2: Marketplace Fees & Wallet Settlement (SCR-02)

![Screen 2: Marketplace Fees & Wallet Settlement](screenshots/scr_2.png)

* **Settlement Filters:** Filter by Reconciliation Status (`Pending Settlement`, `100% Reconciled`, `Discrepancy`), Settlement Period, and Channel.
* **3 Settlement Summary Cards:** Pending Settlement (18 orders), 100% Reconciled (1,159 orders), Discrepancy (3 orders).
* **Automated Fee Schedule Strip:** Displays automated fee formulas (TikTok 4% Comm + 3% Pay + 2,000đ; Shopee 4.5% Comm + 4% Pay + 2% Freeship; POS 1% Card/QR).
* **Granular Fee Deduction Ledger Table:**
  * Order ID, Channel, Paid Amount.
  * Merchant-Borne Costs (Crimson Red `#c5221f`): Commission Fee, Payment Processing Fee, Service/Freeship Fee, Total Platform Fees.
  * Realized Cash Flow (Uniform Dark `#0f172a`): Projected Net Settlement, Actual Net Received.
  * Variance: Highlighted in red if discrepancies/penalties occur (e.g., `-5.000 ₫`).
  * Reconciliation Status (100% Matched, Discrepancy, Pending Arrival).
* **Discrepancy Resolution Audit Panel:** Displays the latest audit resolution case (#DIS-002) with root-cause explanations from carrier/platform re-weighing penalties for financial transparency.

---

### 3.4. Screen 3: Revenue Dashboard & Executive Reporting (SCR-03)

![Screen 3: Revenue Dashboard & Executive Reporting](screenshots/scr_3.png)

* **4 Executive KPI Cards:**
  * *Total Gross Revenue:* `184.500.000 ₫` (strictly calculated on 1,180 DELIVERED orders).
  * *Total Platform Fees Deducted:* `28.620.000 ₫` (Crimson Red `#c5221f` - 15.5% deduction rate).
  * *Net Settlement Realized:* `155.880.000 ₫` (Uniform Dark `#0f172a` - net cash after all fee subtractions).
  * *Delivered Orders:* `1,180 orders` (Uniform Dark `#0f172a` - 94.5% fulfillment rate).
* **Financial Integrity Banner:** Accounting rule reminder: Only DELIVERED orders are recognized in revenue; PENDING, SHIPPED, and CANCELLED orders are 100% excluded to prevent phantom revenue.
* **Grouped Bar Chart (7-Day Cash Flow Trend):** Daily side-by-side comparison of Gross Revenue (Blue Bar) vs Net Realized Cash (Green Bar) to track platform deduction volatility.
* **Two-Column Analytics Panel:**
  * *Left Column:* Donut Chart showing revenue contribution by channel (TikTok 48.0%, Shopee 37.0%, POS 15.0%) with total revenue `184.5M ₫` at the center. Clickable slices open the Drilldown Modal.
  * *Right Column:* Leaderboard ranking Top 5 Best-Selling SKUs by volume and gross revenue.

---

## 4. Modal Window Specifications (Modals & Action Drawers)

Action windows supporting real-world operational workflows triggered by interface buttons:

* **MOD-01: Create Order Modal (Triggered by `[+ Create New Order]` on SCR-01):**
  * *Operational Purpose:* Enables staff to manually record orders taken via livestreams, hotlines, direct messages, in-store POS, or before marketplace API integrations are activated.
  * *Form Fields:*
    * Sales Channel (`TikTok Shop`, `Shopee`, `In-Store POS`) & External Order ID (`ext_order_id`).
    * Customer Name & Payment Method (`Cash`, `Card/QR`, `Marketplace Wallet`).
    * Product Line Items: SKU Code, Quantity, Unit Price.
    * Shop Voucher Discount (`shop_voucher`).
  * *Real-Time Cash Flow & Fee Preview:*
    * Automatically calculates: Subtotal -> Less Shop Voucher (Red) -> Gross Payment (Dark) -> Itemized Platform Fees for comm/pay/service (Red) -> Projected Net Settlement (Dark).
    * POS orders with cash/card payment automatically transition to `DELIVERED` upon saving; marketplace orders enter `PENDING` awaiting fulfillment.
* **MOD-02: Order Cancellation Confirmation Modal (Triggered by `[Cancel]` on SCR-01):**
  * *Purpose:* Controls cancellations and prevents false revenue recognition.
  * *Content:* Displays order summary; requires selecting cancellation reason (customer unreachable, wrong address, out of stock, customer request) and entering an explanation note. Alerts that cancelled orders are 100% excluded from revenue and KPIs.
* **MOD-03: Bank Statement Import Modal (Triggered by `[Import Statement (.xlsx)]` on SCR-02):**
  * *Purpose:* Uploads bank transaction statement files or TikTok/Shopee wallet payout reports (.xlsx, .csv) for automated matching and variance identification.
* **MOD-04: Fee Schedule Configuration Modal (Triggered by `[Configure Fees >]` on SCR-02):**
  * *Purpose:* Dynamically updates platform commission rates, payment fees, fixed fees, and Freeship Xtra charges across channels (Strategy Pattern).
* **MOD-05: Source Order Drilldown Modal (Triggered by `[Trace Source Orders]` or clicking Donut Chart on SCR-03):**
  * *Purpose:* Auditing tool displaying granular itemized DELIVERED orders contributing to revenue KPIs (Gross, Platform Fees, Net Payout) with CSV export capability.

*Multi-Channel Operational Note:* The architecture supports a hybrid workflow: automated ingestion via API/Webhooks where available, paired with mandatory manual order creation (MOD-01) to flexibly handle unintegrated channels, livestreams, and walk-in sales without depending on complex TikTok/Shopee developer approvals.
