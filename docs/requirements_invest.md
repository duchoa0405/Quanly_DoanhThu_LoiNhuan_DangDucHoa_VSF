# INVEST Requirements Specification

## Fashion Revenue & Profit Management System
---

## 1. Introduction & INVEST Principles

This document standardizes the functional requirements of the system into **User Stories**, strictly adhering to the 6 criteria of the **INVEST** framework:

1. **Independent:** Each User Story is decoupled from others, allowing standalone scheduling, development, and release without blocking dependencies.
2. **Negotiable:** The story captures the core business intent, allowing technical implementation and UI details to be refined collaboratively between developers, testers, and stakeholders.
3. **Valuable:** Every story delivers tangible, quantifiable value to an end-user role (Sales/Ops, Finance Manager, Shop Owner).
4. **Estimable:** Scope, constraints, and business rules are sufficiently defined for engineering teams to estimate Story Points and implementation effort.
5. **Small:** Stories are sized appropriately to be completed within 1-3 engineering days (1 sprint iteration), avoiding oversized Epics.
6. **Testable:** All Acceptance Criteria are formalized using Gherkin syntax (`Given... When... Then...`), serving as direct specifications for Unit, Integration, and E2E tests.

> [!NOTE]
> **Canonical Profit Definition:**
> In this system, **Contribution Profit** represents order/channel profitability after deducting marketplace fees and Cost of Goods Sold (COGS), but before corporate operating expenses (rent, payroll, marketing) and taxes.
> - `Gross Revenue = Subtotal - Shop Voucher`
> - `COGS = Σ(Quantity × Unit Cost Snapshot)`
> - `Projected Settlement = Gross Revenue - Total Platform Fees`
> - `Contribution Profit = Projected Settlement - COGS = Gross Revenue - Total Platform Fees - COGS`
> - `Contribution Margin % = (Contribution Profit / Gross Revenue) × 100` (when `Gross Revenue > 0`)
> 
> *Contribution Profit is NOT equivalent to corporate accounting Net Profit or Net Income.*

---

## 2. System User Stories Master Table

| Module | Story ID | User Story Title | Primary Role | Screen | Priority |
|---|---|---|:---:|---|:---:|
| **M00: Product Catalog & Cost Baseline** | `US-CAT-01` | Maintain Product SKU Pricing & Cost Baseline | Shop Owner / Finance Manager | Screen 4: Product Catalog & Cost Management | P1 (Must) |
| **M01: Multi-Channel Order Management** | `US-ORD-01` | Ingest Multi-Channel Multi-Line Orders | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| | `US-ORD-02` | Track Order Lifecycle & Official Revenue & Profit Recognition | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| | `US-ORD-03` | Cancel Orders & Exclude from Revenue & Profit | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| **M02: Sales Fees & Settlement** | `US-FEE-01` | Preview fees by sales channel and payment method, including in-store purchases | Finance Manager / Shop Owner | Screen 1 & Screen 2 | P1 (Must) |
| | `US-SET-01` | Transparent Platform Fee Breakdown & Projected Net | Finance Manager | Screen 2: Fees & Settlement | P1 (Must) |
| | `US-SET-02` | Settlement Audit & Actual Wallet Deposit Update | Finance Manager | Screen 2: Fees & Settlement | P2 (Should) |
| **M03: Cost & Profit Calculation Engine** | `US-PROFIT-01` | Freeze COGS Snapshot & Calculate Order Contribution Profit | Shop Owner / Finance Manager | System Core / Screen 2 & 3 | P1 (Must) |
| **M04: Revenue & Profit Analytics** | `US-DASH-01` | Monitor Core Financial KPI Cards (Revenue, Fees, COGS, Profit) | Shop Owner | Screen 3: Revenue Dashboard | P1 (Must) |
| | `US-DASH-02` | Filter Revenue & Profit Analytics by Date Range & Channel | Finance / Owner | Screen 3: Revenue Dashboard | P1 (Must) |
| | `US-DASH-03` | Visualize Channel Share & Top SKU by Revenue and Profit | Shop Owner | Screen 3: Revenue Dashboard | P2 (Should) |
| | `US-DASH-04` | Drilldown to Source Orders & Export CSV Audit Log | Finance Manager | Screen 3: Revenue Dashboard | P2 (Should) |

