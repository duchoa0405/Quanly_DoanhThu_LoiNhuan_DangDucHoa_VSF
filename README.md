# FASHION-WEB: Multi-Channel Revenue & Settlement Management Platform

## Cashflow Control & Automated Marketplace Fee Deduction Engine (TikTok Shop, Shopee, In-Store POS)

* **Project Name:** FASHION-WEB (Multi-Channel Revenue & Settlement Management Platform)
* **Author / Intern:** Dang Duc Hoa (VSF Internship Program)
* **Core Objective:** Eradicate the persistent "Paper Profit, Negative Cash Flow" paradox in multi-channel fashion retail through automated marketplace fee unbundling, bank statement wallet reconciliation, and prevention of phantom revenue recognition.

---

## 1. System Mindmap Overview

The platform architecture centers around 3 core operational pillars: Multi-channel order stream orchestration, Automated platform fee unbundling engine (Strategy Pattern), and Bank statement settlement reconciliation & discrepancy auditing.

![Multi-Channel Revenue System Mindmap](docs/screenshots/mindmap_multi_channel_revenue.png)

---

## 2. Phase 1: Requirements & Use Case Analysis

* **Detailed Specifications:**
  * 10 User Stories under INVEST & BDD Gherkin Standards: [docs/requirements_invest_en.md](docs/requirements_invest_en.md) (Vietnamese: [docs/requirements_invest.md](docs/requirements_invest.md))
  * Full Use Case Specifications: [docs/usecase.md](docs/usecase.md) (Vietnamese: [docs/usecase_vi.md](docs/usecase_vi.md))

### 2.1. System Use Case Diagram

![System Use Case Diagram - FASHION-WEB](docs/screenshots/usecase_multi_channel_revenue.png)

### 2.2. Actors & Role-Based Access Control (RBAC)
* **Sales & Operations Staff (Sales/Ops):** Ingests and monitors orders across sales channels, performs manual order entry (livestreams, hotlines, physical counter sales), updates fulfillment milestones (Packaging, Shipped, Delivered), and executes cancellation workflows.
* **Finance Manager:** Audits platform deduction policies, imports bank/wallet transaction statements, executes automated settlement matching, and files dispute justification notes for discrepancies (Variance).
* **Shop Owner / Executive Board:** Tracks executive revenue health KPIs, analyzes take-home unit margins, approves settlement variances, and configures channel commission parameters.
* **Marketplace API Engine (Automated Subsystem):** Ingests orders via webhooks, executes dynamic fee calculation per itemized order row, and computes reconciliation variances.

### 2.3. Automated Multi-Channel Fee Schedule Matrix
| Sales Channel | Commission Fee | Payment Processing Fee | Service / Fixed Fees | Offline / Gateway Surcharge |
|---|---|---|---|---|
| **TikTok Shop** | 4.0% $\times$ Subtotal | 3.0% $\times$ Gross | 2,000 ₫ / fulfilled order | 0 ₫ |
| **Shopee (Mall & Standard)** | 4.5% $\times$ Subtotal | 4.0% $\times$ Gross | Freeship Xtra 2.0% (Cap at 20,000 ₫) | 0 ₫ |
| **In-Store POS** | 0% | 1.0% $\times$ Gross (Only on Card/QR payments) | 0 ₫ | Cash payment: 0% fee |

---

## 3. Phase 2: Information Architecture & UI/UX Design

* **Detailed Specifications:**
  * Information Architecture & Screen Hierarchy: [docs/information_architecture_en.md](docs/information_architecture_en.md) (Vietnamese: [docs/information_architecture.md](docs/information_architecture.md))
  * UI/UX Layouts & Component Tokens: [docs/uiux_specifications_en.md](docs/uiux_specifications_en.md) (Vietnamese: [docs/uiux_specifications.md](docs/uiux_specifications.md))
  * Interactive Prototype Artifact: [docs/stitch_prototype.html](docs/stitch_prototype.html)

### 3.1. 3 Core Business Domains
The system decouples operational responsibilities into 3 distinct bounded contexts to prevent data contamination:

![3 Core Business Domains](docs/screenshots/Multi-Channel%20Revenue-domain.png)

### 3.2. End-to-End Data Lifecycle
Tracks immutable financial state transitions from initial cart placement to bank settlement:

![End-to-End Data Lifecycle](docs/screenshots/Multi-Channel%20Revenue-endtoend.png)

### 3.3. 5 Core Accounting Data Integrity Rules
1. **Phantom Revenue Elimination:** Only orders reaching `DELIVERED` status are recognized in revenue metrics and KPI summaries. All `PENDING`, `SHIPPED`, and `CANCELLED` transactions are 100% excluded.
2. **Immutable Realized Ledger:** Once an order transitions to `DELIVERED`, all modification functions regarding SKU items, selling prices, and quantities are permanently locked.
3. **Shop Voucher Boundary Validation:** Enforces strict mathematical limits $0 \le \text{shop\_voucher} \le \text{Subtotal}$ to prevent negative net payable errors.
4. **Mandatory Discrepancy Justification:** During wallet reconciliation, if $\text{Variance} \ne 0$ (e.g., carrier weight penalties or platform adjustments), the accountant must provide a written audit explanation before saving.
5. **Instant In-Store Settlement:** In-store POS orders completed via cash or card swipe transition immediately to `DELIVERED`, bypassing shipping queues.

---

## 4. Screen Hierarchy & Production UI Implementation

### 4.1. Screens Hierarchy Tree
A 5-tier interaction architecture structured under the 3-click rule for fast operational access:

![Screens Hierarchy Tree - FASHION-WEB](docs/screenshots/Multi-Channel%20Revenue-Screens%20Hierarchy%20Tree.png)

### 4.2. Financial Numerical Typography Rule
Adhering to enterprise B2B SaaS standards (inspired by Salesforce Lightning / FlowX), the interface enforces strict numerical coloring:
* **Crimson Red (`#c5221f`):** Reserved exclusively for costs, liabilities, and deductions borne by the merchant (platform commission, payment fees, fixed fees, shop vouchers, carrier weight penalties/negative variances) and loss incident counts (`3 orders` discrepancy, cancelled orders).
* **Uniform Dark Neutral (`#0f172a`):** Applied to ALL standard statistical metrics (Gross sales, net settlement payout, total orders, delivered order volume, in-transit count, average order value, SKU unit sales). No decorative green or blue text is used for standard figures.

---

### 4.3. 3 Core Screen Interfaces

#### Screen 1: Multi-Channel Orders Management (SCR-01)
Engineered for Sales and Fulfillment teams to track multi-channel order streams across TikTok Shop, Shopee, and In-Store POS:
* Top Action Bar: Prominent primary action **`[+ Create New Order]`** (Brand Blue `#0052cc`) for manual order creation, accompanied by view toggles `[Customer Switch]`, `[Kanban View]`, `[Dashboard View]`, and `[Refresh]`.
* 5 Operational Metric Cards: Total Orders (1,248), Delivered (1,180), Gross Sales (184.5M ₫), In Transit (42), Cancelled (26).
* Operational Rhythm Strip & Vertical Bar Chart showing channel order distribution (TikTok 45%, Shopee 38%, POS 17%) with a high-value priority order watchlist (Largest Orders).
* Multi-channel orders ledger with inline quick-action buttons `[Ship]`, `[Delivered]`, `[Cancel]`.

![Screen 1: Multi-Channel Orders Management](docs/screenshots/scr_1.png)

---

