<div align="center">

# Fashion Revenue & Profit Management System

**Multi-Channel Revenue & Cash Flow Settlement Management Platform**

*Eradicate Phantom Revenue · Automate Marketplace Fee Deductions · Audit Wallet Settlements*

[![Architecture](https://img.shields.io/badge/architecture-arc42%20%2B%20C4-blue)](docs/architecture/arc42_en.md)
[![Requirements](https://img.shields.io/badge/requirements-INVEST%20%26%20BDD-brightgreen)](docs/requirements_invest.md)
[![UI Tokens](https://img.shields.io/badge/design-Salesforce%20Lightning%20B2B-purple)](docs/uiux_specifications.md)
[![Database](https://img.shields.io/badge/database-PostgreSQL%2016-1e40af)](docs/database/schema.dbml)
[![Prototype](https://img.shields.io/badge/prototype-Live%20HTML%20Demo-orange)](docs/stitch_prototype.html)

</div>

---

## 1. Project Overview

In multi-channel fashion retail (TikTok Shop, Shopee, In-Store POS), enterprises frequently suffer from the **"Paper Profit, Negative Cash Flow"** dilemma: Gross Merchandise Value (GMV) appears strong on sales dashboards, but actual bank balances fall short due to complex platform fee deductions, shipping weight penalties, customer returns, baseline merchandise costs, and premature revenue recognition.

The **Fashion Revenue & Profit Management System** is a purpose-built financial control platform designed to provide transparency into **channel-level contribution profitability**, automate itemized marketplace fee deductions, freeze baseline merchandise cost snapshots, and eliminate settlement discrepancies across sales channels.

> [!NOTE]
> **Canonical Profit Boundary:**
> *"Contribution Profit represents order/channel profitability after marketplace fees and COGS, but before corporate operating expenses and taxes."*
> The system strictly calculates **Contribution Profit** and does **not** calculate company-wide net income (corporate OPEX, store rent, administrative payroll, marketing campaigns, corporate income taxes, and depreciation are intentionally excluded from MVP scope).

### 1.1 System Mindmap (Functional Architecture)
Below is the core conceptual mindmap unbundling the four operational domains of the platform:

![System Functional Mindmap](docs/screenshots/mindmap_multi_channel_revenue.png)

### 1.2 Screens Hierarchy Tree (Navigation Architecture)
The user interface is architected into 4 top-level workspaces (Level 1), interactive action panels (Level 2), and specialized modals / drawers (Level 3) for transactional control:

```mermaid
flowchart TB
    %% Level 0: Global Shell
    subgraph Level0 [" 🌐 LEVEL 0: GLOBAL APPLICATION SHELL "]
        shell["<b>FASHION-WEB SYSTEM SHELL</b><br/>Brand: Fashion Revenue & Profit Management System"]
        topbar["<b>Top Header & Operational Context</b><br/>• Multi-Channel Status: TikTok Shop | Shopee | Retail POS Active<br/>• User Context & Role Switcher: Sales/Ops | Finance Manager | Shop Owner"]
        shell --> topbar
    end

    %% Level 1: Main Navigation Tabs
    subgraph Level1 [" 📑 LEVEL 1: MAIN NAVIGATION TABS "]
        direction LR
        tab1["<b>[TAB 1 / SCR-01]</b><br/>ORDERS MANAGEMENT"]
        tab2["<b>[TAB 2 / SCR-02]</b><br/>FEES & WALLET SETTLEMENT"]
        tab3["<b>[TAB 3 / SCR-03]</b><br/>REVENUE & PROFIT DASHBOARD"]
        tab4["<b>[TAB 4 / SCR-04]</b><br/>CATALOG & COST MANAGEMENT"]
    end

    topbar --> tab1
    topbar --> tab2
    topbar --> tab3
    topbar --> tab4

    %% Screen 1: Orders Management
    subgraph SCR01 [" 📦 Screen 1: Orders Management (SCR-01) "]
        direction TB
        p11["<b>[Panel 1.1] Toolbar & Status Filters</b><br/>• Status tabs: All | Pending | Shipped | Delivered | Cancelled<br/>• Search: Order ID / SKU / Customer Phone<br/>• Action: [Create New Order]"]
        p12["<b>[Panel 1.2] Multi-Channel Orders Table</b><br/>• Columns: Order ID | Channel | Date | Customer | Items | Gross Revenue | Status<br/>• Row Actions: [Ship Order] | [Deliver Order] | [Cancel Order]"]
        p11 --> p12

        mod01["<b>[MOD-01] Create Order Modal</b><br/>• Channel: TikTok / Shopee / POS<br/>• SKU Selector (Integrated Catalog)<br/>• Price & Qty | Shop Voucher<br/>• Dynamic Cost Snapshot Freezing<br/>• Live Backend Fee & Settlement Preview<br/>• Action: [Save Order]"]
        mod02["<b>[MOD-02] Cancel Order Modal</b><br/>• Mandatory Reason Code & Note<br/>• Financial Warning: 0 Revenue, COGS, Profit<br/>• Action: [Confirm Cancel]"]

        p11 -.->|Click Create| mod01
        p12 -.->|Click Cancel| mod02
    end
    tab1 --> p11

    %% Screen 2: Fees & Wallet Settlement
    subgraph SCR02 [" 💳 Screen 2: Fees & Wallet Settlement (SCR-02) "]
        direction TB
        p21["<b>[Panel 2.1] Settlement Status Counters</b><br/>• Badges: Pending Settlement | Reconciled (Clean Match) | Discrepancy<br/>• Filter: Settlement Date Range & Status"]
        p22["<b>[Panel 2.2] Fee Breakdown & Settlement Ledger</b><br/>• Columns: Order ID | Channel | Customer Paid | Commission | Payment | Service | Total Fees | Projected Settlement | Actual Settlement | Variance<br/>• Row Action: [Reconcile]"]
        p21 --> p22

        mod03["<b>[MOD-03] Wallet Settlement Drawer</b><br/>• Projected Settlement Summary<br/>• Input: Actual Net Cash Received<br/>• Auto-calculated: Variance = Projected - Actual<br/>• Mandatory Reason Note if Variance != 0<br/>• Action: [Confirm Settlement]"]
        mod05["<b>[MOD-05] Discrepancy Audit Drawer</b><br/>• Root Cause Classification<br/>• Detailed Dispute Notes<br/>• Action: [Resolve Dispute]"]

        p22 -.->|Click Reconcile| mod03
        p22 -.->|Investigate Variance| mod05
    end
    tab2 --> p21

    %% Screen 3: Revenue & Profit Dashboard
    subgraph SCR03 [" 📊 Screen 3: Revenue & Profit Dashboard (SCR-03) "]
        direction TB
        p31["<b>[Panel 3.1] Global Filters & Actions</b><br/>• Date Presets: Today | 7 Days | 30 Days | Custom<br/>• Channel Selector: All | TikTok | Shopee | POS<br/>• Action: [Export CSV Report]"]
        p32["<b>[Panel 3.2] Executive Financial KPI Cards (5 Cards)</b><br/>• Card 1: Gross Revenue<br/>• Card 2: Total Platform Fees Deducted<br/>• Card 3: Net Realized Revenue (Projected Settlement)<br/>• Card 4: Cost of Goods Sold (COGS)<br/>• Card 5: Contribution Profit (with Contribution Margin %)"]
        p33["<b>[Panel 3.3] Visual Analytics Panels</b><br/>• Trend: Gross Revenue vs Contribution Profit over time<br/>• Donut: Revenue & Profit Share by Channel (%)<br/>• Leaderboard: Top 5 Best-Selling SKUs (Revenue & Contribution Margin)"]
        p31 --> p32 --> p33

        mod04["<b>[MOD-04] Source Order Drilldown Modal</b><br/>• Filtered constituent DELIVERED orders<br/>• Itemized Fee & Cost Breakdown<br/>• Action: [Export Drilldown CSV]"]

        p32 -.->|Click KPI Card| mod04
        p33 -.->|Click Chart / Slice| mod04
    end
    tab3 --> p31

    %% Screen 4: Product Catalog & Cost Management
    subgraph SCR04 [" 🏷️ Screen 4: Catalog & Cost Management (SCR-04) "]
        direction TB
        p41["<b>[Panel 4.1] Catalog Toolbar & Filters</b><br/>• Search SKU Code / Product Name<br/>• Active Status Toggle<br/>• Action: [Add New Product] | [Export Catalog]"]
        p42["<b>[Panel 4.2] Product & Variant SKU Ledger</b><br/>• Columns: Master Product | Variant SKU | Color | Size | Retail Selling Price | Baseline Unit Cost (cost_price) | Unit Gross Margin | Status<br/>• Row Action: [Edit Pricing & Cost]"]
        p41 --> p42

        mod06["<b>[MOD-06] Configure SKU Pricing & Cost Modal</b><br/>• Listed Retail Selling Price Input<br/>• Baseline Unit Cost Input (cost_price >= 0)<br/>• Role Guard: Finance / Shop Owner Only<br/>• Real-time Unit Margin % Preview<br/>• Action: [Save Changes]"]
        mod07["<b>[MOD-07] Add Product & Variant Matrix Modal</b><br/>• Master Product Title<br/>• Variant Matrix Generator (Color × Size)<br/>• Initial Retail Price & Baseline Cost<br/>• Action: [Create Product]"]

        p41 -.->|Click Add Product| mod07
        p42 -.->|Click Edit Pricing/Cost| mod06
    end
    tab4 --> p41

    %% Level 4: Interactive Toast Notifications
    subgraph Level4 [" 🔔 LEVEL 4: INTERACTIVE TOAST NOTIFICATIONS "]
        direction LR
        t1["Toast: Order Created Successfully"]
        t2["Toast: Lifecycle Status Transitioned"]
        t3["Toast: Settlement Reconciled / Discrepancy Logged"]
        t4["Toast: SKU Pricing & Baseline Cost Updated"]
        t5["Toast: CSV Financial Report Exported"]
    end

    mod01 -.->|Emits Event| t1
    p12 -.->|Emits Event| t2
    mod03 -.->|Emits Event| t3
    mod06 -.->|Emits Event| t4
    p31 -.->|Emits Event| t5
```

---

## 2. Business Problem & Accounting Principles

### 2.1 The Core Equations

#### Payout & Settlement Control:
$$\text{Gross Revenue} = \text{Subtotal} - \text{Shop Voucher}$$
$$\text{Projected Settlement} = \text{Gross Revenue} - \text{Total Platform Fees}$$
$$\text{Settlement Variance} = \text{Projected Settlement} - \text{Actual Settlement}$$

#### Commercial Contribution Profitability:
$$\text{COGS} = \sum (\text{Quantity} \times \text{Unit Cost Snapshot})$$
$$\text{Contribution Profit} = \text{Projected Settlement} - \text{COGS} = \text{Gross Revenue} - \text{Total Platform Fees} - \text{COGS}$$
$$\text{Contribution Margin \%} = \frac{\text{Contribution Profit}}{\text{Gross Revenue}} \times 100 \quad (\text{when } \text{Gross Revenue} > 0)$$

Where:
- $\text{Total Platform Fees} = \text{Commission Fee} + \text{Payment Processing Fee} + \text{Service Fee} + \text{Fixed Fee}$
- $\text{Unit Cost Snapshot}$ is the frozen baseline merchandise cost from the catalog at order placement time.
- Terminology Constraint: The term **Net Profit** is strictly prohibited; the canonical metric is **Contribution Profit**.

### 2.2 Core Financial Control Rules
1. **Anti-Phantom Revenue & Profit Invariant:** Orders in `PENDING`, `SHIPPED`, or `CANCELLED` status contribute **0 ₫** to recognized revenue, COGS, and contribution profit. Commercial financial metrics are officially recognized into reports if and only if the order reaches `DELIVERED`.
2. **Immutable Financial & Cost Snapshots:** Once an order is created, item baseline unit costs are frozen into `order_items.unit_cost_snapshot`. When an order is delivered, applied fee rates, deductions, and projected settlement are permanently frozen to preserve historical audit integrity.
3. **Canonical Variance Formula:** Discrepancy detection between expected and received payouts strictly follows `variance_amount = projected_settlement - actual_settlement`.
4. **Mandatory Discrepancy Justification:** Any non-zero settlement variance requires finance review notes before resolution.
5. **Cost Access Security:** Baseline unit cost (`cost_price`), COGS, and Contribution Profit are restricted to **Shop Owner** and **Finance Manager** roles; **Sales & Operations Staff** have read-only SKU selection without cost visibility.

---

## 3. Core Features & Workspaces

The platform provides four cohesive operational workspaces tailored to business roles:

### 3.1 Screen 1: Multi-Channel Orders Management (SCR-01)
- Records orders from TikTok Shop, Shopee, and In-Store POS channels.
- Product selector integrated with catalog SKUs, freezing item unit costs into immutable snapshots.
- Real-time backend fee preview during manual order composition.
- Governs lifecycle status progression (`Pending` $\rightarrow$ `Shipped` $\rightarrow$ `Delivered` / `Cancelled`).

![Screen 1: Orders Management](docs/screenshots/scr_1.png)

### 3.2 Screen 2: Fees & Wallet Settlement (SCR-02)
- Granular itemization of commission fees, payment processing fees, and service charges.
- Two-way reconciliation matching internal delivered orders against payout statements.
- Slide-over audit drawer for investigating and resolving settlement discrepancies.

![Screen 2: Fees & Settlement](docs/screenshots/scr_2.png)

### 3.3 Screen 3: Revenue & Profit Dashboard (SCR-03)
- Executive KPI summary cards (Gross Revenue, Total Platform Fees, Net Realized Revenue, COGS, Contribution Profit).
- Comparative cash flow and contribution profit trend charts evaluating revenue against marketplace deductions and merchandise costs.
- Channel revenue share distribution and Top 5 SKU leaderboard by revenue and contribution profit.

![Screen 3: Revenue Dashboard](docs/screenshots/scr_3.png)

### 3.4 Screen 4: Product Catalog & Cost Management (SCR-04)
- Centralized master product catalog and variant SKU management (Color, Size, SKU Code).
- Retail selling price and baseline unit merchandise cost configuration (`cost_price >= 0`).
- Role-gated interface: Cost baseline editing is strictly restricted to Shop Owner and Finance Manager.
- Real-time gross margin preview per SKU before channel-specific platform deductions.

---

## 4. Architecture Summary

The system follows a strict, decoupled three-tier architecture:

```
[ User / Operator ]
        │  (HTTPS / Browser)
        ▼
[ React Web SPA ]
        │  (HTTPS / REST / JSON)
        ▼
[ ASP.NET Core Backend API ]
        │  (TCP / SQL via EF Core 8 & Npgsql)
        ▼
[ PostgreSQL Database ]
```

- **React Web SPA:** Pure client-side presentation layer with zero canonical financial calculation logic.
- **ASP.NET Core Backend API:** Authoritative boundary enforcing state machine transitions, Strategy-based fee calculations, and transaction orchestration.
- **PostgreSQL Database:** Relational ACID transactional storage preserving immutable orders, fee schedules, snapshots, and reconciliation ledgers.

---

## 5. Technology Stack

| Layer | Technologies |
| :--- | :--- |
| **Frontend** | React 18, TypeScript, Vite, React Router 6, Axios |
| **Backend** | ASP.NET Core 8 (`net8.0`), C# 12, Entity Framework Core 8, Npgsql, Swagger / OpenAPI |
| **Database** | PostgreSQL 16+ (Normalized 3NF relational schema, UUID PKs, `numeric(15,2)` currency precision) |
| **Design System** | Salesforce Lightning B2B design tokens, tabular figures (`tnum`), English-only UI, VND currency |

---

## 6. Documentation Map

| Area | Document Path | Description |
| :--- | :--- | :--- |
| **Architecture Overview** | [`docs/architecture/arc42_en.md`](docs/architecture/arc42_en.md) | Standard arc42 comprehensive architecture documentation |
| **C4 Context (Level 1)** | [`docs/architecture/c4-context_en.md`](docs/architecture/c4-context_en.md) | System boundary, human actors, and external system relationships *(LOCKED)* |
| **C4 Container (Level 2)**| [`docs/architecture/c4-container_en.md`](docs/architecture/c4-container_en.md) | Deployable containers, communication protocols, and boundaries *(LOCKED)* |
| **C4 Backend Component** | [`docs/architecture/c4-component-backend_en.md`](docs/architecture/c4-component-backend_en.md) | Backend controllers, services, fee strategy engine, and repository ports *(LOCKED)* |
| **C4 Frontend Component**| [`docs/architecture/c4-component-frontend_en.md`](docs/architecture/c4-component-frontend_en.md) | Frontend layout, routing, feature modules, hooks, and shared foundation *(LOCKED)* |
| **Database Schema (DBML)**| [`docs/database/schema.dbml`](docs/database/schema.dbml) | Canonical source of truth for PostgreSQL database design |
| **Database Specification**| [`docs/database/database-design_en.md`](docs/database/database-design_en.md) | English relational schema design specification, ERD, and table catalog |
| **Database Constraints & Indexes** | [`docs/database/database-constraints-indexes_en.md`](docs/database/database-constraints-indexes_en.md) | PostgreSQL constraints, check integrity, index matrix, and ACID transactions |
| **User Stories & BDD** | [`docs/requirements_invest.md`](docs/requirements_invest.md) | INVEST user stories and Gherkin acceptance criteria |
| **Use Cases & RBAC** | [`docs/usecase.md`](docs/usecase.md) | Actor specifications, UML use cases, and role-based access matrix |
| **UI/UX Specifications** | [`docs/uiux_specifications.md`](docs/uiux_specifications.md) | Design tokens, color rules, layout grid, and screen hierarchy |

---

## 7. Repository Structure

```plaintext
Quanly_DoanhThu_LoiNhuan_DangDucHoa_VSF/
├── README.md                              # Main project landing page & overview
├── backend/                               # ASP.NET Core 8 Web API solution
│   ├── src/
│   │   ├── FashionWeb.Api/                # HTTP API Controllers, DI registration, Middlewares
│   │   ├── FashionWeb.Business/           # Domain entities, fee strategies, interfaces, DTOs
│   │   └── FashionWeb.Data/               # EF Core DbContext, PostgreSQL repositories, migrations
│   └── tests/
│       └── FashionWeb.Business.Tests/     # Automated domain & strategy unit tests
├── frontend/                              # React 18 / TypeScript SPA
│   ├── src/
│   │   ├── app/                           # App routing and application providers
│   │   ├── layouts/                       # Application shell, sidebar, topbar
│   │   ├── pages/                         # Route-bound view orchestrators (Orders, Settlement, Dashboard, Catalog)
│   │   ├── features/                      # Domain-specific UI, custom hooks, and API services (orders, settlement, analytics, catalog)
│   │   └── shared/                        # Primitives, API client, formatters, copy dictionaries
│   └── package.json
└── docs/                                  # Project architecture and technical specifications
    ├── architecture/                      # arc42 and C4 Level 1-3 specifications
    ├── database/                          # DBML schema and relational design specifications
    ├── references/                        # Architectural references and sample guides
    ├── screenshots/                       # Application UI screenshots and verified diagrams
    ├── archive/                           # Archived legacy assets
    ├── requirements_invest.md             # INVEST user stories
    ├── usecase.md                         # UML use cases and RBAC matrix
    ├── information_architecture.md        # Data dictionary and accounting control rules
    └── uiux_specifications.md             # Design tokens and visual specifications
```

---