---

## 3. Detailed User Stories (INVEST Standard)

---

### MODULE M00: PRODUCT CATALOG & COST BASELINE

#### US-CAT-01: Maintain Product SKU Pricing & Cost Baseline
- **User Story:**
  - **As a:** Shop Owner / Finance Manager,
  - **I want to:** Configure product catalog styles, SKU variants, retail selling prices, and baseline unit costs,
  - **So that:** Order creation has an authoritative pricing source and historical transactions have a validated cost baseline for profit analysis.

- **INVEST Assessment:**
  - **I (Independent):** Self-contained master catalog domain; consumable by order creation and analytics.
  - **N (Negotiable):** Variant dimensions (color, size) can be optional for single-model items.
  - **V (Valuable):** Establishes the commercial baseline for both selling price and Cost of Goods Sold (COGS).
  - **E (Estimable):** Standard Master-Detail CRUD with role-based field authorization; Estimated Effort: 4 Story Points.
  - **S (Small):** Completable within 1.5 engineering days.
  - **T (Testable):** Verifiable through catalog validation rules (`cost_price >= 0`, `sku` uniqueness).

- **Business Rules & Security Constraints:**
  - `cost_price` must be `>= 0`.
  - `retail_price` must be `>= 0`. Selling price is **not** required to be `>= cost_price` to support promotional clearance/loss-leader sales.
  - **Role-Based Visibility:** Sales & Ops personnel can view and select SKUs and retail prices when creating orders, but **cannot view or modify baseline unit costs**. Only Shop Owner and Finance Manager have permission to view and update `cost_price`.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Create product variant with valid baseline cost
  Given User is authenticated as "Shop Owner"
  When User creates product "Linen Shirt" with SKU "LS-WHT-L"
  And Sets retail selling price to 250,000 VND
  And Sets baseline unit cost to 120,000 VND
  Then The system persists the SKU with active status
  And The baseline unit cost 120,000 VND is stored for COGS reference

Scenario: Block negative baseline unit cost
  Given User is authenticated as "Finance Manager"
  When User attempts to set baseline unit cost to -10,000 VND
  Then The system blocks persistence
  And Displays validation error "Baseline cost price cannot be negative"

Scenario: Sales role has restricted cost access
  Given User is authenticated as "Sales & Ops"
  When Staff opens the Create Order modal to select SKUs
  Then Staff can view SKU code, color, size, and retail selling price
  And The baseline unit cost field is strictly hidden
```

---

### MODULE M01: MULTI-CHANNEL ORDER MANAGEMENT

#### US-ORD-01: Ingest Multi-Channel Multi-Line Orders
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Ingest orders from TikTok Shop, Shopee, or in-store POS containing external order IDs, customer details, shop vouchers, and multiple line items,
  - **So that:** Commercial order records are captured completely to initiate revenue and profit tracking.

- **INVEST Assessment:**
  - **I (Independent):** Can be tested with mock fee engines without waiting for settlement flows.
  - **N (Negotiable):** Customer fields can be optional depending on channel constraints.
  - **V (Valuable):** Core data ingestion engine powering the entire revenue and profit pipeline.
  - **E (Estimable):** Master-Detail pattern (Order & OrderItems); Estimated Effort: 5 Story Points.
  - **S (Small):** Completable within 2 engineering days.
  - **T (Testable):** Verifiable across single-item, multi-item, and voucher boundary cases.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Ingest multi-line order successfully
  Given Catalog contains SKU "AT-COT-BLK-L" (180,000 VND) and "AT-COT-WHT-M" (150,000 VND)
  When Staff selects channel "TIKTOK", inputs external ID "TT-889922"
  And Adds Line 1: SKU "AT-COT-BLK-L", Quantity 2
  And Adds Line 2: SKU "AT-COT-WHT-M", Quantity 1
  And Inputs shop discount voucher 30,000 VND
  And Submits "Create Order"
  Then Total items subtotal is 510,000 VND (360,000 + 150,000)
  And Customer Gross Revenue is 480,000 VND (510,000 - 30,000)
  And Order lifecycle status is initialized as "PENDING"

Scenario: Block order creation with empty line items
  When Staff creates an order on channel "SHOPEE" with 0 line items
  And Submits "Create Order"
  Then The system blocks persistence
  And Displays error "Order must contain at least one line item"

Scenario: Reject voucher exceeding items total
  Given An order with total items subtotal of 200,000 VND
  When Staff inputs discount voucher of 250,000 VND
  Then The system displays warning "Discount voucher cannot exceed total items subtotal"
```

