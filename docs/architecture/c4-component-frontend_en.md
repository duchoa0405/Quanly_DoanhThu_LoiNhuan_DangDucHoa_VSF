# C4 Component Specification: React Web SPA

---

## 1. Target Component Scope & Boundary Definition

### 1.1. Architectural Scope
This document specifies the **C4 Level 3 Component Architecture** for the **React Web SPA** container within the **Fashion Revenue & Profit Management System**. The React Web SPA is decomposed into logical frontend component groups, formalizing clean separation between layout composition, page routing, business feature presentation, custom feature hooks, feature API services, and shared frontend foundation primitives:

1. **App & Routing:** Outer application frame, navigation rail, and route resolution.
2. **Pages (Composition & Orchestration):** Route-bound view orchestrators composing features without direct network access.
3. **Features (Business Capability UI):** Domain-bounded visual components and interactive modals.
4. **Feature Hooks:** Custom React hooks encapsulating server-state lifecycle and remote operations.
5. **Feature API Services:** Domain-specific API clients mediating HTTP transport.
6. **Shared Frontend Foundation:** Domain-agnostic UI primitives, localization dictionaries, formatting utilities, design tokens, and the shared base HTTP client.

```
+---------------------------------------------------------------------------------------------------+
| React Web SPA Container (Vite / React 18 / TypeScript)                                            |
|                                                                                                   |
|  [App & Routing]                  ApplicationShell (Sidebar, Topbar)                              |
|                                                  │                                                |
|                                                  ▼                                                |
|                                              AppRouter                                            |
|                                                  │                                                |
|  [Pages]                       OrdersPage | SettlementPage | RevenueDashboardPage                 |
|                                                  │                                                |
|  [Features]                    orders/    | settlements/   | discrepancies/  | analytics/         |
|                                       │                               │                           |
|                                       ▼                               ▼                           |
|  [Feature Hooks]               useOrders, useSettlement...    [Shared Frontend Foundation]        |
|                                       │                       (Shared UI Primitives, UICopy,      |
|                                       ▼                        FormattingUtils, DesignTokens,     |
|  [Feature API Services]        OrderService, Settlement...     ApiClient)                         |
|                                       │                                                           |
|                                       ▼                                                           |
|  [Shared Infrastructure]          ApiClient                                                       |
|                                       │                                                           |
+---------------------------------------┼-----------------------------------------------------------+
                                        │ HTTPS / REST / JSON
                                        ▼
                           [ASP.NET Core Backend API]
```

### 1.2. Strict Architectural Boundaries & Invariants
1. **Presentation & Interaction Boundary:** The SPA is strictly a presentation, interaction, and workflow orchestration layer. It possesses **zero authoritative financial calculation logic**. All canonical fee formulas, voucher deductions, net settlements, and discrepancy evaluations are computed exclusively by the `ASP.NET Core Backend API`.
2. **External Boundary & Database Isolation:** The frontend SPA communicates exclusively with the Backend API over HTTPS / REST / JSON. The frontend has **zero direct connectivity** to PostgreSQL and **zero direct integration** with third-party marketplace APIs (TikTok Shop, Shopee, or Bank APIs). All platform data ingestion and webhook processing are mediated authoritatively by the Backend API.
3. **Presentation-Only RBAC vs. Production Authenticated Session:**
   - **Persona Switcher:** Strictly a **prototype/demo role simulation aid** for evaluating role-specific UI visibility (`Sales/Ops`, `Finance Manager`, `Shop Owner`). It does not constitute actual authentication or security enforcement.
   - **Target Production Architecture:** Built upon `Authenticated User Session` $\rightarrow$ `RoleAwareNavigation`. Frontend RBAC strictly governs UI element visibility; actual authorization is authoritatively enforced by Backend API controller filters (`[Authorize]`).
4. **Single Shared HTTP Client:** All HTTP communications must route through the single shared `ApiClient` (`src/shared/api/ApiClient.ts`). Pages and feature components **must never call raw Axios directly**, and individual features are strictly prohibited from instantiating their own Axios instances.
5. **Feature-Bounded Target Directory Structure:** Domain services and hooks are co-located strictly within their respective feature directories to preserve modularity:
   ```
   src/features/
   ├── orders/
   │   ├── api/
   │   │   ├── OrderService.ts
   │   │   └── FeeService.ts
   │   └── hooks/
   │       ├── useOrders.ts
   │       └── useFeePreview.ts
   ├── settlements/
   │   ├── api/
   │   │   └── SettlementService.ts
   │   └── hooks/
   │       └── useSettlement.ts
   ├── discrepancies/
   │   ├── api/
   │   │   └── DiscrepancyService.ts
   │   └── hooks/
   │       └── useDiscrepancies.ts
   └── analytics/
       ├── api/
       │   └── AnalyticsService.ts
       └── hooks/
           └── useAnalytics.ts
   ```

---

## 2. Target Frontend Component Groups

The React Web SPA is decomposed into logical frontend component groups. These represent six clear architectural responsibilities rather than a single rigid linear stack:

### 2.1. Group 1: App & Routing
- **`ApplicationShell` (`src/layouts/AppShell.tsx`):** Master layout frame that hosts the global layout grid. It composes the navigation header (`Topbar`), collapsible navigation rail (`Sidebar`), active route viewport (`<Outlet />`), and global toast container. It owns the outer navigation layout; pages never own or re-instantiate the navigation shell.
  - **`Topbar` (`src/layouts/components/Topbar.tsx`):** Layout header subcomponent containing global search, breadcrumbs, and the `Persona Switcher` simulation aid.
  - **`Sidebar` (`src/layouts/components/Sidebar.tsx`):** Collapsible navigation rail subcomponent rendering primary navigation links filtered by `RoleAwareNavigation`.
