# INVEST Requirements Specification

## Multi-Channel Revenue Management System
---

## 1. Introduction & INVEST Principles

This document standardizes the functional requirements of the system into **User Stories**, strictly adhering to the 6 criteria of the **INVEST** framework:

1. **Independent:** Each User Story is decoupled from others, allowing standalone scheduling, development, and release without blocking dependencies.
2. **Negotiable:** The story captures the core business intent, allowing technical implementation and UI details to be refined collaboratively between developers, testers, and stakeholders.
3. **Valuable:** Every story delivers tangible, quantifiable value to an end-user role (Sales/Ops, Finance Manager, Shop Owner).
4. **Estimable:** Scope, constraints, and business rules are sufficiently defined for engineering teams to estimate Story Points and implementation effort.
5. **Small:** Stories are sized appropriately to be completed within 1-3 engineering days (1 sprint iteration), avoiding oversized Epics.
6. **Testable:** All Acceptance Criteria are formalized using Gherkin syntax (`Given... When... Then...`), serving as direct specifications for Unit, Integration, and E2E tests.

---

## 2. System User Stories Master Table

| Module | Story ID | User Story Title | Primary Role | Screen | Priority |
|---|---|---|:---:|---|:---:|
| **M01: Multi-Channel Order Management** | `US-ORD-01` | Ingest Multi-Channel Multi-Line Orders | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| | `US-ORD-02` | Track Order Lifecycle & Official Revenue Recognition | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| | `US-ORD-03` | Cancel Orders & Exclude from Net Revenue | Sales & Ops | Screen 1: Orders Management | P1 (Must) |
| **M02: Sales Fees & Settlement** | `US-FEE-01` | Preview fees by sales channel and payment method, including in-store purchases | Finance Manager / Shop Owner | Screen 1 & Screen 2 | P1 (Must) |
| | `US-SET-01` | Transparent Platform Fee Breakdown & Projected Net | Finance Manager | Screen 2: Fees & Settlement | P1 (Must) |
| | `US-SET-02` | Settlement Audit & Actual Wallet Deposit Update | Finance Manager | Screen 2: Fees & Settlement | P2 (Should) |
| **M03: Revenue Analytics & Dashboard** | `US-DASH-01` | Monitor 3 Core Executive Financial KPI Cards | Shop Owner | Screen 3: Revenue Dashboard | P1 (Must) |
| | `US-DASH-02` | Filter Revenue Analytics by Date Range & Channel | Finance / Owner | Screen 3: Revenue Dashboard | P1 (Must) |
| | `US-DASH-03` | Visualize Channel Share & Top SKU Leaderboard | Shop Owner | Screen 3: Revenue Dashboard | P2 (Should) |
| | `US-DASH-04` | Drilldown to Source Orders & Export CSV Audit Log | Finance Manager | Screen 3: Revenue Dashboard | P2 (Should) |

---

## 3. Detailed User Stories (INVEST Standard)

---

### MODULE M01: MULTI-CHANNEL ORDER MANAGEMENT

#### US-ORD-01: Ingest Multi-Channel Multi-Line Orders
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Ingest orders from TikTok Shop, Shopee, or Facebook POS containing external order IDs, customer details, shop vouchers, and multiple line items,
  - **So that:** Order records are captured completely to initiate revenue tracking.

- **INVEST Assessment:**
  - **I (Independent):** Can be tested with mock fee engines without waiting for settlement flows.
  - **N (Negotiable):** Customer fields can be optional depending on channel constraints.
  - **V (Valuable):** Core data ingestion engine powering the entire revenue pipeline.
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
  Then Total items value is 510,000 VND (360,000 + 150,000)
  And Customer Gross Payment is 480,000 VND (510,000 - 30,000)
  And Order lifecycle status is initialized as "PENDING"

Scenario: Block order creation with empty line items
  When Staff creates an order on channel "SHOPEE" with 0 line items
  And Submits "Create Order"
  Then The system blocks persistence
  And Displays error "Order must contain at least one line item"

Scenario: Reject voucher exceeding items total
  Given An order with total items value of 200,000 VND
  When Staff inputs discount voucher of 250,000 VND
  Then The system displays warning "Discount voucher cannot exceed total items value"