---

#### US-ORD-02: Track Order Lifecycle & Official Revenue & Profit Recognition
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Transition order statuses through their delivery stages (`PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`),
  - **So that:** Revenue, COGS, and Contribution Profit are officially recognized into financial reporting periods only upon confirmed `DELIVERED` status.

- **INVEST Assessment:**
  - **I (Independent):** Governed by a dedicated state machine.
  - **N (Negotiable):** Transition intermediate steps can store carrier tracking numbers.
  - **V (Valuable):** Prevents premature or phantom revenue/profit recognition before physical customer receipt.
  - **E (Estimable):** Conditional state transitions; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** State transition rules easily verified via unit/integration tests.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Transition to DELIVERED and recognize revenue and profit
  Given Order "ORD-001" is in "SHIPPED" status with Gross Revenue 480,000 VND and order COGS 250,000 VND
  When Staff marks status as "DELIVERED"
  Then Order status updates to "DELIVERED"
  And Timestamp "delivered_at" is recorded in UTC
  And 480,000 VND is officially recognized as Gross Revenue
  And 250,000 VND is officially recognized as COGS
  And Platform fees and Contribution Profit are frozen for period reporting

Scenario: Prevent illegal status skipping for online orders
  Given Online order "ORD-002" is in "PENDING" status
  When Staff attempts to directly jump to "DELIVERED" without entering "SHIPPED"
  Then The system rejects the transition
  And Displays error "Order must be in SHIPPED status before marking DELIVERED"

Scenario: Complete in-store POS order immediately upon payment
  Given Staff creates an in-store order on channel "POS"
  When Customer completes payment at the cash register
  Then Order status is immediately initialized as "DELIVERED"
  And Revenue, fees, and Contribution Profit are officially recognized on the spot
```

---

#### US-ORD-03: Cancel Orders & Exclude from Revenue & Profit
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Cancel failed or rejected orders (`CANCELLED`) with a mandatory cancellation reason,
  - **So that:** Cancelled orders contribute 0 to recognized Gross Revenue, COGS, Platform Fees, and Contribution Profit.

- **INVEST Assessment:**
  - **I (Independent):** Dedicated cancellation endpoint and filter exclusion.
  - **N (Negotiable):** Cancellation reasons list can be expanded dynamically.
  - **V (Valuable):** Ensures financial reporting integrity without distortion from aborted orders.
  - **E (Estimable):** Status update with business rule check; Estimated Effort: 2 Story Points.
  - **S (Small):** Completable within 0.5 engineering day.
  - **T (Testable):** Verifiable that cancelled orders contribute 0 to all financial totals.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Cancel pending order successfully
  Given Order "ORD-003" is in "PENDING" status with items valued at 300,000 VND
  When Staff chooses "Cancel Order" with reason "Customer changed mind"
  Then Order status updates to "CANCELLED"
  And The reason is recorded in order status history
  And The order contributes 0 VND to Gross Revenue, 0 VND to COGS, and 0 VND to Contribution Profit

Scenario: Prevent cancellation of delivered order
  Given Order "ORD-004" is in "DELIVERED" status
  When User attempts to cancel the order
  Then The action is blocked
  And The system displays error "Cannot cancel an order that has already been delivered"
```

---

### MODULE M02: SALES FEES & SETTLEMENT

#### US-FEE-01: Preview fees by sales channel and payment method, including in-store purchases
- **User Story:**
  - **As a:** Finance Manager / Shop Owner,
  - **I want to:** Automatically apply channel-specific fee deduction formulas (Strategy Pattern) upon order creation,
  - **So that:** Commission, payment processing, service fees, and projected net wallet settlement are accurately estimated.

