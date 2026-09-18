# 07 — API Traceability Matrix: End-to-End Architectural Traceability

> **System:** Fashion Revenue & Profit Management System  
> **Target Framework:** ASP.NET Core 8 Web API / React 18+ Single Page Application  
> **API Specification Standard:** OpenAPI 3.0.3 ([`docs/api/openapi.yaml`](./openapi.yaml))  
> **Relational Persistence Standard:** PostgreSQL 16+ ([`schema.dbml`](../database/schema.dbml), [`database-constraints-indexes_en.md`](../database/database-constraints-indexes_en.md))  
> **Document Status:** Authoritative API Traceability Specification (Phase P06)

---

## 1. Scope & Architectural Traceability Methodology

This document establishes the **100% authoritative end-to-end traceability matrix** connecting business requirements, use cases, frontend UI components, API routes, ASP.NET Core backend controllers, application services, domain strategy engines, repository ports, and the 9 canonical PostgreSQL database tables.

```
[UI Trigger / Modal / Table]
       │
       ▼ (HTTPS REST / JSON camelCase)
[ASP.NET Core Controller Action] (`FashionWeb.Api.Controllers`)
       │
       ▼ (Dependency Inversion Interface Invocation)
[Application Business Service] (`FashionWeb.Application.Services`)
       │
       ├─► [Dynamic Fee Engine / Strategy Pattern] (`FashionWeb.Domain.Strategies`)
       │
       ▼ (Repository Port Interface)
[Persistence Adapter / EF Core Repository] (`FashionWeb.Infrastructure.Repositories`)
       │
       ▼ (Npgsql / PostgreSQL 16+)
[Canonical Relational Tables] (9 P05 Database Tables)
       │
       ▼ (Invariant Verification)
[Mathematical Financial Rules & RBAC Guard Policies]
```

### Core Financial Rules & System Invariants:
1. **Zero Phantom Revenue Invariant:** Revenue is officially recognized **if and only if** order status is `DELIVERED`. `PENDING`, `SHIPPED`, and `CANCELLED` orders strictly contribute **0 VND** to revenue and COGS KPIs.
2. **Exact Monetary Precision:** All monetary fields use PostgreSQL `numeric(15,2)` and C# `decimal` with rounding to 2 decimal places (`multipleOf: 0.01`). Floating-point approximations (`float`, `double`) are strictly prohibited.
3. **Immutable Baseline Cost Snapshot:** Upon order item creation, the system queries `product_variants.cost_price` and freezes it permanently into `order_items.unit_cost_snapshot`. Subsequent catalog price mutations never alter historical order line snapshots.
4. **Immutable Fee Snapshot:** Upon transitioning to `DELIVERED`, marketplace fees are evaluated via the Strategy Pattern and permanently frozen into `order_fee_snapshots` with `is_immutable = true`.
5. **Canonical Variance Formula:**
   $$\text{Variance Amount} = \text{Projected Settlement} - \text{Actual Settlement}$$
   *(Positive variance = payout shortfall / funds withheld by channel; Negative variance = unexpected platform overpayment).*
6. **Separation of Duties (RBAC):** 
   - `Sales & Ops Staff`: Order recording, status progression, order cancellation, selectable SKU queries (strictly zero visibility into baseline `costPrice` or financial profit analytics).
   - `Finance Manager`: Read orders, fee previews, manual bank/wallet settlement entry, discrepancy investigation/resolution, 5 Core Financial KPIs, CSV exports, catalog retail price and baseline cost management. No order creation/editing.
   - `Shop Owner`: Full system access across all business capabilities and executive analytics.
7. **Strict Prohibition of "Net Profit":** Creating any column, endpoint, or schema property named `net_profit`, `net_income`, or `operating_profit` is strictly prohibited. Contribution Profit is dynamically derived as $\text{Projected Settlement} - \text{COGS}$ and excludes operational overhead (rent, payroll, marketing OPEX, taxes).

---

## 2. Master End-to-End Traceability Matrix (100% OpenAPI Coverage)