- **`AppRouter` (`src/app/AppRouter.tsx`):** Declarative route-based viewport resolution via `react-router-dom`:
  - `/orders`: Mounts `OrdersPage` (SCR-01).
  - `/settlement`: Mounts `SettlementPage` (SCR-02), encapsulating fee ledger, reconciliation summary, and embedded discrepancy review.
  - `/analytics`: Mounts `RevenueDashboardPage` (SCR-03), executive cash flow and profit reporting.
  - `/`: Redirects immediately to `/orders`.
  - *Invariant:* `/discrepancies` is **not** a top-level route; discrepancy management is strictly an embedded sub-feature within `/settlement`.
- **`RoleAwareNavigation` (`src/layouts/RoleAwareNavigation.tsx`):** Evaluates user role permissions to conditionally render navigation links and action triggers. In prototype mode, it responds to the demo Persona Switcher; in production, it binds to the `Authenticated User Session`.

### 2.2. Group 2: Pages (Composition & Orchestration)
Page components reside in `src/pages/` and act strictly as high-level view orchestrators. A page composes feature components, filters, and dialogs for its bound route, but **never executes direct HTTP calls, never imports Axios, and never contains financial calculation formulas**:
- **Rule:** `Page = composition/orchestration`, `Feature = business capability UI`. A Page is never considered a Feature, and business capability components must never be placed in `src/pages/`.
- **`OrdersPage` (`src/pages/orders/OrdersPage.tsx` / SCR-01):** Assembles order metrics, channel filters, master orders table, and modal dialogs (`CreateOrderModal`, `CancelOrderModal`).
- **`SettlementPage` (`src/pages/settlement/SettlementPage.tsx` / SCR-02):** Assembles reconciliation summary cards, status filters, fee ledger table, period selector, fee schedule modal, and the embedded discrepancy review drawer (`DiscrepancyPanel`).
- **`RevenueDashboardPage` (`src/pages/analytics/RevenueDashboardPage.tsx` / SCR-03):** Assembles executive KPI cards, cash flow trend chart, channel share donut, top SKU table, and source order drilldown modal (`SourceOrderDrilldown`).

### 2.3. Group 3: Features (Business Capability UI)
Features reside in `src/features/` and are organized strictly by **business domain capability**:
- **`orders` (`src/features/orders/`):**
  - `OrderMetrics`: Operational volume counters and delivered gross revenue cards.
  - `OrderFilters`: Channel capsule pills and lifecycle status filters.
  - `OrderTable`: High-density master orders data grid with line item details and action buttons.
  - `OrderStatusActions`: Inline lifecycle action buttons (`[Ship]`, `[Mark as Delivered]`, `[Cancel]`).
  - `CreateOrderModal` (MOD-01): Multi-line order creation dialog with real-time backend fee preview.
  - `CancelOrderModal` (MOD-02): Cancellation confirmation dialog capturing mandatory reason notes.
- **`settlements` (`src/features/settlements/`):**
  - `SettlementSummary`: High-level reconciliation summary cards (Matched vs. Variance counters).
  - `SettlementFilters`: Reconciliation status filter tabs and settlement period dropdown selector.
  - `SettlementLedger`: Financial ledger displaying itemized platform fees, projected payout, actual payout, and variance.
  - `RecordSettlementModal` (MOD-03): Dialog capturing verified wallet payout figures and explanation notes.
  - `FeeScheduleModal` (MOD-04): Interface for configuring channel-specific fee percentage schedules.
- **`discrepancies` (`src/features/discrepancies/` - Settlement Sub-Feature):**
  - `DiscrepancyPanel`: Slide-over audit drawer embedded inside `SettlementPage`.
  - `DiscrepancyDetails` (internal subcomponent): Itemized comparison between projected settlement and marketplace adjustments.
  - `DiscrepancyReviewAction` (internal subcomponent): Resolution inputs for saving authorized explanation notes.
- **`analytics` (`src/features/analytics/`):**
  - `KpiCards`: Executive metric cards for Gross Revenue, Platform Fees, Net Realized Revenue, Delivered Count.
  - `RevenueTrendChart`: Grouped bar visualization comparing gross customer payment against net realized payout.
  - `ChannelShareChart`: Donut visualization detailing revenue contribution share by channel.
  - `TopSkuTable`: Leaderboard table ranking Top 5 revenue-generating SKUs.
  - `SourceOrderDrilldown` (MOD-05): Itemized modal listing constituent delivered orders with CSV export trigger.

### 2.4. Group 4: Feature Hooks
Feature hooks reside in `src/features/*/hooks/` and serve as the explicit intermediate orchestration layer between React components and API services. They manage asynchronous server-state lifecycle (fetching, caching, mutation state, loading, error handling):
- **`useOrders` (`src/features/orders/hooks/useOrders.ts`):** Manages order remote data queries, order creation mutations, status transitions, and cancellations (`useCancelOrder`) via `OrderService`.
- **`useFeePreview` (`src/features/orders/hooks/useFeePreview.ts`):** Manages real-time backend fee calculation requests and fee schedule queries via `FeeService`.
- **`useSettlement` (`src/features/settlements/hooks/useSettlement.ts`):** Manages settlement ledger queries, statement imports, and wallet payout submission mutations via `SettlementService`.
- **`useDiscrepancies` (`src/features/discrepancies/hooks/useDiscrepancies.ts`):** Manages discrepancy audit queries and review note persistence via `DiscrepancyService`.
- **`useAnalytics` (`src/features/analytics/hooks/useAnalytics.ts`):** Manages executive KPI aggregations, cash flow chart data, channel share distributions, and CSV export triggers via `AnalyticsService`.
- *Server-State Policy:* The server-state library is an implementation decision, not yet fixed. Custom hooks provide a clean abstraction boundary, allowing future adoption of TanStack Query or SWR without altering presentation components.