```

---

#### US-ORD-02: Track Order Lifecycle & Official Revenue Recognition
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Transition order statuses through their delivery stages (`PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`),
  - **So that:** Revenue is officially recognized into financial periods only upon confirmed `DELIVERED` status.

- **INVEST Assessment:**
  - **I (Independent):** Governed by a dedicated state machine.
  - **N (Negotiable):** Transition intermediate steps can store carrier tracking numbers.
  - **V (Valuable):** Prevents premature or phantom revenue recognition before actual fulfillment.
  - **E (Estimable):** Conditional state transitions; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** State transition rules easily verified via unit/integration tests.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Transition to DELIVERED and recognize revenue
  Given Order "ORD-001" is in "SHIPPED" status with gross value 480,000 VND
  When Staff marks status as "DELIVERED"
  Then Order status updates to "DELIVERED"
  And Timestamp "delivered_at" is recorded
  And 480,000 VND is officially credited to recognized period revenue

Scenario: Prevent illegal status skipping for online orders
  Given Online order "ORD-002" is in "PENDING" status
  When Staff attempts to directly jump to "DELIVERED" without entering "SHIPPED"
  Then The system rejects the transition
  And Displays error "Order must be in SHIPPED status before marking DELIVERED"

Scenario: Complete in-store POS order immediately upon payment
  Given Staff creates an in-store order on channel "POS"
  When Customer completes payment at the cash register
  Then Order status is immediately initialized as "DELIVERED"
  And Revenue is officially recognized on the spot without requiring a "SHIPPED" transition
```

---

#### US-ORD-03: Cancel Orders & Exclude from Net Revenue
- **User Story:**
  - **As a:** Sales & Ops Staff member,
  - **I want to:** Cancel failed or rejected orders (`CANCELLED`) with a specified cancellation reason,
  - **So that:** Cancelled orders are automatically excluded from all realized revenue metrics.

- **INVEST Assessment:**
  - **I (Independent):** Dedicated cancellation endpoint and filter exclusion.
  - **N (Negotiable):** Cancellation reasons list can be expanded dynamically.
  - **V (Valuable):** Ensures true revenue reporting without inflated figures from boom orders.
  - **E (Estimable):** Status update with business rule check; Estimated Effort: 2 Story Points.
  - **S (Small):** Completable within 0.5 engineering day.
  - **T (Testable):** Verifiable that cancelled orders do not impact gross revenue sums.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Cancel pending order successfully
  Given Order "ORD-003" is in "PENDING" status
  When Staff chooses "Cancel Order" with reason "Customer changed mind"
  Then Order status updates to "CANCELLED"
  And The reason is recorded in order history
  And The order value is excluded from all revenue totals

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
  - **So that:** Commission, payment processing, service/Freeship fees, and projected net wallet settlement are accurately estimated.

- **INVEST Assessment:**
  - **I (Independent):** Decoupled Strategy Pattern service, easily testable in isolation with mock order payloads.
  - **N (Negotiable):** Commission, payment processing, and POS terminal rates can be stored in configuration tables for dynamic tuning.
  - **V (Valuable):** Resolves the primary cashflow blind spot for shop owners: clearly distinguishing nominal gross payment from true take-home cash after third-party deductions.
  - **E (Estimable):** Explicit mathematical formulas and clear multi-channel matrix; Estimated Effort: 4 Story Points.
  - **S (Small):** Completable within 1.5 engineering days.
  - **T (Testable):** Exhaustive unit test coverage for Shopee, TikTok, in-store cash, and in-store card/QR payments.

- **Business Reality & Fee Bearer Specifications (Who Bears The Fees?):**
  - **Marketplace Payment Fee (3.0% to 4.0%):** Paid **100% BY THE SHOP / SELLER**. Regardless of whether the customer pays via COD, bank transfer, or ShopeePay, the marketplace automatically deducts this processing cut before depositing funds into the shop wallet.
  - **Freeship Xtra / Program Fee (2.0% to 6.0%):** Paid **BY THE SHOP**. To display free shipping badges to shoppers, the shop subscribes to marketplace service packages; the platform deducts this fee on every delivered order.
  - **Platform Commission & Fixed Fees:** Paid **100% BY THE SHOP** as virtual storefront rental.
  - **Direct In-Store POS:** No marketplace intermediary $\rightarrow$ **Commission = 0 VND**. If the customer pays with **Cash**, the shop keeps 100% (**Fees = 0 VND**). If the customer pays with **Bank Card / QR Transfer**, the shop incurs a standard bank POS processing fee (**1.0%**).

- **Multi-Channel Fee Matrix:**