- **INVEST Assessment:**
  - **I (Independent):** Decoupled Strategy Pattern service, easily testable in isolation with mock order payloads.
  - **N (Negotiable):** Commission, payment processing, and POS terminal rates can be stored in configuration tables for dynamic tuning.
  - **V (Valuable):** Resolves the primary cashflow blind spot for shop owners: clearly distinguishing nominal gross payment from true take-home cash after third-party deductions.
  - **E (Estimable):** Explicit mathematical formulas and clear multi-channel matrix; Estimated Effort: 4 Story Points.
  - **S (Small):** Completable within 1.5 engineering days.
  - **T (Testable):** Exhaustive unit test coverage for Shopee, TikTok, in-store cash, and in-store card/QR payments.

- **Multi-Channel Fee Matrix:**

| Sales Channel | Payment Method | Marketplace Commission | Payment Processing Fee | Service / Fixed Fee | Projected Net Settlement Formula |
|---|---|:---:|:---:|:---:|---|
| **1. In-Store POS (Direct)** | **Cash** | **0%** | **0%** | **0 VND** | `Net = Gross Revenue` (Shop keeps 100%) |
| | **Bank Card Swipe / QR** | **0%** | **1.0%** on Gross Revenue | **0 VND** | `Net = Gross Revenue - 1.0%` |
| **2. TikTok Shop (Online)** | TikTok Wallet Payout | **4.0%** on items subtotal | **3.0%** on Gross Revenue | **3,000 VND** fixed/order | `Net = Gross Revenue - TikTok Fees` |
| **3. Shopee (Online)** | Shopee Balance Payout | **4.5%** on items subtotal | **4.0%** on Gross Revenue | **2.0%** Service Fee (capped) | `Net = Gross Revenue - Shopee Fees` |

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Estimate marketplace fees for Shopee order with service fee
  Given An order on "SHOPEE" with Items Subtotal 400,000 VND and Shop Voucher 0 VND
  When Platform Fee Engine calculates fees
  Then Commission fee is 18,000 VND (400,000 x 4.5%)
  And Payment processing fee is 16,000 VND (400,000 x 4.0%)
  And Service fee is 8,000 VND (400,000 x 2.0%)
  And Total platform deductions equal 42,000 VND (18,000 + 16,000 + 8,000)
  And Projected settlement equals 358,000 VND (400,000 - 42,000)

Scenario: Estimate marketplace fees for TikTok Shop order
  Given An order on "TIKTOK" with Items Subtotal 500,000 VND and Shop Voucher 0 VND
  When Platform Fee Engine calculates fees
  Then Commission fee is 20,000 VND (500,000 x 4.0%)
  And Payment processing fee is 15,000 VND (500,000 x 3.0%)
  And Fixed fee is 3,000 VND
  And Total platform deductions equal 38,000 VND
  And Projected settlement equals 462,000 VND (500,000 - 38,000)

Scenario: Estimate fees for in-store POS cash purchase
  Given An in-store order on "POS" with Gross Revenue 500,000 VND and payment method "CASH"
  When Platform Fee Engine calculates fees
  Then Commission fee is 0 VND
  And Payment processing fee is 0 VND
  And Total fee deduction is 0 VND
  And Projected settlement is 500,000 VND (100% of revenue)

Scenario: Estimate fees for in-store POS card swipe purchase
  Given An in-store order on "POS" with Gross Revenue 500,000 VND and payment method "POS_CARD_QR"
  When Platform Fee Engine calculates fees
  Then Commission fee is 0 VND
  And Bank POS processing fee is 5,000 VND (500,000 x 1.0%)
  And Total fee deduction is 5,000 VND
  And Projected settlement is 495,000 VND (500,000 - 5,000)
```

---

#### US-SET-01: Transparent Platform Fee Breakdown & Projected Net
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Inspect a granular fee deduction ledger for each order,
  - **So that:** Every subtracted component (commission, payment, service, fixed fees) is auditable prior to wallet payout.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Display fee breakdown ledger columns
  Given Finance Manager navigates to "Screen 2: Fees & Settlement"
  When The order list is loaded
  Then Each row displays: Order ID, Channel, Gross Amount, Commission, Payment Fee, Service/Fixed Fees, Total Fee, Projected Net, and Audit Status
  And "Projected Net" strictly equals "Gross Amount" minus "Total Fee"
```

