# Business Architecture & Use Case Specification: Fashion Revenue & Profit Management System

> **System:** Fashion Revenue & Profit Management System  
> **Objective:** Streamline multi-channel order revenue streams (TikTok Shop, Shopee, POS), automate marketplace fee deductions via Strategy Pattern, freeze immutable COGS baselines, calculate Contribution Profit, and reconcile net wallet payouts.

---

## 1. Use Case Diagram (UML Standard)

The diagram illustrates the interaction between 2 Human Actors (Sales & Ops Staff, Finance Manager & Shop Owner) and 1 Automated Engine (Platform Fee Engine), grouped across 4 primary UI workspaces:

```mermaid
graph LR
    subgraph HumanActors [Human Actors]
        SO[Sales & Ops Staff]
        FM[Finance Manager / Shop Owner]
    end

    subgraph AutoEngine [Automated Engines]
        PFE[Platform Fee Engine]
    end

    subgraph Screen1 [Screen 1: Orders Management]
        UC01[UC01: Ingest Multi-Channel Orders]
        UC03[UC03: Transition Status Shipped to Delivered]
        UC04[UC04: Cancel Order & Reverse Revenue/Profit]
    end

    subgraph Screen2 [Screen 2: Fees & Settlement]
        UC05[UC05: View Fee Breakdown & Estimated Settlement]
        UC06[UC06: Reconcile Payout Statement on Variance]
        UC07[UC07: Track Settlement Discrepancy Audits]
    end

    subgraph Screen3 [Screen 3: Revenue & Profit Dashboard]
        UC08[UC08: View 5 Core Financial KPI Cards]
        UC09[UC09: Filter Analytics by Date & Channel]
        UC10[UC10: View Channel Breakdown & Top SKUs by Profit]
        UC11[UC11: Drilldown Source Orders & Export CSV]
        UC13[UC13: Analyze Contribution Profit & Margin]
    end

    subgraph Screen4 [Screen 4: Product Catalog & Cost Management]
        UC12[UC12: Maintain SKU Catalog Pricing & Cost Baseline]
    end

    SO --> UC01
    SO --> UC03
    SO --> UC04

    FM --> UC05
    FM --> UC06
    FM --> UC07
    FM --> UC08
    FM --> UC09
    FM --> UC10
    FM --> UC11
    FM --> UC12
    FM --> UC13

    UC01 -.->|<<include>>| UC02[UC02: Estimate Platform Fees]
    PFE --> UC02
    UC01 -.->|<<include>>| UC12
    UC03 -.->|<<include>>| UC13
```

> **Canonical Financial Equations:**
> - $\text{Gross Revenue} = \text{Subtotal} - \text{Shop Voucher}$
> - $\text{COGS} = \sum (\text{Quantity} \times \text{Unit Cost Snapshot})$
> - $\text{Projected Settlement (Net Realized Revenue)} = \text{Gross Revenue} - \text{Total Platform Fees}$
> - $\text{Contribution Profit} = \text{Projected Settlement} - \text{COGS} = \text{Gross Revenue} - \text{Platform Fees} - \text{COGS}$
> - $\text{Contribution Margin \%} = \frac{\text{Contribution Profit}}{\text{Gross Revenue}} \times 100 \quad (\text{when } \text{Gross Revenue} > 0)$

---

## 2. 3-Way Traceability Matrix: Screen — Use Case — Role