#### Screen 2: Marketplace Fees & Wallet Settlement (SCR-02)
Engineered for Finance teams to audit platform deductions and execute bank statement reconciliation:
* 3 Settlement Summary Cards: Pending Settlement (18 orders), 100% Reconciled (1,159 orders), Discrepancy (3 orders).
* Granular Fee Deduction Ledger: Unbundles Commission Fee, Payment Processing Fee, Freeship/Service Fee, Total Platform Fees (Red), Projected Net, Actual Net Received, and Variance.
* Audit Resolution Card (#DIS-002): Documents root-cause audit records (such as carrier dimensional re-weighing penalties).

![Screen 2: Marketplace Fees & Wallet Settlement](docs/screenshots/scr_2.png)

---

#### Screen 3: Revenue Dashboard & Executive Reporting (SCR-03)
Engineered for Shop Owners and Executives to evaluate financial health and net realized cash flow:
* 4 Executive KPI Cards: Total Gross Sales (184,500,000 ₫), Total Platform Fees Deducted (28,620,000 ₫ - Red), Net Cash Realized (155,880,000 ₫), Delivered Volume (1,180 orders).
* 7-Day Grouped Bar Chart: Daily side-by-side comparison between Gross Sales (Gross) vs Net Realized Cash (Net) illustrating fee erosion trends.
* Donut Chart showing revenue contribution by channel (TikTok 48%, Shopee 37%, POS 15%) with click-to-drilldown capability and Top 5 Best-Selling SKUs leaderboard.

![Screen 3: Revenue Dashboard & Executive Reporting](docs/screenshots/scr_3.png)

---

### 4.4. 5 Operational Modals & Action Drawers
1. **MOD-01: Create Order Modal (Manual / Multi-Channel):** Form for livestream, hotline, or walk-in orders with real-time fee preview: Subtotal -> Less Shop Voucher (Red) -> Gross Payable -> Itemized Platform Fees (Red) -> Projected Net Settlement (Dark).
2. **MOD-02: Cancel Order Confirmation Modal:** Enforces cancellation reason tracking and excludes cancelled orders 100% from revenue recognition.
3. **MOD-03: Bank Statement Import Modal:** Uploads bank statement spreadsheets (.xlsx, .csv) for automated matching and variance flagging.
4. **MOD-04: Fee Schedule Configuration Modal:** Configures commission percentages, payment gateway fees, and fixed service fees per channel in real time (Strategy Pattern).
5. **MOD-05: Source Order Drilldown Modal:** Displays granular constituent DELIVERED orders driving executive KPI cards with CSV export support.

---

## 5. Project Directory Structure

```plaintext
QuanLyDoanhThu_LoiNhuan_Dang_Duc_Hoa_Intern_VSF/
├── README.md                              # Main project report (Vietnamese)
├── README_en.md                           # Main project report (English - Current File)
├── docs/                                  # Specifications and design documentation
│   ├── requirements_invest.md             # 10 User Stories under INVEST & BDD (Vietnamese)
│   ├── requirements_invest_en.md          # 10 User Stories under INVEST & BDD (English)
│   ├── usecase.md                         # Use Case Specifications (English)
│   ├── usecase_vi.md                      # Use Case Specifications (Vietnamese)
│   ├── information_architecture.md        # Information Architecture & Screen Hierarchy (Vietnamese)
│   ├── information_architecture_en.md     # Information Architecture & Screen Hierarchy (English)
│   ├── uiux_specifications.md             # UI/UX Layouts & Component Tokens (Vietnamese)
│   ├── uiux_specifications_en.md          # UI/UX Layouts & Component Tokens (English)
│   ├── stitch_prototype.html              # Standalone interactive UI prototype
│   ├── schema.dbml                        # Relational Database Schema (DBML)
│   ├── C4_AND_ARC42_GUIDE.md              # Architectural Reference Guide (C4 & Arc42)
│   └── screenshots/                       # Architectural diagrams and interface screenshots
│       ├── mindmap_multi_channel_revenue.png
│       ├── usecase_multi_channel_revenue.png
│       ├── Multi-Channel Revenue-domain.png
│       ├── Multi-Channel Revenue-endtoend.png
│       ├── Multi-Channel Revenue-Screens Hierarchy Tree.png
│       ├── scr_1.png                      # Production screenshot: Screen 1
│       ├── scr_2.png                      # Production screenshot: Screen 2
│       └── scr_3.png                      # Production screenshot: Screen 3
```

---

## 6. Next Phase Roadmap (Phase 3: Technical Architecture & Database Design)

1. **System Architecture Design under Arc42 & C4 Model Standards:**
   * Author comprehensive architectural documentation: Level 1 (System Context), Level 2 (Container Diagram), Level 3 (Component Diagram), and Level 4 (Code / Sequence Diagram).
   * Specify asynchronous webhook ingestion, message queues (Async Message Queue / Transactional Outbox Pattern), and idempotency validation to prevent double-counting.
2. **Relational Database Design & Data Modeling:**
   * Finalize relational schema definition (`schema.dbml`).
   * Compile formal Data Dictionary, enforce exact currency storage using `DECIMAL(15,2)`, specify foreign key constraints, establish performance indexes for high-volume settlement querying, and output PostgreSQL DDL migration scripts.