### 2.5. Group 5: Feature API Services
Feature services reside in `src/features/*/api/` and encapsulate domain-specific HTTP endpoints, request payloads, and response DTO mappings. They depend strictly on `ApiClient` and **never depend on React UI components, custom hooks, or shared UI primitives**:
- **`OrderService` (`src/features/orders/api/OrderService.ts`):** Executes HTTP calls for order queries, creation, and lifecycle status transitions.
- **`FeeService` (`src/features/orders/api/FeeService.ts`):** Executes HTTP calls for real-time backend fee calculation previews and channel fee schedule configurations.
- **`SettlementService` (`src/features/settlements/api/SettlementService.ts`):** Executes HTTP calls for settlement ledger queries, payout verification, and statement imports.
- **`DiscrepancyService` (`src/features/discrepancies/api/DiscrepancyService.ts`):** Executes HTTP calls for discrepancy audit records and resolution notes.
- **`AnalyticsService` (`src/features/analytics/api/AnalyticsService.ts`):** Executes HTTP calls for executive revenue KPIs, trends, channel share, and CSV data export.

### 2.6. Group 6: Shared Frontend Foundation
The Shared Frontend Foundation runs in parallel with features and data access. It is completely domain-agnostic and provides universal UI primitives, localization dictionaries, formatting utilities, design tokens, and base network infrastructure:
- **`SharedUI` (`src/shared/ui/`):** Domain-agnostic UI primitives (`Button`, `Input`, `Select`, `Modal`, `Table`, `Badge`, `Card`, `Toast`, `EmptyState`, `ConfirmDialog`). Must possess **zero knowledge of business entities** (`Order`, `Settlement`, `Commission`, `TikTok`, `Shopee`). If a component knows `OrderStatus`, it belongs to `features/orders/`, not `shared/ui/`.
- **`UICopy` (`src/shared/constants/uiCopy.ts`):** Centralized dictionary of English-only UI strings and terminology enforcing the **P03.5 English-Only Policy**.
- **`FormattingUtils` (`src/shared/lib/formatters.ts`):** Pure formatting functions standardizing Vietnamese commercial locale under English UI:
  - Currency: `184,500,000 ₫` (`formatCurrency`).
  - Alphanumeric Date: `17 Sep 2026` (`formatDate`).
  - Datetime: `17 Sep 2026, 14:30` (`formatDateTime`).
  - Timezone: `Asia/Ho_Chi_Minh` (UTC+7).
  - Percentage: `15.5%` (`formatPercentage`).
- **`DesignTokens` (`src/styles/tokens.css`):** CSS custom properties enforcing corporate financial color and typography rules:
  - Uniform Dark Neutral (`#0f172a`): Standard numbers, gross revenue, net payout, and volume counts.
  - Crimson Red (`#c5221f`): Strictly reserved for active expenses, deductions, and negative variances where the shop loses money.
  - Neutral Muted Slate (`#64748b`): Applied to zero values (`0 ₫` / `0`), indicating zero incurred cost.
  - Tabular Figures (`tnum`): Enforced across all financial columns for exact vertical alignment.
- **`ApiClient` (`src/shared/api/ApiClient.ts`):** Universal HTTP client instance wrapping Axios with base URL, standard JSON headers, authentication token interceptors, and normalized error handling (`ApiError`).

---

## 3. Component Catalog

The React Web SPA is decomposed into logical frontend component groups. Only architecturally significant components are cataloged below:

| Component | Responsibility | Target Location | Depends On |
|---|---|---|---|
| **`ApplicationShell`** | Master outer layout grid (Sidebar rail, Topbar header, Omnisearch, Toast container). | `src/layouts/AppShell.tsx` | `Topbar`, `Sidebar`, `AppRouter`, `tokens.css` |
| **`Topbar`** | Header layout subcomponent composing breadcrumbs, search, and prototype Persona Switcher. | `src/layouts/components/Topbar.tsx` | `uiCopy`, `RoleAwareNavigation` |
| **`Sidebar`** | Collapsible navigation rail subcomponent rendering filtered navigation links. | `src/layouts/components/Sidebar.tsx` | `uiCopy`, `RoleAwareNavigation` |
| **`AppRouter`** | Route resolution for `/orders`, `/settlement`, `/analytics` via `react-router-dom`. | `src/app/AppRouter.tsx` | `OrdersPage`, `SettlementPage`, `RevenueDashboardPage` |
| **`RoleAwareNavigation`** | Filters navigation links and action buttons based on active persona / user session. | `src/layouts/RoleAwareNavigation.tsx` | `uiCopy`, user session / persona context |
| **`OrdersPage`** | Page orchestrator for Screen 1: Orders Management (SCR-01). Zero direct HTTP. | `src/pages/orders/OrdersPage.tsx` | `OrderMetrics`, `OrderFilters`, `OrderTable`, `CreateOrderModal`, `CancelOrderModal` |
| **`SettlementPage`** | Page orchestrator for Screen 2: Fees & Settlement (SCR-02) and embedded Discrepancy drawer. | `src/pages/settlement/SettlementPage.tsx` | `SettlementSummary`, `SettlementFilters`, `SettlementLedger`, `RecordSettlementModal`, `FeeScheduleModal`, `DiscrepancyPanel` |
| **`RevenueDashboardPage`** | Page orchestrator for Screen 3: Revenue Dashboard (SCR-03) and drilldown modal. | `src/pages/analytics/RevenueDashboardPage.tsx` | `KpiCards`, `RevenueTrendChart`, `ChannelShareChart`, `TopSkuTable`, `SourceOrderDrilldown` |
| **`OrderMetrics`** | Displays operational volume counters and delivered gross revenue cards. | `src/features/orders/components/OrderMetrics.tsx` | `Card`, `formatters`, `useOrders` |
| **`OrderFilters`** | Capsule filter pills for sales channels and order lifecycle statuses. | `src/features/orders/components/OrderFilters.tsx` | `Badge`, `uiCopy` |
| **`OrderTable`** | Master orders data grid with line item details and action triggers. | `src/features/orders/components/OrderTable.tsx` | `Table`, `Badge`, `OrderStatusActions`, `formatters`, `useOrders` |
| **`OrderStatusActions`** | Inline lifecycle action triggers (`[Ship]`, `[Mark as Delivered]`, `[Cancel]`). | `src/features/orders/components/OrderStatusActions.tsx` | `Button`, `useOrders` |
| **`CreateOrderModal`** | Multi-line order creation dialog with real-time backend fee preview (MOD-01). | `src/features/orders/components/CreateOrderModal.tsx` | `Modal`, `Input`, `Select`, `Button`, `useFeePreview`, `formatters` |
| **`CancelOrderModal`** | Confirmation dialog capturing cancellation reason and note (MOD-02). | `src/features/orders/components/CancelOrderModal.tsx` | `Modal`, `Select`, `Input`, `Button`, `useOrders` |
| **`SettlementSummary`** | High-level reconciliation summary cards (Matched vs. Variance counters). | `src/features/settlements/components/SettlementSummary.tsx` | `Card`, `formatters`, `useSettlement` |
| **`SettlementFilters`** | Status filter tabs and settlement period dropdown selector. | `src/features/settlements/components/SettlementFilters.tsx` | `Select`, `uiCopy` |
| **`SettlementLedger`** | Fee deduction audit table with itemized commission, payment, and service fees. | `src/features/settlements/components/SettlementLedger.tsx` | `Table`, `Badge`, `formatters`, `useSettlement` |
| **`RecordSettlementModal`** | Modal capturing verified wallet payout figures and explanation notes (MOD-03). | `src/features/settlements/components/RecordSettlementModal.tsx` | `Modal`, `Input`, `Button`, `useSettlement`, `formatters` |
| **`FeeScheduleModal`** | Form for configuring channel-specific fee percentage schedules (MOD-04). | `src/features/settlements/components/FeeScheduleModal.tsx` | `Modal`, `Table`, `Input`, `Button`, `useFeePreview`, `formatters` |
| **`DiscrepancyPanel`** | Slide-over drawer presenting audit discrepancies and resolution actions. | `src/features/discrepancies/components/DiscrepancyPanel.tsx` | `Modal`, `DiscrepancyDetails`, `DiscrepancyReviewAction`, `useDiscrepancies` |
| **`DiscrepancyDetails`** | Subcomponent rendering itemized comparison between projected and actual values. | `src/features/discrepancies/components/DiscrepancyDetails.tsx` | `Table`, `Badge`, `formatters`, `useDiscrepancies` |
| **`DiscrepancyReviewAction`** | Subcomponent capturing resolution explanation notes and save action. | `src/features/discrepancies/components/DiscrepancyReviewAction.tsx` | `Input`, `Button`, `useDiscrepancies` |
| **`KpiCards`** | Executive cards for Gross Revenue, Platform Fees, Net Realized Revenue, Delivered Count. | `src/features/analytics/components/KpiCards.tsx` | `Card`, `formatters`, `useAnalytics`, `SourceOrderDrilldown` |
| **`RevenueTrendChart`** | Grouped bar visualization comparing gross customer payment vs. net realized payout. | `src/features/analytics/components/RevenueTrendChart.tsx` | Pure SVG primitives, `formatters`, `useAnalytics` |
| **`ChannelShareChart`** | Interactive donut chart detailing revenue contribution share by channel. | `src/features/analytics/components/ChannelShareChart.tsx` | Pure SVG primitives, `formatters`, `useAnalytics` |
| **`TopSkuTable`** | Leaderboard table ranking Top 5 revenue-generating SKUs. | `src/features/analytics/components/TopSkuTable.tsx` | `Table`, `formatters`, `useAnalytics` |
| **`SourceOrderDrilldown`** | Modal listing constituent delivered orders with CSV export trigger (MOD-05). | `src/features/analytics/components/SourceOrderDrilldown.tsx` | `Modal`, `Table`, `Button`, `formatters`, `useAnalytics` |
| **`useOrders`** | Feature hook managing order queries, mutations, and status transitions. | `src/features/orders/hooks/useOrders.ts` | `OrderService` |
| **`useFeePreview`** | Feature hook managing real-time fee calculation preview and schedules. | `src/features/orders/hooks/useFeePreview.ts` | `FeeService` |
| **`useSettlement`** | Feature hook managing settlement queries, payout records, and statement imports. | `src/features/settlements/hooks/useSettlement.ts` | `SettlementService` |
| **`useDiscrepancies`** | Feature hook managing discrepancy audit queue and resolution note submission. | `src/features/discrepancies/hooks/useDiscrepancies.ts` | `DiscrepancyService` |
| **`useAnalytics`** | Feature hook managing executive KPI aggregations, cash flow charts, and CSV download. | `src/features/analytics/hooks/useAnalytics.ts` | `AnalyticsService` |
| **`OrderService`** | Feature service executing order queries, creation, and status transitions. | `src/features/orders/api/OrderService.ts` | `ApiClient`, `OrderDto` contracts |
| **`FeeService`** | Feature service requesting real-time fee preview calculations and fee schedules. | `src/features/orders/api/FeeService.ts` | `ApiClient`, `FeeBreakdownDto` contracts |
| **`SettlementService`** | Feature service executing ledger queries, settlement records, and statement imports. | `src/features/settlements/api/SettlementService.ts` | `ApiClient`, `SettlementDto` contracts |
| **`DiscrepancyService`** | Feature service querying discrepancy audits and persisting review notes. | `src/features/discrepancies/api/DiscrepancyService.ts` | `ApiClient`, `DiscrepancyDto` contracts |
| **`AnalyticsService`** | Feature service retrieving executive KPIs, trends, channel share, and CSV exports. | `src/features/analytics/api/AnalyticsService.ts` | `ApiClient`, `AnalyticsDto` contracts |
| **`ApiClient`** | Shared HTTP client instance wrapping Axios with headers, interceptors, and error handling. | `src/shared/api/ApiClient.ts` | `axios`, `ApiError` interface |
| **`SharedUI`** | Domain-agnostic UI primitives (`Button`, `Input`, `Modal`, `Table`, `Badge`, `Card`, `Toast`, `EmptyState`). | `src/shared/ui/` | *None* (Zero domain dependencies) |
| **`UICopy`** | Centralized English-only string tokens for all user-facing copy (P03.5). | `src/shared/constants/uiCopy.ts` | *None* |
| **`FormattingUtils`** | Pure formatting functions for VND currency (`184,500,000 ₫`), English dates, rates. | `src/shared/lib/formatters.ts` | Standard `Intl` API |
| **`DesignTokens`** | CSS custom properties enforcing corporate financial color and typography rules. | `src/styles/tokens.css` | *None* |