| UI Screen (Workspace) | Use Case / Feature Name | User Role | Business Responsibility & Controls |
|---|---|:---:|---|
| **Screen 1: Orders Management** *(Orders Tab)* | **UC01: Ingest Multi-Channel Orders (TikTok/Shopee/POS)** | `Sales / Ops`<br>`Shop Owner` | Records orders from TikTok, Shopee, and POS. Snapshots SKU selling prices and unit cost baselines. |
| | **UC02: Automated Platform Fee Estimation (Strategy)** | `Platform Fee Engine`<br>*(Automated Engine)* | Automatically applies channel-specific fee deduction formulas (commission, payment, service fees). |
| | **UC03: Update Order Status (Shipped $\rightarrow$ Delivered)** | `Sales / Ops` | Upon `DELIVERED`, the system **officially recognizes revenue, COGS, and Contribution Profit**. |
| | **UC04: Cancel Order & Exclude from Revenue/Profit** | `Sales / Ops` | Handles customer cancellations before fulfillment, strictly excluding them from revenue and profit metrics. |
| **Screen 2: Fees & Settlement** *(Settlement Tab)* | **UC05: View Fee Breakdown & Estimated Settlement** | `Finance Manager`<br>`Shop Owner` | Inspects fee deductions: Gross Revenue $\rightarrow$ Commission $\rightarrow$ Payment $\rightarrow$ Service $\rightarrow$ Net Payout. |
| | **UC06: Reconcile Payout Statement (On Variance)** | `Finance Manager` | Audits payout statements and enters `Actual Settlement Amount` to reconcile cash receipts. |
| | **UC07: Track Settlement Discrepancy Audits** | `Finance Manager`<br>`Shop Owner` | Investigates variances between projected payout vs. actual wallet deposit and records explanation notes. |
| **Screen 3: Revenue Dashboard** *(Dashboard Tab)* | **UC08: View 5 Core Financial KPI Cards** | `Shop Owner`<br>`Finance Manager` | Monitors 5 headline KPIs: **Gross Revenue**, **Platform Fees**, **Net Realized Revenue**, **COGS**, **Contribution Profit**. |
| | **UC09: Filter Revenue & Profit by Date & Channel** | `Shop Owner`<br>`Finance Manager` | Dynamically filters financial performance by timeframe (Today, 7 Days, Month) and sales channel. |
| | **UC10: View Channel Breakdown & Top SKUs by Profit** | `Shop Owner`<br>`Finance Manager` | Visualizes channel contribution shares and ranks top-performing SKUs by revenue and Contribution Profit. |
| | **UC11: Drilldown Source Orders & Export CSV** | `Finance Manager`<br>`Shop Owner` | Drills through KPI totals to inspect granular source orders and exports reconciliation CSV files. |
| | **UC13: Analyze Contribution Profit & Margin** | `Shop Owner`<br>`Finance Manager` | Assesses channel profitability and margin percentage after marketplace fees and merchandise costs. |
| **Screen 4: Product Catalog & Cost** *(Catalog Tab)* | **UC12: Maintain SKU Catalog Pricing & Cost Baseline** | `Shop Owner`<br>`Finance Manager` | Manages apparel products, SKU variants, retail selling prices, and baseline unit costs for COGS calculation. |

---

## 3. Functional Scope Matrix

| Subsystem Module | In-Scope Features (MVP Release) | Deliberately Out-of-Scope (Excluded) |
|---|---|---|
| **1. Catalog & Pricing** | • Master product apparel model management.<br>• SKU variants (color, size, retail price).<br>• Baseline unit cost configuration (`cost_price`).<br>• Role-sensitive cost hiding (Sales cannot view costs). | • Raw material Bill of Materials (BOM).<br>• Automated production costing.<br>• Multi-currency catalog pricing. |
| **2. Multi-Channel Orders** | • Order ingestion from TikTok Shop, Shopee, POS.<br>• Multi-item multi-line order support.<br>• Immutable snapshots: SKU, product title, unit price, unit cost.<br>• Lifecycle: `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED` / `CANCELLED`.<br>• Revenue, COGS, and profit recognition upon `DELIVERED`. | • Real-time live open API webhook connectors.<br>• Automated warehouse wave routing and dispatch.<br>• Return & refund operational workflows (`RETURNED`). |
| **3. Platform Fees & Audit** | • Automated Strategy Pattern fee deduction.<br>• Channel matrix (TikTok, Shopee, POS Cash/Card).<br>• Actual wallet payout manual entry and variance reporting.<br>• Mandatory discrepancy explanation notes. | • Automated bulk Excel bank statement file parsing.<br>• Direct commercial banking API integrations. |
| **4. Revenue & Profit Analytics** | • 5 core executive KPI cards (Gross, Fees, Net, COGS, Contribution Profit).<br>• Channel revenue and profit share breakdown charts.<br>• Top SKU rankings by Gross Revenue and Contribution Profit.<br>• Source order drillthrough & CSV export. | • **EXCLUDED:** Automated inventory valuation methods (FIFO/LIFO/Moving Weighted Average).<br>• **EXCLUDED:** Company-wide net margin accounting (Rent, Payroll, Marketing OPEX, Corporate Tax, Depreciation). |