| Sales Channel | Payment Method | Marketplace Commission | Payment Processing Fee | Service / Freeship Fee | Projected Net Settlement Formula |
|---|---|:---:|:---:|:---:|---|
| **1. In-Store POS (Direct)** | **Cash** | **0 VND (0%)** | **0 VND (0%)** | **0 VND** | $\text{Net} = \text{Gross Payment}$ (Shop keeps 100%) |
| | **Bank Card Swipe / QR** | **0 VND (0%)** | **1.0%** on total payment | **0 VND** | $\text{Net} = \text{Gross Payment} - 1.0\%$ |
| **2. TikTok Shop (Online)** | TikTok Wallet Payout | **4.0%** items subtotal | **3.0%** total payment | **2,000 VND** fixed/order | $\text{Net} = \text{Gross} - \text{TikTok Fees}$ |
| **3. Shopee (Online)** | Shopee Balance Payout | **4.5%** items subtotal | **4.0%** total payment | **2.0%** Freeship Xtra (max 20k) | $\text{Net} = \text{Gross} - \text{Shopee Fees}$ |

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Estimate marketplace fees for Shopee order with Freeship Xtra
  Given An order on "SHOPEE" with Items Subtotal 400,000 VND and Shop Voucher 0 VND
  When Platform Fee Engine calculates fees
  Then Commission fee is 18,000 VND (400,000 x 4.5%)
  And Payment processing fee is 16,000 VND (400,000 x 4.0%)
  And Freeship Xtra fee is 8,000 VND (400,000 x 2.0%)
  And Total platform deductions equal 42,000 VND (18,000 + 16,000 + 8,000)
  And Projected net wallet settlement equals 358,000 VND (400,000 - 42,000)

Scenario: Estimate marketplace fees for TikTok Shop order
  Given An order on "TIKTOK" with Items Subtotal 500,000 VND and Shop Voucher 0 VND
  When Platform Fee Engine calculates fees
  Then Commission fee is 20,000 VND (500,000 x 4.0%)
  And Payment processing fee is 15,000 VND (500,000 x 3.0%)
  And Fixed service fee is 2,000 VND
  And Total platform deductions equal 37,000 VND
  And Projected net wallet settlement equals 463,000 VND (500,000 - 37,000)

Scenario: Estimate fees for in-store POS cash purchase
  Given An in-store order on "POS" with Total Payment 500,000 VND and payment method "CASH"
  When Platform Fee Engine calculates fees
  Then Commission fee is 0 VND
  And Payment processing fee is 0 VND
  And Total fee deduction is 0 VND
  And Net cash received in store drawer is 500,000 VND (100% of revenue)

Scenario: Estimate fees for in-store POS card swipe purchase
  Given An in-store order on "POS" with Total Payment 500,000 VND and payment method "CARD"
  When Platform Fee Engine calculates fees
  Then Commission fee is 0 VND
  And Bank POS processing fee is 5,000 VND (500,000 x 1.0%)
  And Total fee deduction is 5,000 VND
  And Net funds deposited to shop bank account is 495,000 VND (500,000 - 5,000)
```

---

#### US-SET-01: Transparent Platform Fee Breakdown & Projected Net
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Inspect a granular fee deduction ledger for each order,
  - **So that:** Every subtracted component (commission, payment, service fees) is auditable prior to wallet payout.

- **INVEST Assessment:**
  - **I (Independent):** Read-only view on Screen 2 consuming computed fee records.
  - **N (Negotiable):** Columns can be dynamically toggled or customized.
  - **V (Valuable):** Equips finance with immediate visibility into excessive fee charges.
  - **E (Estimable):** Structured data table and query; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Mathematical integrity: $\text{Projected Net} = \text{Gross Payment} - \sum \text{Fees}$.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Display fee breakdown ledger columns
  Given Finance Manager navigates to "Screen 2: Fees & Settlement"
  When The order list is loaded
  Then Each row displays: Order ID, Channel, Gross Amount, Commission, Payment Fee, Other Fees, Total Fee, Projected Net, and Audit Status
  And "Projected Net" strictly equals "Gross Amount" minus "Total Fee"
```

---

#### US-SET-02: Settlement Audit & Actual Wallet Deposit Update
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Input the actual settlement amount received from the marketplace payout statement and capture any variance,
  - **So that:** The system flags discrepancies and completes wallet reconciliation.

- **INVEST Assessment:**
  - **I (Independent):** Isolated update on settlement fields without rewriting original order values.
  - **N (Negotiable):** Can be performed per order or via batch reconciliation.
  - **V (Valuable):** Ensures 100% fidelity between internal books and bank/wallet balances.
  - **E (Estimable):** Clear input, formula, and status mutation; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Variance calculation: $\text{Variance} = \text{Projected} - \text{Actual}$.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Match settlement with zero variance
  Given Order "ORD-001" has Projected Net 358,000 VND
  When Finance Manager enters Actual Received 358,000 VND
  And Confirms settlement
  Then Settlement status changes to "RECONCILED"
  And Variance displays 0 VND

Scenario: Flag settlement discrepancy when marketplace deductions exceed estimates
  Given Order "ORD-002" has Projected Net 500,000 VND
  When Marketplace settles 480,000 VND (due to 20,000 VND shipping surcharge)
  And Finance enters 480,000 VND with note "Carrier overweight fee"
  Then Settlement status changes to "DISCREPANCY"
  And Discrepancy amount highlights 20,000 VND variance