| # | Use Case | HTTP & Route | Operation ID | UI Component / Modal | Controller Action | Application Service | Repository Port | Target P05 Tables | RBAC Guard |
|---|---|---|---|---|---|---|---|---|---|
| **01** | `UC01` | `GET /catalog/variants/selectable` | `getSelectableVariants` | `CreateOrderModal` (`ProductSelector`) | `CatalogController.GetSelectableVariants` | `CatalogService.GetSelectableVariantsAsync` | `IProductRepository` | `products`, `product_variants` | `Sales & Ops Staff`, `Shop Owner` |
| **02** | `UC12` | `GET /catalog/products` | `listProducts` | `CatalogPage` (`ProductTable`) | `CatalogController.ListProducts` | `CatalogService.ListProductsAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **03** | `UC12` | `POST /catalog/products` | `createProduct` | `AddProductModal` | `CatalogController.CreateProduct` | `CatalogService.CreateProductAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **04** | `UC12` | `GET /catalog/products/{id}` | `getProductById` | `ProductDetailDrawer` | `CatalogController.GetProductById` | `CatalogService.GetProductByIdAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **05** | `UC12` | `PATCH /catalog/products/{id}` | `updateProduct` | `EditProductModal` | `CatalogController.UpdateProduct` | `CatalogService.UpdateProductAsync` | `IProductRepository` | `products` | `Finance Manager`, `Shop Owner` |
| **06** | `UC12` | `PATCH /catalog/variants/{id}` | `updateVariant` | `PricingCostEditorModal` | `CatalogController.UpdateVariant` | `CatalogService.UpdateVariantAsync` | `IProductRepository` | `product_variants` | `Finance Manager`, `Shop Owner` |
| **07** | `UC01` | `GET /orders` | `listOrders` | `OrdersPage` (`OrdersTable`) | `OrdersController.ListOrders` | `OrderService.ListOrdersAsync` | `IOrderRepository` | `orders`, `order_items`, `order_fee_snapshots` | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **08** | `UC01` | `POST /orders` | `createOrder` | `CreateOrderModal` | `OrdersController.CreateOrder` | `OrderService.CreateOrderAsync` | `IOrderRepository`, `IProductRepository` | `orders`, `order_items`, `order_status_history` | `Sales & Ops Staff`, `Shop Owner` |
| **09** | `UC02` | `POST /orders/preview-fee` | `previewOrderFees` | `CreateOrderModal` (`FeePreviewWidget`) | `OrdersController.PreviewFees` | `DynamicFeeEngine.CalculateFees` | `IFeeScheduleRepository` | `fee_schedules` *(In-Memory calculation)* | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **10** | `UC01`, `UC03` | `GET /orders/{id}` | `getOrderById` | `OrderDetailDrawer` | `OrdersController.GetOrderById` | `OrderService.GetOrderByIdAsync` | `IOrderRepository` | `orders`, `order_items`, `order_status_history`, `order_fee_snapshots` | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **11** | `UC03` | `PATCH /orders/{id}/status` | `updateOrderStatus` | `OrdersTable` (Status Action) | `OrdersController.UpdateStatus` | `OrderService.UpdateStatusAsync` | `IOrderRepository`, `IFeeScheduleRepository`, `IReconciliationRepository` | `orders`, `order_status_history`, `order_fee_snapshots`, `reconciliation_records` | `Sales & Ops Staff`, `Shop Owner` |
| **12** | `UC04` | `POST /orders/{id}/cancel` | `cancelOrder` | `CancelOrderModal` | `OrdersController.CancelOrder` | `OrderService.CancelOrderAsync` | `IOrderRepository` | `orders`, `order_status_history` | `Sales & Ops Staff`, `Shop Owner` |
| **13** | `UC05` | `GET /settlements` | `getSettlementLedger` | `SettlementPage` (`LedgerTable`) | `SettlementController.GetLedger` | `SettlementService.GetLedgerAsync` | `IReconciliationRepository` | `reconciliation_records`, `orders`, `order_fee_snapshots` | `Finance Manager`, `Shop Owner` |
| **14** | `UC05` | `GET /settlements/summary` | `getSettlementSummary` | `SettlementKPIHeader` | `SettlementController.GetSummary` | `SettlementService.GetSummaryAsync` | `IReconciliationRepository` | `reconciliation_records` | `Finance Manager`, `Shop Owner` |
| **15** | `UC06` | `POST /settlements/{orderId}/reconcile` | `reconcileSettlement` | `RecordSettlementModal` | `SettlementController.Reconcile` | `SettlementService.ReconcileAsync` | `IReconciliationRepository`, `IDiscrepancyRepository` | `reconciliation_records`, `discrepancy_audits` | `Finance Manager`, `Shop Owner` |
| **16** | `UC07` | `GET /discrepancies` | `listDiscrepancies` | `DiscrepancyPanel` (`DiscrepancyTable`) | `DiscrepanciesController.List` | `DiscrepancyService.ListAsync` | `IDiscrepancyRepository` | `discrepancy_audits`, `reconciliation_records`, `orders` | `Finance Manager`, `Shop Owner` |
| **17** | `UC07` | `GET /discrepancies/{id}` | `getDiscrepancyById` | `DiscrepancyDetailDrawer` | `DiscrepanciesController.GetById` | `DiscrepancyService.GetByIdAsync` | `IDiscrepancyRepository` | `discrepancy_audits`, `reconciliation_records` | `Finance Manager`, `Shop Owner` |
| **18** | `UC07` | `PATCH /discrepancies/{id}/resolve` | `resolveDiscrepancy` | `ResolveDiscrepancyModal` | `DiscrepanciesController.Resolve` | `DiscrepancyService.ResolveAsync` | `IDiscrepancyRepository` | `discrepancy_audits` | `Finance Manager`, `Shop Owner` |
| **19** | `UC08`, `UC13` | `GET /analytics/kpis` | `getFinancialKpis` | `ExecutiveKPIHeader` | `AnalyticsController.GetKpis` | `AnalyticsService.GetKpisAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **20** | `UC08`, `UC13` | `GET /analytics/trend` | `getFinancialTrend` | `RevenueProfitTrendChart` | `AnalyticsController.GetTrend` | `AnalyticsService.GetTrendAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **21** | `UC10`, `UC13` | `GET /analytics/channel-breakdown` | `getChannelBreakdown` | `ChannelBreakdownChart` | `AnalyticsController.GetBreakdown` | `AnalyticsService.GetBreakdownAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **22** | `UC10`, `UC13` | `GET /analytics/top-skus` | `getTopSkus` | `TopSkuTable` | `AnalyticsController.GetTopSkus` | `AnalyticsService.GetTopSkusAsync` | `IAnalyticsRepository` | `order_items`, `orders`, `product_variants`, `products` | `Finance Manager`, `Shop Owner` |
| **23** | `UC11` | `GET /analytics/drilldown` | `getDrilldownOrders` | `DrilldownOrderModal` | `AnalyticsController.GetDrilldown` | `AnalyticsService.GetDrilldownAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **24** | `UC11` | `GET /analytics/export-csv` | `exportReconciliationCsv` | `ExportCsvButton` | `AnalyticsController.ExportCsv` | `AnalyticsService.ExportCsvAsync` | `IAnalyticsRepository` | `reconciliation_records`, `orders`, `order_fee_snapshots` | `Finance Manager`, `Shop Owner` |

---

## 3. Detailed Architectural Traceability by Module

### 3.1. Module 1: Catalog Management & SKU Selection (`/catalog`)

#### [01] `GET /catalog/variants/selectable`
- **Use Case Trace:** `UC01` (Create Order), `UC12` (Catalog Management).
- **UI Trigger:** `CreateOrderModal` $\rightarrow$ `ProductSelector` dropdown.
- **Backend Invocation:** `CatalogController.GetSelectableVariants(search)` $\rightarrow$ `ICatalogService.GetSelectableVariantsAsync(search)`.
- **Repository Interface:** `IProductRepository.GetSelectableVariantsAsync(search)`.
- **Target Tables:** `products` (INNER JOIN) `product_variants` WHERE `is_active = TRUE`.
- **Anti-Cost Leakage Guarantee:**
  - Response DTO: `SelectableVariantListResponse` containing `[ { id, skuCode, productName, color, size, retailPrice, isActive } ]`.
  - **`costPrice` is strictly omitted from the DTO** to ensure `Sales & Ops Staff` cannot view baseline product costs.

#### [02 - 06] Catalog Management Endpoints (`/catalog/products`, `/catalog/variants/{id}`)
- **Use Case Trace:** `UC12` (Maintain Baseline Unit Cost & Product Catalog).
- **UI Triggers:** `CatalogPage`, `AddProductModal`, `PricingCostEditorModal`.
- **Backend Invocation:** `CatalogController` $\rightarrow$ `ICatalogService` $\rightarrow$ `IProductRepository`.
- **Target Tables:** `products`, `product_variants`.
- **Integrity Validation:**
  - `retail_price >= 0` and `cost_price >= 0` enforced via PostgreSQL CHECK constraints `chk_product_variants_prices`.
  - Master product soft deactivation toggles `products.is_active` without cascade deletion.

---

### 3.2. Module 2: Order Recording & Fee Preview (`/orders`)

#### [07] `GET /orders`
- **Use Case Trace:** `UC01` (Multi-Channel Order Recording).
- **UI Trigger:** `OrdersPage` $\rightarrow$ `OrdersTable`.
- **Backend Invocation:** `OrdersController.ListOrders(channel, status, search, page, pageSize)`.
- **Repository Interface:** `IOrderRepository.GetPagedOrdersAsync(...)`.
- **Target Tables:** `orders` (LEFT JOIN `order_items`, `order_fee_snapshots`).
- **Pagination Standard:** `page >= 1`, `pageSize` (default 20, max 100), returning `totalItems` and `totalPages`.

#### [08] `POST /orders`
- **Use Case Trace:** `UC01` (Multi-Channel Order Recording & Cost Freezing).
- **UI Trigger:** `CreateOrderModal` Submit button.
- **Backend Invocation:** `OrdersController.CreateOrder(CreateOrderRequest)` $\rightarrow$ `IOrderService.CreateOrderAsync(...)`.
- **Authoritative Cost Freezing Flow:**
  1. Client sends: `externalOrderId`, `channel`, `paymentMethod`, `customerName`, `customerPhone`, `shopVoucher`, and item inputs `[ { productVariantId, quantity, unitPrice } ]`.
  2. Client **never sends** `unitCostSnapshot`, `cogs`, or `contributionProfit`.
  3. `OrderService` queries `IProductRepository` to retrieve current authoritative `product_variants.cost_price`.
  4. Freezes snapshot permanently into `order_items.unit_cost_snapshot`.
  5. Computes:
     - `order_items.line_total = quantity * unit_price`
     - `order_items.total_cost = quantity * unit_cost_snapshot`
     - `orders.subtotal = SUM(line_total)`
     - `orders.gross_revenue = subtotal - shop_voucher`
  6. Order is created in `PENDING` status.
  7. Inserts initial audit record into `order_status_history` (`from_status = NULL`, `to_status = 'PENDING'`).
- **Target Tables:** `orders`, `order_items`, `order_status_history`.
- **Validation Guard:**
  - $0 \le \text{shopVoucher} \le \text{subtotal}$ (HTTP 422 if violated).
  - Channel / Payment compatibility: `TIKTOK` and `SHOPEE` require `MARKETPLACE_WALLET`; `POS` permits `CASH` or `POS_CARD_QR`.
  - Uniqueness: `(channel, external_order_id)` unique constraint (HTTP 409 Conflict if duplicate).

#### [09] `POST /orders/preview-fee`
- **Use Case Trace:** `UC02` (Estimate Platform Fees in Real-Time).
- **UI Trigger:** `CreateOrderModal` $\rightarrow$ `FeePreviewWidget` (dynamic debounce calculation).
- **Backend Invocation:** `OrdersController.PreviewFees(FeePreviewRequest)` $\rightarrow$ `DynamicFeeEngine.CalculateFees(...)`.
- **Repository Interface:** `IFeeScheduleRepository.GetActiveScheduleAsync(channel, paymentMethod)`.
- **Canonical Calculation Rules:**
  - **TikTok Shop:**
    $$\text{Commission} = 4.0\% \times \text{Gross Revenue}$$
    $$\text{Payment} = 3.0\% \times \text{Gross Revenue}$$
    $$\text{Fixed} = 3{,}000 \text{ VND per order}$$
  - **Shopee:**
    $$\text{Commission} = 4.5\% \times \text{Gross Revenue}$$
    $$\text{Payment} = 4.0\% \times \text{Gross Revenue}$$
    $$\text{Service} = \min(2.0\% \times \text{Gross Revenue}, 20{,}000 \text{ VND})$$
  - **POS:**
    - Cash: 0 VND fees.
    - Card/QR: 1.0% Payment processing fee.
- **Stateless Guarantee:** Zero database write operations. Pure in-memory calculation.

#### [11] `PATCH /orders/{id}/status`
- **Use Case Trace:** `UC03` (Track Order Status Progression & Recognize Revenue).
- **UI Trigger:** `OrdersTable` Action Menu $\rightarrow$ "Ship Order" or "Confirm Delivery".
- **Backend Invocation:** `OrdersController.UpdateStatus(id, toStatus)` $\rightarrow$ `IOrderService.UpdateStatusAsync(...)`.
- **State Machine Rules:**
  - Allowed transitions: `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`.
  - Skipping states (e.g. `PENDING` $\rightarrow$ `DELIVERED`) returns HTTP 409 Conflict.
  - Backward transitions (e.g. `DELIVERED` $\rightarrow$ `SHIPPED`) returns HTTP 409 Conflict.
- **Delivery Progression Pipeline (`SHIPPED` $\rightarrow$ `DELIVERED`):**
  1. `DynamicFeeEngine` evaluates final fee deduction breakdown.
  2. Persists immutable snapshot into `order_fee_snapshots` (`is_immutable = TRUE`).
  3. Officially recognizes `Gross Revenue` and merchandise `COGS`.
  4. Derives `Contribution Profit = Projected Settlement - Total COGS`.
  5. Inserts initial reconciliation record into `reconciliation_records` in `PENDING_SETTLEMENT` status (`actual_settlement = NULL`, `variance_amount = NULL`).
  6. Inserts transition log into `order_status_history`.
- **Target Tables:** `orders`, `order_status_history`, `order_fee_snapshots`, `reconciliation_records`.

#### [12] `POST /orders/{id}/cancel`
- **Use Case Trace:** `UC04` (Cancel Order & Exclude from Revenue/Profit).
- **UI Trigger:** `CancelOrderModal` Confirmation.
- **Backend Invocation:** `OrdersController.CancelOrder(id, reason)` $\rightarrow$ `IOrderService.CancelOrderAsync(...)`.
- **Exclusion Guarantee:**
  - Status transitions to `CANCELLED`.
  - Records `cancelled_at` timestamp and non-empty `cancellation_reason`.
  - Order is excluded 100% from revenue recognition, COGS accumulation, and analytics.
  - **Cancelling a `DELIVERED` order is strictly rejected with HTTP 422 Unprocessable Entity.**
- **Target Tables:** `orders`, `order_status_history`.

---

### 3.3. Module 3: Settlement Ledger & Manual Reconciliation (`/settlements`)

#### [13 - 14] `GET /settlements` & `GET /settlements/summary`
- **Use Case Trace:** `UC05` (View Settlement Reconciliation Ledger).
- **UI Triggers:** `SettlementPage` $\rightarrow$ `LedgerTable`, `SettlementKPIHeader`.
- **Backend Invocation:** `SettlementController.GetLedger(...)` and `SettlementController.GetSummary()`.
- **Repository Interface:** `IReconciliationRepository`.
- **Target Tables:** `reconciliation_records` (INNER JOIN `orders`, `order_fee_snapshots`).
- **Summary Counters:** Returns aggregated counts for `pendingSettlementCount`, `reconciledCount`, and `discrepancyCount`.

#### [15] `POST /settlements/{orderId}/reconcile`
- **Use Case Trace:** `UC06` (Record Manual Settlement & Reconcile Variance).
- **UI Trigger:** `RecordSettlementModal` Submit button.
- **Backend Invocation:** `SettlementController.Reconcile(orderId, request)` $\rightarrow$ `ISettlementService.ReconcileAsync(...)`.
- **Authoritative Reconciliation Logic:**
  1. Client sends: `actualSettlement` and optional/mandatory `notes`.
  2. Client **never sends** `varianceAmount` or `reconciliationStatus`.
  3. Backend derives variance:
     $$\text{varianceAmount} = \text{projectedSettlement} - \text{actualSettlement}$$
  4. If `varianceAmount == 0`:
     - Updates `reconciliation_records`: `status = 'RECONCILED'`, `reconciled_at = NOW()`, `reconciled_by = CurrentUser`.
  5. If `varianceAmount != 0`:
     - Updates `reconciliation_records`: `status = 'DISCREPANCY'`, `reconciled_at = NOW()`, `reconciled_by = CurrentUser`.
     - Non-empty `notes` is mandatory (HTTP 422 if empty).
     - Auto-generates initial discrepancy audit entry in `discrepancy_audits` (`discrepancy_type = 'UNEXPECTED_PLATFORM_CHARGE'`).
  6. **Order status is NEVER altered:** The underlying order remains `DELIVERED`.
- **Target Tables:** `reconciliation_records`, `discrepancy_audits`.

---

### 3.4. Module 4: Discrepancy Auditing & Dispute Resolution (`/discrepancies`)

#### [16 - 17] `GET /discrepancies` & `GET /discrepancies/{id}`
- **Use Case Trace:** `UC07` (Audit Discrepancies & File Disputes).
- **UI Triggers:** `DiscrepancyPanel` (`DiscrepancyTable`), `DiscrepancyDetailDrawer`.
- **Backend Invocation:** `DiscrepanciesController` $\rightarrow$ `IDiscrepancyService` $\rightarrow$ `IDiscrepancyRepository`.
- **Target Tables:** `discrepancy_audits` (JOIN `reconciliation_records`, `orders`).

#### [18] `PATCH /discrepancies/{id}/resolve`
- **Use Case Trace:** `UC07` (Audit Discrepancies & File Disputes).
- **UI Trigger:** `ResolveDiscrepancyModal` Confirm.
- **Backend Invocation:** `DiscrepanciesController.Resolve(id, request)` $\rightarrow$ `IDiscrepancyService.ResolveAsync(...)`.
- **Resolution Execution:**
  - Captures `resolutionNotes` (minimum 5 characters).
  - Automatically sets `is_resolved = TRUE`, `resolved_at = clock_timestamp()`, and `resolved_by = CurrentUser.Username`.
  - Eliminates legacy multi-level approval workflows (`PENDING_APPROVAL`, `APPROVED`, `REJECTED` are deprecated).
- **Target Tables:** `discrepancy_audits`.

---

### 3.5. Module 5: Financial Analytics & Export (`/analytics`)

#### [19] `GET /analytics/kpis`
- **Use Case Trace:** `UC08` (Analyze Multi-Channel Contribution Profit & Margins), `UC13` (Track Margin Trends).
- **UI Trigger:** `ExecutiveKPIHeader` (5 KPI Cards).
- **Backend Invocation:** `AnalyticsController.GetKpis(from, to, channel)` $\rightarrow$ `IAnalyticsService.GetKpisAsync(...)`.
- **The 5 Core Financial KPIs:**
  1. `grossRevenue`: $\sum(\text{Gross Revenue})$ (aggregated from `orders.gross_revenue`) for `DELIVERED` orders.
  2. `totalPlatformFees`: $\sum(\text{Total Platform Fees})$ (aggregated from `order_fee_snapshots.total_platform_fees`) for `DELIVERED` orders.
  3. `projectedSettlement`: $\text{Gross Revenue} - \text{Total Platform Fees}$.
  4. `cogs`: $\sum(\text{Line Total Cost})$ (aggregated from `order_items.total_cost`) for `DELIVERED` orders.
  5. `contributionProfit`: $\text{Projected Settlement} - \text{COGS}$.
  - Plus: `contributionMarginPct`: $(\text{Contribution Profit} / \text{Gross Revenue}) \times 100$.
- **Zero Phantom Revenue:** Orders with status `PENDING`, `SHIPPED`, or `CANCELLED` are strictly excluded from all aggregations.

#### [20] `GET /analytics/trend`
- **Use Case Trace:** `UC08`, `UC13`.
- **UI Trigger:** `RevenueProfitTrendChart`.
- **Metrics Plotted:** Daily time-series of `Gross Revenue` vs. `Contribution Profit` (replacing legacy "Gross Revenue vs Net Cash").

#### [21] `GET /analytics/channel-breakdown`
- **Use Case Trace:** `UC10` (Compare Channel & SKU Profitability), `UC13`.
- **UI Trigger:** `ChannelBreakdownChart` (Donut / Bar).
- **Metrics Segmented:** Multi-channel breakdown across `TIKTOK`, `SHOPEE`, and `POS`.

#### [22] `GET /analytics/top-skus`
- **Use Case Trace:** `UC10`, `UC13`.
- **UI Trigger:** `TopSkuTable`.
- **Ranking Capabilities:** Sort by `CONTRIBUTION_PROFIT`, `GROSS_REVENUE`, or `DELIVERED_UNITS`.

#### [23] `GET /analytics/drilldown`
- **Use Case Trace:** `UC11` (Drilldown to Source Orders).
- **UI Trigger:** `DrilldownOrderModal` (triggered by clicking KPI cards or chart points).
- **Itemized Audit:** Returns paginated delivered order records backing the aggregated figures.

#### [24] `GET /analytics/export-csv`
- **Use Case Trace:** `UC11` (Export Financial & Settlement Reports).
- **UI Trigger:** `ExportCsvButton`.
- **Stream Format:** RFC 4180 CSV (`text/csv`) containing delivered order items, platform fee breakdowns, actual settlement, and reconciliation variance for accounting audits.

---

## 4. P05 Entity & Schema Cross-Verification

To prevent schema deviation, the following table confirms that **100% of P06 API operations** map strictly to the 9 canonical PostgreSQL tables defined in Phase P05:

| Canonical P05 Table | CRUD Operations in P06 | Excluded Legacy Concepts |
|---|---|---|
| `products` | C, R, U (via `CatalogController`) | Soft-delete / Hard-delete (P05 uses `is_active`) |
| `product_variants` | C, R, U (via `CatalogController`) | Direct mutation from order creation (Snapshots are decoupled) |
| `orders` | C, R, U (via `OrdersController`) | Direct net profit columns; client-supplied cost/fees |
| `order_items` | C, R (via `OrdersController`) | Client-supplied `unit_cost_snapshot` or `total_cost` |
| `order_status_history` | C, R (via `OrdersController`) | Client-supplied timestamp (server clock sets `changed_at`) |
| `fee_schedules` | R (via `DynamicFeeEngine`) | Public CRUD UI in MVP (managed as configuration data) |
| `order_fee_snapshots` | C, R (via `OrdersController`, `SettlementController`) | Mutability after `DELIVERED` (`is_immutable = TRUE`) |
| `reconciliation_records`| C, R, U (via `SettlementController`) | Statement spreadsheet upload; client-supplied variance |
| `discrepancy_audits` | C, R, U (via `DiscrepanciesController`) | Multi-level approval status (`PENDING_APPROVAL`, `APPROVED`) |

> [!NOTE]
> **Eliminated Legacy Concepts:**
> The following legacy tables and concepts from older drafts have been completely purged from the P06 API specification:
> - `channels` table (replaced by canonical enum `ChannelType`: `TIKTOK`, `SHOPEE`, `POS`).
> - `statement_imports` and `statement_lines` tables (statement spreadsheet file parsing is out-of-scope for MVP).
> - `StatementParser` and SHA-256 upload deduplication.
> - Direct marketplace webhooks or background queue ingestion workers.

---

## 5. Role-Based Access Control (RBAC) & Cost Privacy Matrix

| Endpoint Group / Route | Operation ID | Sales & Ops Staff | Finance Manager | Shop Owner | Cost Leakage Guard |
|---|---|:---:|:---:|:---:|---|
| `GET /catalog/variants/selectable` | `getSelectableVariants` | ✅ | ❌ | ✅ | **`costPrice` strictly stripped from response.** |
| `GET /catalog/products` | `listProducts` | ❌ | ✅ | ✅ | Full catalog visibility including baseline cost. |
| `POST /catalog/products` | `createProduct` | ❌ | ✅ | ✅ | Catalog creation restricted to financial managers. |
| `PATCH /catalog/products/{id}` | `updateProduct` | ❌ | ✅ | ✅ | Master product updates. |
| `PATCH /catalog/variants/{id}` | `updateVariant` | ❌ | ✅ | ✅ | Baseline cost modification restricted. |
| `GET /orders` | `listOrders` | ✅ | ✅ | ✅ | Item costs hidden from Sales in list view. |
| `POST /orders` | `createOrder` | ✅ | ❌ | ✅ | Frontend input only; backend freezes cost. |
| `POST /orders/preview-fee` | `previewOrderFees` | ✅ | ✅ | ✅ | Real-time fee preview (no cost data shown). |
| `GET /orders/{id}` | `getOrderById` | ✅ | ✅ | ✅ | `unitCostSnapshot`, `cogs`, `contributionProfit` omitted for Sales. |
| `PATCH /orders/{id}/status` | `updateOrderStatus` | ✅ | ❌ | ✅ | Status progression. |
| `POST /orders/{id}/cancel` | `cancelOrder` | ✅ | ❌ | ✅ | Active order cancellation. |
| `GET /settlements` | `getSettlementLedger` | ❌ | ✅ | ✅ | Settlement ledger restricted to finance/owner. |
| `GET /settlements/summary` | `getSettlementSummary` | ❌ | ✅ | ✅ | Audit counters restricted to finance/owner. |
| `POST /settlements/{orderId}/reconcile` | `reconcileSettlement` | ❌ | ✅ | ✅ | Manual actual payout entry. |
| `GET /discrepancies` | `listDiscrepancies` | ❌ | ✅ | ✅ | Discrepancy investigation restricted. |
| `GET /discrepancies/{id}` | `getDiscrepancyById` | ❌ | ✅ | ✅ | Discrepancy investigation restricted. |
| `PATCH /discrepancies/{id}/resolve` | `resolveDiscrepancy` | ❌ | ✅ | ✅ | Discrepancy resolution restricted. |
| `GET /analytics/kpis` | `getFinancialKpis` | ❌ | ✅ | ✅ | Executive KPIs restricted to finance/owner. |
| `GET /analytics/trend` | `getFinancialTrend` | ❌ | ✅ | ✅ | Financial trend restricted to finance/owner. |
| `GET /analytics/channel-breakdown` | `getChannelBreakdown` | ❌ | ✅ | ✅ | Channel breakdown restricted to finance/owner. |
| `GET /analytics/top-skus` | `getTopSkus` | ❌ | ✅ | ✅ | Top merchandise performance restricted. |
| `GET /analytics/drilldown` | `getDrilldownOrders` | ❌ | ✅ | ✅ | Order drilldown restricted to finance/owner. |
| `GET /analytics/export-csv` | `exportReconciliationCsv` | ❌ | ✅ | ✅ | CSV export restricted to finance/owner. |