---

## 4. Target Frontend Component Diagram

The C4 Level 3 diagram below illustrates the internal component architecture of the `React Web SPA` and its clean unidirectional flow into the external `ASP.NET Core Backend API`:

```mermaid
C4Component
    title Component Diagram for React Web SPA (C4 Level 3 - Target Architecture)

    Container_Boundary(spa, "React Web SPA (Vite / React 18 / TypeScript)") {
        
        Boundary(app_boundary, "1. App & Routing") {
            Component(shell, "Application Shell", "React Layout", "Master outer frame composing Topbar header, Sidebar rail, and router viewport.")
            Component(topbar, "Topbar", "React Subcomponent", "Layout header with search, breadcrumbs, and demo Persona Switcher.")
            Component(sidebar, "Sidebar", "React Subcomponent", "Collapsible navigation rail with role-filtered links.")
            Component(router, "App Router", "React Router DOM", "Route resolution for /orders, /settlement, and /analytics.")
            Component(rbac, "Role-Aware Navigation", "React / Context", "Filters navigation and action triggers by active persona context.")
        }

        Boundary(pages_boundary, "2. Pages (Composition & Orchestration)") {
            Component(orders_page, "OrdersPage", "React Page (src/pages/orders/)", "Composes SCR-01 order metrics, filters, table, and modals.")
            Component(settle_page, "SettlementPage", "React Page (src/pages/settlement/)", "Composes SCR-02 fee ledger, reconciliation summary, and discrepancy drawer.")
            Component(dash_page, "RevenueDashboardPage", "React Page (src/pages/analytics/)", "Composes SCR-03 executive KPIs, trend chart, channel donut, and drilldown.")
        }

        Boundary(features_boundary, "3. Features (Business Capability UI)") {
            Component(orders_feat, "Orders Feature", "React Feature", "OrderMetrics, OrderFilters, OrderTable, OrderStatusActions, CreateOrderModal, CancelOrderModal.")
            Component(settle_feat, "Settlement Feature", "React Feature", "SettlementSummary, SettlementFilters, SettlementLedger, RecordSettlementModal, FeeScheduleModal.")
            Component(disc_feat, "Discrepancy Feature", "React Sub-Feature", "DiscrepancyPanel, DiscrepancyDetails, DiscrepancyReviewAction.")
            Component(analytics_feat, "Analytics Feature", "React Feature", "KpiCards, RevenueTrendChart, ChannelShareChart, TopSkuTable, SourceOrderDrilldown.")
        }

        Boundary(hooks_boundary, "4. Feature Hooks") {
            Component(orders_hooks, "Orders Hooks", "useOrders, useFeePreview", "Manages order remote data, mutations, and fee preview calculations.")
            Component(settle_hooks, "Settlement Hooks", "useSettlement", "Manages settlement ledger queries and payout recording mutations.")
            Component(disc_hooks, "Discrepancy Hooks", "useDiscrepancies", "Manages discrepancy audit queries and review note persistence.")
            Component(analytics_hooks, "Analytics Hooks", "useAnalytics", "Manages executive KPI aggregations, charts, and CSV export.")
        }

        Boundary(services_boundary, "5. Feature API Services") {
            Component(order_srv, "OrderService & FeeService", "Domain API Clients", "Executes HTTP requests for orders and fee calculation preview.")
            Component(settle_srv, "SettlementService", "Domain API Client", "Executes HTTP requests for settlement records and statement imports.")
            Component(disc_srv, "DiscrepancyService", "Domain API Client", "Executes HTTP requests for discrepancy audits and notes.")
            Component(analytics_srv, "AnalyticsService", "Domain API Client", "Executes HTTP requests for KPIs, trend series, and CSV export.")
        }

        Boundary(shared_boundary, "6. Shared Frontend Foundation") {
            Component(api_client, "ApiClient", "Shared Axios Client", "Single base HTTP client: baseURL, interceptors, error normalization.")
            Component(shared_ui, "Shared UI Primitives", "Domain-Agnostic UI", "Button, Input, Select, Modal, Table, Badge, Card, Toast, EmptyState.")
            Component(ui_copy, "UI Copy Dictionary", "TypeScript Constants", "Centralized English-only string tokens (P03.5).")
            Component(formatters, "Formatting Utilities", "TypeScript Utilities", "VND currency (184,500,000 ₫), English dates, Asia/Ho_Chi_Minh timezone.")
            Component(tokens, "Design System Tokens", "CSS Variables", "Financial tokens: Dark Neutral #0f172a, Crimson Red #c5221f, Slate #64748b.")
        }
    }

    Container_Ext(backend_api, "ASP.NET Core Backend API", "External C4 Container", "Authoritative backend computing fee math, lifecycle validation, and persistence.")

    %% Shell composition
    Rel(shell, topbar, "Composes")
    Rel(shell, sidebar, "Composes")
    Rel(topbar, rbac, "Evaluates persona visibility")
    Rel(sidebar, rbac, "Filters navigation links")

    %% Shell -> Router -> Pages -> Features
    Rel(shell, router, "Mounts and hosts")
    Rel(router, orders_page, "Renders on /orders")
    Rel(router, settle_page, "Renders on /settlement")
    Rel(router, dash_page, "Renders on /analytics")

    Rel(orders_page, orders_feat, "Composes")
    Rel(settle_page, settle_feat, "Composes")
    Rel(settle_page, disc_feat, "Embeds as drawer")
    Rel(dash_page, analytics_feat, "Composes")

    %% Features -> Feature Hooks
    Rel(orders_feat, orders_hooks, "Invokes")
    Rel(settle_feat, settle_hooks, "Invokes")
    Rel(disc_feat, disc_hooks, "Invokes")
    Rel(analytics_feat, analytics_hooks, "Invokes")

    %% Feature Hooks -> Feature API Services
    Rel(orders_hooks, order_srv, "Calls domain methods")
    Rel(settle_hooks, settle_srv, "Calls domain methods")
    Rel(disc_hooks, disc_srv, "Calls domain methods")
    Rel(analytics_hooks, analytics_srv, "Calls domain methods")

    %% Feature API Services -> ApiClient
    Rel(order_srv, api_client, "Dispatches HTTP requests")
    Rel(settle_srv, api_client, "Dispatches HTTP requests")
    Rel(disc_srv, api_client, "Dispatches HTTP requests")
    Rel(analytics_srv, api_client, "Dispatches HTTP requests")

    %% ApiClient -> External Backend API
    Rel(api_client, backend_api, "HTTPS / REST / JSON", "HTTPS/JSON")

    %% Features -> Shared Frontend Foundation (Parallel Layer)
    Rel(orders_feat, shared_ui, "Renders primitives")
    Rel(settle_feat, shared_ui, "Renders primitives")
    Rel(disc_feat, shared_ui, "Renders primitives")
    Rel(analytics_feat, shared_ui, "Renders primitives")

    Rel(orders_feat, formatters, "Formats currency & dates")
    Rel(settle_feat, formatters, "Formats currency & dates")
    Rel(analytics_feat, formatters, "Formats currency & dates")

    Rel(orders_feat, ui_copy, "Imports canonical copy")
    Rel(settle_feat, ui_copy, "Imports canonical copy")
    Rel(analytics_feat, ui_copy, "Imports canonical copy")
```

