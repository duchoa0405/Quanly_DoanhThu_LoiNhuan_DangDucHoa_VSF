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

In multi-channel fashion retail (TikTok Shop, Shopee, In-Store POS), enterprises frequently suffer from the **"Paper Profit, Negative Cash Flow"** dilemma: Gross Merchandise Value (GMV) appears strong on sales dashboards, but actual bank balances fall short due to complex platform fee deductions, shipping weight penalties, customer returns, and premature revenue recognition.

The **Fashion Revenue & Profit Management System** is a purpose-built financial control platform designed to provide transparency into real commercial earnings, automate itemized deduction calculations, and eliminate accounting discrepancies across sales channels.

---

## 2. Business Problem & Accounting Principles

### 2.1 The Core Equation
$$\text{Net Realized Cash Flow} = \text{Gross Revenue} - \text{Platform Fees} \pm \text{Settlement Variance}$$

Where:
- $\text{Gross Revenue} = \text{Subtotal} - \text{Shop Voucher}$
- $\text{Platform Fees} = \text{Commission Fee} + \text{Payment Processing Fee} + \text{Fixed/Service Fees}$
- $\text{Settlement Variance} = \text{Projected Settlement} - \text{Actual Settlement}$

### 2.2 Core Financial Control Rules
1. **Anti-Phantom Revenue Invariant:** Orders in `PENDING`, `SHIPPED`, or `CANCELLED` status contribute **0 ₫** to realized revenue. Revenue is officially recognized into accounting periods if and only if the order reaches `DELIVERED`.
2. **Immutable Financial Snapshots:** Once an order is delivered, applied fee rates, deductions, and net margins are permanently frozen to preserve historical audit integrity.
3. **Canonical Variance Formula:** Discrepancy detection between expected and received payouts strictly follows $\text{variance\_amount} = \text{projected\_settlement} - \text{actual\_settlement}$.
4. **Mandatory Discrepancy Justification:** Any non-zero settlement variance requires finance review notes before resolution.

---

## 3. Core Features & Workspaces

The platform provides three cohesive operational workspaces tailored to business roles:

### 3.1 Screen 1: Multi-Channel Orders Management (SCR-01)
- Ingests orders across TikTok Shop, Shopee, and In-Store POS.
- Real-time backend fee preview during manual order composition.
- Governs lifecycle status progression (`Pending` $\rightarrow$ `Shipped` $\rightarrow$ `Delivered` / `Cancelled`).

![Screen 1: Orders Management](docs/screenshots/scr_1.png)

### 3.2 Screen 2: Fees & Wallet Settlement (SCR-02)
- Granular itemization of commission fees, payment processing fees, and service charges.
- Two-way reconciliation matching internal delivered orders against payout statements.
- Slide-over audit drawer for investigating and resolving settlement discrepancies.

![Screen 2: Fees & Settlement](docs/screenshots/scr_2.png)

### 3.3 Screen 3: Revenue Dashboard & Executive Reporting (SCR-03)
- Executive KPI summary cards (Gross Revenue, Platform Fees, Net Realized Revenue, Delivered Volume).
- Cash flow comparison chart evaluating gross earnings against net cash payouts.
- Channel revenue share distribution and Top 5 SKU revenue leaderboard with source order drilldown.

![Screen 3: Revenue Dashboard](docs/screenshots/scr_3.png)

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
│   │   ├── pages/                         # Route-bound view orchestrators (Orders, Settlement, Dashboard)
│   │   ├── features/                      # Domain-specific UI, custom hooks, and API services
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