---

#### US-SET-02: Settlement Audit & Actual Wallet Deposit Update
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Input the actual settlement amount received from the marketplace payout statement and capture any variance,
  - **So that:** The system flags discrepancies and completes wallet reconciliation.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Match settlement with zero variance
  Given Order "ORD-001" has Projected Settlement 358,000 VND
  When Finance Manager enters Actual Received 358,000 VND
  And Confirms settlement
  Then Settlement status changes to "RECONCILED"
  And Variance displays 0 VND

Scenario: Flag settlement discrepancy when marketplace deductions exceed estimates
  Given Order "ORD-002" has Projected Settlement 500,000 VND
  When Marketplace settles 480,000 VND
  And Finance enters 480,000 VND with note "Carrier weight adjustment fee"
  Then Settlement status changes to "DISCREPANCY"
  And Discrepancy amount highlights 20,000 VND positive variance (shortfall)
```

---

### MODULE M03: COST & PROFIT CALCULATION ENGINE

#### US-PROFIT-01: Freeze COGS Snapshot & Calculate Order Contribution Profit
- **User Story:**
  - **As a:** Shop Owner / Finance Manager,
  - **I want to:** Automatically freeze each line item's unit cost baseline upon order creation and calculate order-level Contribution Profit upon delivery,
  - **So that:** Historical profitability remains completely auditable even if master catalog costs are modified in the future.

- **INVEST Assessment:**
  - **I (Independent):** Core calculation engine executed during order placement and delivery transitions.
  - **N (Negotiable):** Presentation can display both monetary Contribution Profit and Contribution Margin %.
  - **V (Valuable):** Transforms the system into a true commercial profit intelligence tool.
  - **E (Estimable):** Snapshot replication and basic arithmetic formulas; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Verification of immutable cost freezing and arithmetic formula: `Contribution Profit = Projected Settlement - COGS`.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Freeze immutable unit cost snapshot at order creation
  Given Catalog SKU "SLS-WHT-M" has baseline unit cost 100,000 VND
  When Staff creates an order containing 2 units of "SLS-WHT-M"
  Then The order line item records "unit_cost_snapshot" = 100,000 VND
  And Total line COGS is calculated as 200,000 VND (100,000 x 2)

Scenario: Catalog cost update does not alter historical order COGS
  Given Order "ORD-010" contains SKU "SLS-WHT-M" with frozen unit cost 100,000 VND
  When Shop Owner updates catalog baseline cost of "SLS-WHT-M" to 130,000 VND
  Then Order "ORD-010" retains historical unit cost snapshot of 100,000 VND
  And Order "ORD-010" total COGS remains unchanged

Scenario: Compute Contribution Profit upon delivery
  Given An order with Gross Revenue 500,000 VND, Total Platform Fees 50,000 VND, and total COGS 300,000 VND
  When Order transitions to "DELIVERED"
  Then Projected Settlement is 450,000 VND (500,000 - 50,000)
  And Contribution Profit is recognized as 150,000 VND (450,000 - 300,000)
  And Contribution Margin is recognized as 30.0% (150,000 / 500,000 x 100)
```

---

### MODULE M04: REVENUE & PROFIT ANALYTICS

#### US-DASH-01: Monitor Core Financial KPI Cards (Revenue, Fees, COGS, Profit)
- **User Story:**
  - **As a:** Shop Owner,
  - **I want to:** View 5 core financial KPI cards: Total Gross Revenue, Total Platform Fees, Projected Settlement, Total COGS, and Total Contribution Profit,
  - **So that:** Executive leadership immediately understands true commercial take-home earnings.