---

## 5. State Management & Unidirectional Dependency Rules

### 5.1. Unidirectional Dependency Invariants
```
ApplicationShell
      │
      ▼
  AppRouter
      │
      ▼
    Pages
      │
      ▼
   Features
   ├───► Feature Hooks
   │          │
   │          ▼
   │     Feature Services
   │          │
   │          ▼
   │      ApiClient
   │          │
   │          ▼
   │     Backend API
   │
   └───► Shared Frontend Foundation
         (Shared UI Primitives, UICopy, Formatters, Tokens)
```

1. **Outer Shell to Inner Features (`ApplicationShell` $\rightarrow$ `AppRouter` $\rightarrow$ `Pages` $\rightarrow$ `Features`):**
   - The `ApplicationShell` is the outer layout frame composing `Topbar` and `Sidebar`.
   - `ApplicationShell` hosts `AppRouter`, which resolves route URLs (`/orders`, `/settlement`, `/analytics`) to `Pages`.
   - `Pages` act strictly as route orchestrators and compose domain `Features`.
   - **Strict prohibition:** Pages never own the navigation shell; features never import pages; and relationships like `Pages → AppShell` are strictly forbidden.
2. **Strict Component-to-Service Flow via Hooks:**
   $$\text{Feature Component} \longrightarrow \text{Feature Hook} \longrightarrow \text{Feature Service} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
   - Feature components must **never call feature services directly**.
   - Feature components must **never call `ApiClient` or raw Axios directly**.
   - No individual feature may create its own Axios instance.
   - Specific flow examples:
     - **Create Order & Fee Preview:**
       $$\text{CreateOrderModal} \longrightarrow \text{useFeePreview} \longrightarrow \text{FeeService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Cancel Order:**
       $$\text{CancelOrderModal} \longrightarrow \text{useOrders} \longrightarrow \text{OrderService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Order Lifecycle Status Transitions:**
       $$\text{OrderStatusActions} \longrightarrow \text{useOrders} \longrightarrow \text{OrderService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Settlement Record Submission:**
       $$\text{RecordSettlementModal} \longrightarrow \text{useSettlement} \longrightarrow \text{SettlementService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Fee Schedule Configuration:**
       $$\text{FeeScheduleModal} \longrightarrow \text{useFeePreview} \longrightarrow \text{FeeService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Discrepancy Review & Resolution Notes:**
       $$\text{DiscrepancyPanel / DiscrepancyReviewAction} \longrightarrow \text{useDiscrepancies} \longrightarrow \text{DiscrepancyService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
     - **Analytics KPIs, Charts & Drilldown:**
       $$\text{KpiCards / Charts / SourceOrderDrilldown} \longrightarrow \text{useAnalytics} \longrightarrow \text{AnalyticsService} \longrightarrow \text{ApiClient} \longrightarrow \text{Backend API}$$
3. **Canonical Fee Calculation Authority:**
   The frontend has **zero canonical fee formulas**. Fee Preview is an interactive backend estimation flow:
   $$\text{CreateOrderModal} \xrightarrow{\text{input items}} \text{useFeePreview} \xrightarrow{\text{previewFee()}} \text{FeeService} \xrightarrow{\text{POST /api/fees/preview}} \text{Backend Strategy Engine}$$
   The modal renders whatever breakdown is computed and returned by the backend.
4. **Shared Frontend Foundation Independence:**
   - Primitives in `src/shared/ui/` (`Button`, `Input`, `Modal`, `Table`, `Badge`, `Card`, `Toast`, `EmptyState`) must remain completely domain-agnostic and possess zero knowledge of business concepts (`Order`, `Settlement`, `Commission`, `TikTok`, `Shopee`, or `OrderStatus`).
   - If a component knows `OrderStatus`, it must be placed in `features/orders/`, not `shared/ui/`.
   - **No reverse dependencies:** `Shared Foundation` never imports from `Features`, and `Feature Services` never import `SharedUI` primitives.
5. **No Circular Feature Coupling:**
   - Features must never import deep internal components or private state from other features (e.g., `analytics` never imports `OrderTable` or private state from `orders`).
   - Cross-domain interactions (such as the analytics order drilldown modal) consume shared public contracts (`src/shared/types/`) or public feature contracts.

### 5.2. State Management Strategy: Server State vs. UI State
- **Server State (Remote Async Business Data):** Master orders, fee schedules, settlement ledgers, discrepancy queues, and KPI aggregates.
  - Server state is acquired and mutated exclusively through co-located Feature Hooks (`useOrders`, `useFeePreview`, `useSettlement`, `useDiscrepancies`, `useAnalytics`) backed by feature services.
  - *Implementation Decision:* The server-state management library (e.g., TanStack Query, SWR, or lightweight React hooks) is an implementation detail and is not yet fixed. The architecture abstracts network state behind custom hooks, allowing future caching libraries to be introduced without modifying presentation components.
  - Hardcoded sample arrays (`sampleOrders`) are strictly prototype fixtures and do not form part of the target architecture.
- **UI State (Local Ephemeral State):** Modal open/closed state, active tabs, filter capsule selections, temporary form inputs, and sort toggles. Managed via standard React `useState` / `useReducer` within the component scope.

---

## 6. Existing Prototype Mapping

> **Reference Disclaimer:** The table below documents the delta between the existing initial prototype (`frontend/src/`) and the target architecture. This mapping serves as a roadmap for subsequent implementation phases. Architectural evaluation must assess the target design on its own structural merits and not penalize the specification for prototype code currently lacking real routers, hooks, or backend services.

| Architectural Concern | Existing Prototype (`frontend/src/`) | Target Architecture State | Implementation Roadmap Action |
|---|---|---|---|
| **Routing & Shell Layout** | `AppShell.tsx` manages `useState('orders')` tab switching; outer layout mixed with views. | `ApplicationShell` hosts `AppRouter` via `react-router-dom` (`/orders`, `/settlement`, `/analytics`). | Implement `src/app/AppRouter.tsx`; replace conditional tab rendering with `<Outlet />`. |
| **Page Layer Separation** | Pages mixed directly inside feature directories. | Dedicated orchestrators in `src/pages/` (`src/pages/orders/OrdersPage.tsx`, etc.). | Move route orchestrators to `src/pages/`; separate page composition from capability UI. |
| **Discrepancy View Location** | Standalone top-level tab (`currentTab === 'discrepancies'`). | Embedded audit drawer (`DiscrepancyPanel`) inside `SettlementPage` (SCR-02). | Remove top-level `/discrepancies` tab; mount `DiscrepancyPanel` inside `SettlementPage`. |
| **Data Access & Services** | Hardcoded in-memory arrays (`sampleOrders`) inline in components. | Co-located feature services (`features/*/api/`) backed by shared `ApiClient`. | Create `src/shared/api/ApiClient.ts` and domain services; eliminate inline sample data arrays. |
| **Feature Hooks** | State logic mixed directly inside JSX presentation components. | Dedicated hooks (`useOrders`, `useFeePreview`, `useSettlement`, `useDiscrepancies`, `useAnalytics`). | Extract asynchronous operations and state management into co-located `hooks/`. |
| **Fee Calculation** | Inline JavaScript formulas inside modal component inputs. | Canonical backend fee preview via `POST /api/fees/preview`. | Connect `CreateOrderModal` $\rightarrow$ `useFeePreview` $\rightarrow$ `FeeService` $\rightarrow$ `ApiClient`. |
| **UI Copy & Localization** | Hardcoded JSX literals with mixed English/Vietnamese strings. | 100% English-only string tokens centralized in `uiCopy.ts` (P03.5). | Consolidate all user-facing copy into `src/shared/constants/uiCopy.ts`. |
| **Financial Color Standards** | Ad-hoc CSS classes with red figures on zero amounts. | Strict tokens: `#0f172a` for values, `#c5221f` only for real loss, `#64748b` for zero. | Enforce `tokens.css` with `.negative` and `.zero` classes across all financial tables. |

---

## 7. Architectural Decision Records (ADRs)

- **ADR-FE-01: Feature-Oriented Structure with Co-located Services & Hooks:** Organize code by business capability (`features/orders/`, `features/settlements/`, `features/discrepancies/`, `features/analytics/`). Co-locate domain services in `features/*/api/` and hooks in `features/*/hooks/`, reserving `shared/api/` strictly for the generic `ApiClient`.
- **ADR-FE-02: Route-Based Navigation with Embedded Discrepancy Drawer:** Replace prototype `useState` view switching with canonical routes (`/orders`, `/settlement`, `/analytics`) using `react-router-dom`. Discrepancy management is an embedded audit drawer inside `/settlement`, not an independent top-level route.
- **ADR-FE-03: Backend Authority for Financial Calculations & Fee Preview:** The frontend possesses zero authoritative fee math. All fee calculations, net settlements, and discrepancy evaluations originate from the Backend API Strategy Pattern engine. `CreateOrderModal` requests previews through `useFeePreview -> FeeService -> ApiClient -> Backend API`.
- **ADR-FE-04: Explicit Feature Hooks Architecture:** Introduce custom feature hooks as the mandatory bridge between UI components and domain API services. Feature components invoke hooks; hooks invoke services; services invoke `ApiClient`. No direct component-to-service calls.
- **ADR-FE-05: Domain-Agnostic Shared Frontend Foundation:** Components in `src/shared/ui/` must remain completely decoupled from business domain models (`Order`, `Settlement`, `Commission`). Shared Foundation runs in parallel with features; services never depend on shared UI.
- **ADR-FE-06: Strict English-Only UI Copy Policy:** All user-facing text is written exclusively in English and centralized in `shared/constants/uiCopy.ts` in compliance with P03.5.
- **ADR-FE-07: Vietnamese Commercial Locale Under English UI:** Standardize formatting on VND currency (`184,500,000 ₫`), alphanumeric dates (`17 Sep 2026`), and `Asia/Ho_Chi_Minh` timezone via `FormattingUtils`. Individual features are strictly prohibited from custom ad-hoc formatting.
- **ADR-FE-08: Prototype Persona Simulation vs. Production Authenticated Session:** The Topbar Persona Switcher is strictly a demo simulation aid. In production, role-based visibility is driven by `Authenticated User Session -> RoleAwareNavigation`. Real operational authorization is enforced authoritatively on the Backend API.
- **ADR-FE-09: Decoupled Server-State Management Strategy:** Encapsulate remote data fetching inside custom hooks. TanStack Query or SWR adoption is deferred as an implementation decision; the architecture maintains clean boundaries regardless of the caching library selected.
- **ADR-FE-10: Single-Backend Gateway & Strict Isolation:** The frontend communicates exclusively with the Backend API via `ApiClient`. It has zero direct connectivity to PostgreSQL, TikTok Shop APIs, Shopee APIs, or Bank APIs.

---

## 8. Traceability Matrix: Requirements & UI/UX $\rightarrow$ Frontend Architecture

| Requirement / UI/UX Spec | Page Orchestrator | Feature Component | Feature Hook | Feature API Service | Shared Foundation & Invariants |
|---|---|---|---|---|---|
| **SCR-01: Orders Management** | `OrdersPage` (`src/pages/orders/`) | `OrderTable`, `OrderMetrics`, `OrderFilters`, `OrderStatusActions` | `useOrders` | `OrderService` | `Table`, `Badge`, `formatters.ts`, `uiCopy.ts` |
| **SCR-02: Fees & Settlement** | `SettlementPage` (`src/pages/settlement/`) | `SettlementLedger`, `SettlementSummary`, `SettlementFilters` | `useSettlement` | `SettlementService` | `Table`, `Badge`, `formatters.ts`, `uiCopy.ts` |
| **SCR-02 Discrepancy Review** | `SettlementPage` (`src/pages/settlement/`) | `DiscrepancyPanel`, `DiscrepancyDetails`, `DiscrepancyReviewAction` | `useDiscrepancies` | `DiscrepancyService` | `Modal`, `Table`, `Button`, `uiCopy.ts`, `formatters.ts` |
| **SCR-03: Revenue Dashboard** | `RevenueDashboardPage` (`src/pages/analytics/`) | `KpiCards`, `RevenueTrendChart`, `ChannelShareChart`, `TopSkuTable` | `useAnalytics` | `AnalyticsService` | `Card`, `formatters.ts`, `uiCopy.ts`, `tokens.css` |
| **MOD-01: Create Order Modal** | `OrdersPage` | `CreateOrderModal` | `useFeePreview` | `FeeService` | `Modal`, `Input`, `Select`, `Button`, `formatters.ts` |
| **MOD-02: Cancel Order Modal** | `OrdersPage` | `CancelOrderModal` | `useOrders` (`useCancelOrder`) | `OrderService` | `Modal`, `Select`, `Input`, `Button`, `uiCopy.ts` |
| **MOD-03: Record Settlement Modal**| `SettlementPage` | `RecordSettlementModal` | `useSettlement` | `SettlementService` | `Modal`, `Input`, `Button`, `formatters.ts` |
| **MOD-04: Fee Schedule Modal** | `SettlementPage` | `FeeScheduleModal` | `useFeePreview` | `FeeService` | `Modal`, `Table`, `Input`, `Button`, `formatters.ts` |
| **MOD-05: Source Order Drilldown** | `RevenueDashboardPage` | `SourceOrderDrilldown` | `useAnalytics` | `AnalyticsService` | `Modal`, `Table`, `Button`, `formatters.ts` |
| **Persona Role Simulation** | `ApplicationShell` | `Topbar` (Persona Switcher) | Demo Persona Context | — | `uiCopy.ts` (Visibility simulation only; target production: Authenticated User Session) |
| **P03.5 English-Only Copy** | All Pages | All Features | — | — | `uiCopy.ts` (Centralized English terminology, ADR-FE-06) |
| **P03.5 Commercial Locale (VND)** | All Pages | All Features | — | — | `formatters.ts` (`formatCurrency` VND, `Asia/Ho_Chi_Minh`, alphanumeric dates) |
| **P03.5 Financial Colors** | All Pages | All Features | — | — | `tokens.css` (`#0f172a` values, `#c5221f` losses, `#64748b` zero amounts) |
| **P01 Anti-Phantom Revenue** | `RevenueDashboardPage` | `KpiCards`, `RevenueTrendChart` | `useAnalytics` | `AnalyticsService` | Authoritative `DELIVERED` status recognition enforced by Backend API |