```

---

### MODULE M03: REVENUE ANALYTICS & DASHBOARD

#### US-DASH-01: Monitor 3 Core Executive Financial KPI Cards
- **User Story:**
  - **As a:** Shop Owner / Finance Manager,
  - **I want to:** View 3 core headline KPI summary cards: Total Gross Sales, Total Deducted Platform Fees, and Total Net Settlement,
  - **So that:** Executive leadership immediately understands true revenue health.

- **INVEST Assessment:**
  - **I (Independent):** Aggregation service querying delivered orders.
  - **N (Negotiable):** Can display fee percentage ratios alongside gross numbers.
  - **V (Valuable):** Delivers at-a-glance financial visibility for key business decisions.
  - **E (Estimable):** Standard SQL SUM aggregations; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Verified that cancelled orders are excluded and sums match line item totals.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Accurate headline KPI calculation from delivered orders
  Given 2 DELIVERED orders exist (Gross: 1,000,000 & 500,000; Fees: 100,000 & 50,000)
  And 1 CANCELLED order exists (Gross: 300,000)
  When User views Screen 3: Revenue Dashboard
  Then "Total Gross Sales" card displays 1,500,000 VND
  And "Total Platform Fees" card displays 150,000 VND
  And "Total Net Settlement" card displays 1,350,000 VND
  And The CANCELLED order is completely excluded from all cards
```

---

#### US-DASH-02: Filter Revenue Analytics by Date Range & Channel
- **User Story:**
  - **As a:** Finance Manager / Shop Owner,
  - **I want to:** Filter all dashboard KPIs and graphs by preset/custom date ranges and selling channels,
  - **So that:** Performance can be analyzed by campaign periods and channel channels.

- **INVEST Assessment:**
  - **I (Independent):** Query parameters passed to existing aggregation APIs.
  - **N (Negotiable):** Presets (Today, Last 7 Days, This Month, Custom) can be adjusted.
  - **V (Valuable):** Enables granular performance comparison across channels.
  - **E (Estimable):** Filtered queries; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Verifiable across boundary dates and multi-channel selections.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Filter analytics for TikTok Shop over last 7 days
  Given User is on Revenue Dashboard
  When User selects time range "Last 7 Days" and channel "TIKTOK"
  Then All KPI cards and charts refresh to display only TikTok orders delivered in the last 7 days
  And Shopee and POS figures are excluded
```

---

#### US-DASH-03: Visualize Channel Share & Top SKU Leaderboard
- **User Story:**
  - **As a:** Shop Owner,
  - **I want to:** See a channel revenue contribution pie chart and a leaderboard of Top 5 revenue-generating SKUs,
  - **So that:** Merchandising focus and inventory replenishment are aligned with top performers.

- **INVEST Assessment:**
  - **I (Independent):** Isolated UI chart components consuming grouped API endpoints.
  - **N (Negotiable):** Top N items toggleable (Top 5 vs Top 10).
  - **V (Valuable):** Visual insights drive strategic inventory replenishment.
  - **E (Estimable):** Charting library integration + Group By queries; Estimated Effort: 3 Story Points.
  - **S (Small):** Completable within 1 engineering day.
  - **T (Testable):** Verifiable descending sort order of revenue per SKU.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Top SKU leaderboard displays in descending order
  Given Delivered SKU revenues: SKU-A (15M VND), SKU-B (25M VND), SKU-C (10M VND)
  When Shop Owner views the Top SKU Leaderboard
  Then SKU-B appears at Rank 1
  And SKU-A appears at Rank 2
  And SKU-C appears at Rank 3
```

---

#### US-DASH-04: Drilldown to Source Orders & Export CSV Audit Log
- **User Story:**
  - **As a:** Finance Manager,
  - **I want to:** Click into any KPI card to inspect underlying source orders and export the filtered dataset as a CSV file,
  - **So that:** Financial audit trails are preserved for accounting reconciliation.

- **INVEST Assessment:**
  - **I (Independent):** Stateless CSV export stream relying on active filter parameters.
  - **N (Negotiable):** Encoded with UTF-8 BOM for seamless Microsoft Excel compatibility in Vietnamese.
  - **V (Valuable):** Fulfills internal accounting and statutory audit trail requirements.
  - **E (Estimable):** Streaming CSV response; Estimated Effort: 2 Story Points.
  - **S (Small):** Completable within 0.5 engineering day.
  - **T (Testable):** Downloaded file content verified to match dashboard figures.

- **Acceptance Criteria (Gherkin):**

```gherkin
Scenario: Export filtered revenue report to CSV
  Given Finance Manager is viewing the filtered monthly dashboard
  When Clicks "Export CSV Report"
  Then A file named "Revenue_Report_YYYYMMDD.csv" downloads automatically
  And File contains all underlying orders matching the active filters
  And Character encoding displays properly in Microsoft Excel
```