- **INVEST Assessment:**
  - **I (Independent):** Aggregation service querying delivered order snapshots.
  - **N (Negotiable):** Secondary operational metrics (Delivered Orders count) can be displayed alongside.
  - **V (Valuable):** Delivers instantaneous financial visibility for strategic decisions.
  - **E (Estimable):** Standard SQL SUM aggregations; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Verified that cancelled orders are excluded and sums match line item snapshots.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Accurate headline financial KPI calculation from delivered orders
  Given 2 DELIVERED orders exist:
    | Order | Gross Revenue | Platform Fees | COGS |
    | ORD-1 | 1,000,000 VND | 100,000 VND   | 600,000 VND |
    | ORD-2 |   500,000 VND |  50,000 VND   | 300,000 VND |
  And 1 CANCELLED order exists (Gross: 300,000 VND, COGS: 180,000 VND)
  When User views Screen 3: Revenue Dashboard
  Then "Total Gross Revenue" card displays 1,500,000 VND
  And "Total Platform Fees" card displays 150,000 VND
  And "Projected Settlement" card displays 1,350,000 VND
  And "Total COGS" card displays 900,000 VND
  And "Contribution Profit" card displays 450,000 VND (1,350,000 - 900,000)
  And The CANCELLED order is completely excluded from all financial cards
```

---

#### US-DASH-02: Filter Revenue & Profit Analytics by Date Range & Channel
- **User Story:**
  - **As a:** Finance Manager / Shop Owner,
  - **I want to:** Filter all dashboard KPIs and graphs by date ranges and sales channels,
  - **So that:** Revenue, COGS, and Contribution Profit can be compared across channels and promotional campaigns.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Filter analytics for TikTok Shop over last 7 days
  Given User is on Revenue Dashboard
  When User selects time range "Last 7 Days" and channel "TIKTOK"
  Then All KPI cards and charts refresh to display only TikTok orders delivered in the last 7 days
  And Shopee and POS figures are excluded
```

---

#### US-DASH-03: Visualize Channel Share & Top SKU by Revenue and Profit
- **User Story:**
  - **As a:** Shop Owner,
  - **I want to:** View a channel revenue/profit share chart and a leaderboard of Top 5 SKUs displaying Gross Revenue, COGS, and Contribution Profit,
  - **So that:** Merchandising focus and marketing budgets are allocated to high-margin apparel items.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Top SKU leaderboard displays Revenue and Contribution Profit
  Given Delivered SKU performance:
    | SKU   | Gross Revenue | Total COGS    | Contribution Profit |
    | SKU-A | 25,000,000 VND| 15,000,000 VND| 10,000,000 VND      |
    | SKU-B | 20,000,000 VND| 10,000,000 VND| 10,000,000 VND      |
    | SKU-C | 15,000,000 VND| 12,000,000 VND|  3,000,000 VND      |
  When Shop Owner views the Top SKU Leaderboard sorted by Contribution Profit
  Then SKU-A and SKU-B appear at the top
  And Each row displays Gross Revenue, COGS, Contribution Profit, and Contribution Margin %
```

> [!NOTE]
> **Top SKU Contribution Profit Proportional Allocation Rule:**  
> For multi-item orders, order-level vouchers and platform fee deductions are allocated down to individual SKU line items proportionally based on line subtotal share:
> $$\text{lineShare} = \frac{\text{lineSubtotal}}{\text{orderSubtotal}}$$
> $$\text{allocatedVoucher} = \text{orderVoucher} \times \text{lineShare}$$
> $$\text{lineGrossRevenue} = \text{lineSubtotal} - \text{allocatedVoucher}$$
> $$\text{allocatedPlatformFees} = \text{orderTotalPlatformFees} \times \text{lineShare}$$
> $$\text{lineContributionProfit} = \text{lineGrossRevenue} - \text{allocatedPlatformFees} - \text{lineCOGS}$$
> *(Any rounding residual across lines is absorbed into the highest-value order line).*

---

#### US-DASH-04: Drilldown to Source Orders & Export CSV Audit Log
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Click into any KPI card to inspect underlying source orders and export the filtered dataset as a CSV file,
  - **So that:** Comprehensive audit trails are preserved for accounting reconciliation.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Export filtered revenue and profit report to CSV
  Given Finance Manager is viewing the filtered monthly dashboard
  When Clicks "Export CSV Report"
  Then A file named "Revenue_Profit_Report_YYYYMMDD.csv" downloads automatically
  And File contains: Order ID, Channel, Gross Revenue, Fees, COGS, Contribution Profit, and Settlement Status
  And Character encoding displays properly in Microsoft Excel
```