---

## 4. Core Use Case Scenarios & RBAC Matrix

### 4.1. Scenario 1: Multi-Channel Order Ingestion & Cost Freezing (UC-ORDER-01)
* **Primary Actor:** `Sales & Ops Staff`
* **Workspace:** Screen 1: Orders Management
* **Main Flow:**
  1. Operator selects Channel (*TikTok Shop, Shopee, or POS*) and enters external Order ID.
  2. Adds order line items (SKU, quantity, unit selling price) and enters shop voucher.
  3. System automatically freezes `unit_cost_snapshot` from master catalog baseline:
     $$\text{Line COGS} = \text{Quantity} \times \text{unit\_cost\_snapshot}$$
  4. System triggers `Platform Fee Engine` via Strategy Pattern:
     $$\text{Estimated Fees} = \text{Commission} + \text{Payment Fee} + \text{Service Fee} + \text{Fixed Fee}$$
     $$\text{Projected Settlement} = \text{Gross Revenue} - \text{Estimated Fees}$$
  5. Order is saved with `PENDING` status.

### 4.2. Scenario 2: Order Lifecycle & Revenue/Profit Recognition (UC-REV-02)
* **Primary Actor:** `Sales & Ops Staff`
* **Workspace:** Screen 1: Orders Management
* **Rules:**
  1. **Dispatch (`SHIPPED`):** Order is marked in-transit; financial amounts remain provisional.
  2. **Delivery (`DELIVERED`):** System **officially credits Gross Revenue, COGS, and Contribution Profit** to the period financial dashboard.
  3. **Cancellation (`CANCELLED`):** System requires a cancellation reason; order contributes 0 VND to Gross Revenue, COGS, and Contribution Profit.

### 4.3. Scenario 3: Platform Wallet Reconciliation & CSV Export (UC-AUDIT-03)
* **Primary Actor:** `Finance Manager` / `Shop Owner`
* **Workspace:** Screen 2 (Settlement) & Screen 3 (Dashboard)
* **Main Flow:**
  1. Accountant reviews delivered orders, inputs actual deposit amount (`Actual Settlement Amount`) from platform wallet payout statement.
  2. System calculates variance: $\text{Variance} = \text{Projected Settlement} - \text{Actual Deposit}$.
  3. If variance $\neq 0$, accountant logs mandatory explanation notes.
  4. Navigates to Dashboard, filters reporting timeframe, and clicks **Export CSV** for accounting audit trails.

---

### 4.4. Role-Based Access Control Matrix (RBAC)

| Business Operation | UI Screen Workspace | Sales & Ops Staff | Finance Manager | Shop Owner |
|---|---|:---:|:---:|:---:|
| **Create and edit orders** | Screen 1 (Orders) | Full Access | Read-Only | Full Access |
| **Update status (Shipped/Delivered/Cancel)** | Screen 1 (Orders) | Full Access | Read-Only | Full Access |
| **View detailed fee deduction breakdown** | Screen 2 (Settlement) | No Access | Full Access | Full Access |
| **Reconcile and confirm actual payout** | Screen 2 (Settlement) | No Access | Full Access | Full Access |
| **View 5 Financial KPI cards & Charts** | Screen 3 (Dashboard) | No Access | Full Access | Full Access |
| **Filter revenue & Export CSV reports** | Screen 3 (Dashboard) | No Access | Full Access | Full Access |
| **Select SKUs for order creation** | Modal (Order Ingestion) | Full Access | Full Access | Full Access |
| **View baseline unit costs (`cost_price`)** | Screen 4 (Catalog) | **No Access (Hidden)** | Full Access | Full Access |
| **Manage products, SKUs, and baseline costs**| Screen 4 (Catalog) | No Access | Full Access | Full Access |
