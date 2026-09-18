# Software Architecture Documentation: Fashion Revenue & Profit Management System (arc42)

## 1. Introduction and Goals

The **Fashion Revenue & Profit Management System** is designed to resolve the critical "Paper Profit, Negative Cash Flow" dilemma in multi-channel fashion retail. The system eliminates phantom revenues, automates complex marketplace fee deductions, audits settlement bank statements, and delivers real-time visibility into net realized financial performance and **channel-level Contribution Profitability**.

> [!NOTE]
> **Canonical Profit Scope:**
> *"Contribution Profit represents order/channel profitability after marketplace fees and COGS, but before corporate operating expenses and taxes."*
> The system calculates **Contribution Profit** (`Gross Revenue - Platform Fees - COGS`) and does **not** calculate company-wide net income (corporate OPEX, store rent, administrative payroll, marketing campaigns, corporate income taxes, and depreciation are intentionally excluded from MVP scope). Calling Contribution Profit *Net Profit*, *Net Income*, or *Operating Profit* is strictly prohibited.

### 1.1. Core Business Goals
- **Anti-Phantom Revenue & Profit Recognition:** Recognize commercial revenue, merchandise COGS, and contribution profit strictly upon successful customer delivery (`DELIVERED`).
- **Dynamic Multi-Channel Fee Unbundling:** Authoritatively calculate and freeze channel deductions (marketplace commissions, payment gateway fees, voucher costs, and fixed fees) based on configurable fee schedules.
- **Product Catalog & Cost Baseline Management:** Maintain SKU retail selling prices and baseline unit costs, freezing immutable cost snapshots upon order placement.
- **Settlement Payout Auditing:** Reconcile projected settlement against actual cash/wallet disbursements, isolating and logging variance discrepancies.
- **Executive Visibility:** Real-time visibility into Gross Revenue, Platform Fees, Net Realized Revenue, COGS, Contribution Profit, and Contribution Margin %.

### 1.2. Requirements & Use Case References
Detailed requirements and functional specifications are maintained in separate dedicated documents to preserve separation of concerns:
- Detailed User Stories & Acceptance Criteria: [`docs/requirements_invest.md`](../requirements_invest.md) (INVEST & BDD Gherkin).
- Actor Boundaries & Use Cases: [`docs/usecase.md`](../usecase.md) (UML Use Cases & 3-Way Traceability).
- Information Architecture & Accounting Rules: [`docs/information_architecture.md`](../information_architecture.md).

---

## 2. Architecture Constraints

The target MVP architecture is governed by the following verified technical and operational constraints:

| Constraint Category | Bound Constraint | Architectural Impact |
| :--- | :--- | :--- |
| **Frontend Framework** | **React 18** (TypeScript, Vite, React Router 6, Axios) | Client-side Single Page Application (SPA). Presentation state only. |
| **Backend Framework** | **ASP.NET Core 8** (C# 12, `net8.0`, Web API) | Modular monolith backend container enforcing canonical business logic and transactional integrity. |
| **Persistence RDBMS**| **PostgreSQL 16+** | Relational transactional database accessed via Entity Framework Core 8 and Npgsql provider. |
| **Language & UI Policy**| **English-Only UI** (P03.5) | 100% of user-facing UI labels, tables, tooltips, and action texts are in English (`shared/constants/uiCopy.ts`). |
| **Financial Localization**| **VND Currency & Asia/Ho_Chi_Minh Timezone** | Currency formatted as `184,500,000 ₫`. Timestamps stored in UTC and rendered in UTC+7 (`Asia/Ho_Chi_Minh`). |
| **Marketplace Workflow**| **MVP Manual & Assisted Portal Workflow** | Order ingestion and statement verification operate via user-assisted web workflows and Excel/CSV imports. Direct external API/webhook integrations are deferred to post-MVP. |
| **Simplified Baseline COGS**| **Manually Maintained Unit Cost** | `product_variants.cost_price` represents standard baseline unit cost. Automated inventory valuation engines (FIFO/LIFO/Moving Weighted Average), purchase orders, and warehouse receiving ledgers are excluded. |
| **Corporate Scope Boundary**| **No Enterprise General Ledger / OPEX** | Administrative payroll, store rent, marketing OPEX, corporate income taxes, and asset depreciation are strictly out-of-scope for MVP. |

---

## 3. Context and Scope

The system boundary, human actors, and external system interactions are formally defined and maintained in:
- **C4 Level 1 System Context:** [`docs/architecture/c4-context_en.md`](c4-context_en.md) *(LOCKED)*

### 3.1. Context Summary
- **Human Actors:** Sales & Operations Staff, Finance Manager, Shop Owner.
- **Core System Boundary:** Fashion Revenue & Profit Management System.
- **External Systems:** Marketplace Seller Centers (TikTok Shop, Shopee) and Commercial Banking Portals (Vietcombank, MB, etc.). In the MVP, interactions are mediated by human operators using standard web workflows.
- **In-Scope Capabilities:** Multi-Channel Order Ingestion, Dynamic Platform Fee Calculation, Manual Payout Reconciliation, Product Catalog & Baseline Cost Maintenance, and Revenue & Contribution Profit Analytics.

---

## 4. Solution Strategy

The high-level technical strategy balances low operational complexity with strict financial correctness:
1. **Three-Tier Container Topology:** Strict unidirectional flow: `User -> React Web SPA -> ASP.NET Core Backend API -> PostgreSQL Database`.
2. **Authoritative Server-Side Business Rules:** The frontend is purely a presentation layer. All financial calculations (fee deduction formulas, gross revenue, net settlement, COGS, contribution profit, variance detection) are authoritatively executed on the Backend API.
3. **Immutable Financial & Cost Snapshots:** Order line items freeze baseline merchandise costs (`order_items.unit_cost_snapshot`) at order creation. Delivered orders freeze applied fee schedules into `order_fee_snapshots`.
4. **Canonical Contribution Profit Derivation:** Contribution Profit is dynamically derived (`Projected Settlement - COGS` or `Gross Revenue - Platform Fees - COGS`), avoiding duplicate source of truth persistence bugs.
5. **Modular Monolith Backend Architecture:** Domain logic and data access are packaged as internal class libraries (`FashionWeb.Business`, `FashionWeb.Data`) compiled into the backend container, avoiding distributed microservices overhead for MVP.
6. **Feature-Oriented Frontend Architecture:** Frontend components are organized by business domain (`features/orders/`, `features/settlements/`, `features/discrepancies/`, `features/analytics/`, `features/catalog/`) with dedicated hooks and services, decoupled from shared UI primitives.
7. **Strategy Pattern for Channel Fees:** Channel fee algorithms (TikTok, Shopee, POS) are encapsulated in strategy components resolved via the Dynamic Fee Engine.

---

## 5. Building Block View

The structural decomposition of the system is formally specified across three architectural levels:
- **Level 2 Container Architecture:** [`docs/architecture/c4-container_en.md`](c4-container_en.md) *(LOCKED)*
  - Decomposes the system into `React Web SPA`, `ASP.NET Core Backend API`, and `PostgreSQL Database`.
- **Level 3 Backend Component Architecture:** [`docs/architecture/c4-component-backend_en.md`](c4-component-backend_en.md) *(LOCKED)*
  - Decomposes the backend into Presentation Controllers (`OrdersController`, `SettlementController`, `AnalyticsController`, `CatalogController`), Application Services (`OrderService`, `SettlementService`, `AnalyticsService`, `CatalogService`), Domain Rules & Fee Strategies, Persistence Ports (`IOrderRepository`, `IFeeScheduleRepository`, `IReconciliationRepository`, `IProductRepository`, `IAnalyticsRepository`), and EF Core Data Adapters.
- **Level 3 Frontend Component Architecture:** [`docs/architecture/c4-component-frontend_en.md`](c4-component-frontend_en.md) *(LOCKED)*
  - Decomposes the frontend into App & Routing (`/orders`, `/settlement`, `/analytics`, `/catalog`), Pages Layer (`OrdersPage`, `SettlementPage`, `RevenueDashboardPage`, `CatalogPage`), Feature Components (`ProductTable`, `ProductEditor`, `PricingCostEditor`, `ProductSelector`), Feature Hooks (`useCatalog`), Feature Services (`CatalogService`), and Shared Frontend Foundation.

---

## 6. Runtime View

*To be completed from Sequence Diagrams during Phase 3 detailed design.*

---

## 7. Deployment View

*To be completed during Deployment Architecture design.*

---

## 8. Crosscutting Concepts

The following crosscutting concerns are standardized across all system tiers:
- **Role-Based Access Control (RBAC):** UI element visibility governed by role contexts in the SPA; authoritative authorization enforced on the Backend API via ASP.NET Core policies (`[Authorize(Roles = "...")]`).
- **Financial Cost & Contribution Profit Integrity:**
  - Baseline merchandise unit cost is frozen into `order_items.unit_cost_snapshot` at order creation, guaranteeing that line COGS remains permanently immutable against retroactive catalog cost adjustments.
  - Canonical Contribution Profit is dynamically derived as `Projected Settlement - COGS` (or `Gross Revenue - Total Platform Fees - COGS`).
  - Anti-phantom profit recognition: revenue, COGS, and Contribution Profit are recognized strictly upon `DELIVERED`. Cancelled orders contribute 0 to all financial metrics.
  - Role-sensitive cost data visibility: baseline unit cost, COGS, and Contribution Profit are visible strictly to `Shop Owner` and `Finance Manager`; `Sales & Operations Staff` have read-only SKU selection with zero cost visibility.
- **Global Error Handling & API Boundary:** Centralized backend middleware converts unhandled domain exceptions into standardized `RFC 7807 ProblemDetails`. Controllers do not perform ad-hoc exception mapping.
- **English-Only UI Standards:** Centralized user-facing dictionary in `src/shared/constants/uiCopy.ts`.
- **Financial Typography & Color Tokens:** Dark neutral (`#0f172a`) for values, Crimson Red (`#c5221f`) strictly for expense deductions/losses, muted slate (`#64748b`) for zero amounts, and tabular figures (`tnum`) for alignment.
- **Dependency Inversion in Persistence:** Business domain logic defines repository interfaces (`IOrderRepository`, `IFeeScheduleRepository`, `IProductRepository`, etc.); data layer implements them via EF Core.
- **Immutable Financial Snapshots:** Channel fees and net revenues are calculated and permanently frozen into immutable snapshot records upon order delivery.
- **Transactional Consistency:** Database operations modifying order state and financial records execute within atomic ACID transactions.

---

## 9. Architecture Decisions (ADR Summary)

Key architectural decisions consolidated from C4 Level 2, 3 (Backend), and 3 (Frontend) specifications:
- **ADR-01:** Decoupling of Presentation (SPA) from Business Logic (API) ([c4-container_en.md](c4-container_en.md)).
- **ADR-02:** PostgreSQL as the Single Persistent Transactional Source of Truth ([c4-container_en.md](c4-container_en.md)).
- **ADR-03:** Strict Tiered Invariant (`SPA -> API -> Database`) ([c4-container_en.md](c4-container_en.md)).
- **ADR-04:** Simplified Baseline COGS instead of Inventory Valuation Engine (Maintain manually updated `cost_price` and freeze into `unit_cost_snapshot` to satisfy MVP contribution profit requirements without enterprise WMS/inventory ledger complexity).
- **ADR-P03-01:** Controllers as Thin HTTP Adapters with Global Error Handling ([c4-component-backend_en.md](c4-component-backend_en.md)).
- **ADR-P03-03:** Dynamic Fee Engine as Sole Calculation Facade ([c4-component-backend_en.md](c4-component-backend_en.md)).
- **ADR-P03-04:** Repository Interfaces in Business, Implementations in Data ([c4-component-backend_en.md](c4-component-backend_en.md)).
- **ADR-FE-01:** Feature-Oriented Structure with Co-located Services & Hooks ([c4-component-frontend_en.md](c4-component-frontend_en.md)).
- **ADR-FE-03:** Backend Authority for Financial Calculations & Fee Preview ([c4-component-frontend_en.md](c4-component-frontend_en.md)).
- **ADR-FE-06:** Strict English-Only UI Copy Policy ([c4-component-frontend_en.md](c4-component-frontend_en.md)).

---

## 10. Quality Requirements

*To be completed from Non-Functional Requirements (NFRs) specification.*

---

## 11. Risks and Technical Debt

*To be completed during Risk Assessment and Technical Debt audit.*

---

## 12. Glossary

| Term | Definition |
| :--- | :--- |
| **Gross Revenue** | Total commercial revenue realized from sales after deducting merchant-funded discounts (`subtotal - shop_voucher`). Does not deduct platform fees. |
| **Platform Fee** | Sum of all deductions charged by marketplace platforms or payment processors (commission, payment gateway fee, shipping subsidies, fixed fees). |
| **Projected Settlement** | The net payout expected from the sales channel or payment processor after deducting platform fees from gross sales (`gross_revenue - total_fees`). Equivalent to Net Realized Revenue. |
| **Actual Settlement** | The actual net funds deposited into the merchant's commercial bank account or marketplace wallet as recorded in bank statements. |
| **Variance** | The mathematical discrepancy between projected settlement and actual disbursement (`variance_amount = projected_settlement - actual_settlement`). |
| **Reconciliation** | The accounting verification workflow comparing internal financial projections against external settlement statements. |
| **Discrepancy** | An unresolved financial variance ($|\text{Variance Amount}| > 0$, where `variance_amount != 0`) requiring investigation, justification notes, and dispute filing. |
| **Net Realized Revenue** | Official recognized revenue retained by the business after all platform deductions have been settled (`Projected Settlement`). |
| **COGS (Cost of Goods Sold)** | Direct merchandise baseline cost associated with delivered order items, computed as $\sum (\text{Quantity} \times \text{Unit Cost Snapshot})$. |
| **Contribution Profit** | Commercial order/channel profitability after marketplace platform fees and baseline merchandise COGS (`Projected Settlement - COGS` or `Gross Revenue - Total Platform Fees - COGS`). |
| **Contribution Margin %** | Contribution Profit expressed as a percentage of Gross Revenue (`(Contribution Profit / Gross Revenue) * 100`). |
| **Accounting Metric Boundary** | Strict distinction: $\text{Net Realized Revenue} \neq \text{Contribution Profit} \neq \text{Net Profit}$. Contribution Profit does not deduct corporate OPEX (rent, administrative salaries, marketing OPEX, taxes, depreciation); labeling it "Net Profit" or "Net Income" is strictly prohibited. |

