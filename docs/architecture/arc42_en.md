# Software Architecture Documentation: Fashion Revenue & Profit Management System (arc42)

## 1. Introduction and Goals

The **Fashion Revenue & Profit Management System** is designed to resolve the critical "Paper Profit, Negative Cash Flow" dilemma in multi-channel fashion retail. The system eliminates phantom revenues, automates complex marketplace fee deductions, audits settlement bank statements, and delivers real-time visibility into net realized financial performance.

### 1.1. Core Business Goals
- **Anti-Phantom Revenue Recognition:** Recognize commercial revenue strictly upon successful customer delivery (`DELIVERED`).
- **Dynamic Multi-Channel Fee Unbundling:** Authoritatively calculate and freeze channel deductions (marketplace commissions, payment gateway fees, voucher costs, and fixed fees) based on configurable fee schedules.
- **Settlement Payout Auditing:** Reconcile projected settlement against actual cash/wallet disbursements, isolating and logging variance discrepancies.
- **Executive Visibility:** Real-time visibility into net realized earnings, channel profitability share, and top-performing merchandise.

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

---

## 3. Context and Scope

The system boundary, human actors, and external system interactions are formally defined and maintained in:
- **C4 Level 1 System Context:** [`docs/architecture/c4-context_en.md`](c4-context_en.md) *(LOCKED)*

### 3.1. Context Summary
- **Human Actors:** Sales & Operations Staff, Finance Manager, Shop Owner.
- **Core System Boundary:** Fashion Revenue & Profit Management System.
- **External Systems:** Marketplace Seller Centers (TikTok Shop, Shopee) and Commercial Banking Portals (Vietcombank, MB, etc.). In the MVP, interactions are mediated by human operators using standard web workflows.

---

## 4. Solution Strategy

The high-level technical strategy balances low operational complexity with strict financial correctness:
1. **Three-Tier Container Topology:** Strict unidirectional flow: `User -> React Web SPA -> ASP.NET Core Backend API -> PostgreSQL Database`.
2. **Authoritative Server-Side Business Rules:** The frontend is purely a presentation layer. All financial calculations (fee deduction formulas, gross revenue, net revenue, variance detection) are authoritatively executed on the Backend API.
3. **Modular Monolith Backend Architecture:** Domain logic and data access are packaged as internal class libraries (`FashionWeb.Business`, `FashionWeb.Data`) compiled into the backend container, avoiding distributed microservices overhead for MVP.
4. **Feature-Oriented Frontend Architecture:** Frontend components are organized by business domain (`features/orders/`, `features/settlements/`, `features/discrepancies/`, `features/analytics/`) with dedicated hooks and services, decoupled from shared UI primitives.
5. **Strategy Pattern for Channel Fees:** Channel fee algorithms (TikTok, Shopee, POS) are encapsulated in strategy components resolved via the Dynamic Fee Engine.

---

## 5. Building Block View

The structural decomposition of the system is formally specified across three architectural levels:
- **Level 2 Container Architecture:** [`docs/architecture/c4-container_en.md`](c4-container_en.md) *(LOCKED)*
  - Decomposes the system into `React Web SPA`, `ASP.NET Core Backend API`, and `PostgreSQL Database`.
- **Level 3 Backend Component Architecture:** [`docs/architecture/c4-component-backend_en.md`](c4-component-backend_en.md) *(LOCKED)*
  - Decomposes the backend into Presentation Controllers, Application Services, Domain Rules & Fee Strategies, Persistence Ports, and EF Core Data Adapters.
- **Level 3 Frontend Component Architecture:** [`docs/architecture/c4-component-frontend_en.md`](c4-component-frontend_en.md) *(LOCKED)*
  - Decomposes the frontend into App & Routing, Pages Layer, Feature Components, Feature Hooks, Feature Services, and Shared Frontend Foundation.

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
- **Global Error Handling & API Boundary:** Centralized backend middleware converts unhandled domain exceptions into standardized `RFC 7807 ProblemDetails`. Controllers do not perform ad-hoc exception mapping.
- **English-Only UI Standards:** Centralized user-facing dictionary in `src/shared/constants/uiCopy.ts`.
- **Financial Typography & Color Tokens:** Dark neutral (`#0f172a`) for values, Crimson Red (`#c5221f`) strictly for expense deductions/losses, muted slate (`#64748b`) for zero amounts, and tabular figures (`tnum`) for alignment.
- **Dependency Inversion in Persistence:** Business domain logic defines repository interfaces (`IOrderRepository`, `IFeeScheduleRepository`, etc.); data layer implements them via EF Core.
- **Immutable Financial Snapshots:** Channel fees and net revenues are calculated and permanently frozen into immutable snapshot records upon order delivery.
- **Transactional Consistency:** Database operations modifying order state and financial records execute within atomic ACID transactions.

---

## 9. Architecture Decisions (ADR Summary)

Key architectural decisions consolidated from C4 Level 2, 3 (Backend), and 3 (Frontend) specifications:
- **ADR-01:** Decoupling of Presentation (SPA) from Business Logic (API) ([c4-container_en.md](c4-container_en.md)).
- **ADR-02:** PostgreSQL as the Single Persistent Transactional Source of Truth ([c4-container_en.md](c4-container_en.md)).
- **ADR-03:** Strict Tiered Invariant (`SPA -> API -> Database`) ([c4-container_en.md](c4-container_en.md)).
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
| **Projected Settlement** | The net payout expected from the sales channel or payment processor after deducting platform fees from gross sales (`gross_revenue - total_fees`). |
| **Actual Settlement** | The actual net funds deposited into the merchant's commercial bank account or marketplace wallet as recorded in bank statements. |
| **Variance** | The mathematical discrepancy between projected settlement and actual disbursement (`variance_amount = projected_settlement - actual_settlement`). |
| **Reconciliation** | The accounting verification workflow comparing internal financial projections against external settlement statements. |
| **Discrepancy** | An unresolved financial variance ($|\text{variance\_amount}| > 0$) requiring investigation, justification notes, and dispute filing. |
| **Net Realized Revenue** | Official recognized revenue retained by the business after all platform deductions have been settled. |
