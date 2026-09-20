# P06 — API Traceability Matrix: End-to-End Architectural Traceability


## 1. Scope & Architectural Traceability Methodology

This document establishes the **authoritative end-to-end traceability matrix** connecting business requirements, use cases, frontend UI components, API routes, ASP.NET Core backend controllers, application business services, domain strategy engines, repository ports, and the 9 canonical PostgreSQL database tables defined in Phase P05.

```
[UI Trigger / Modal / Table]
       │
       ▼ (HTTPS REST / JSON camelCase)
[ASP.NET Core Controller Action] (`FashionWeb.Api.Controllers`)
       │
       ▼ (Dependency Inversion Interface Invocation)
[Application Business Service] (`FashionWeb.Business.Services`)
       │
       ├─► [Dynamic Fee Engine / Strategy Pattern] (`FashionWeb.Business.Strategies`)
       │
       ▼ (Repository Port Interface) (`FashionWeb.Business.Interfaces`)
[Persistence Adapter / EF Core Repository] (`FashionWeb.Data.Repositories`)
       │
       ▼ (Npgsql / PostgreSQL 16+)
[Canonical Relational Tables] (9 P05 Database Tables)
       │
       ▼ (Invariant Verification)
[Mathematical Financial Rules & RBAC Guard Policies]
```

### Core Financial Rules & System Invariants:
1. **Zero Phantom Revenue Invariant:** Revenue is officially recognized **if and only if** order status is `DELIVERED`. `PENDING`, `SHIPPED`, and `CANCELLED` orders strictly contribute **0 VND** to revenue and COGS KPIs.
2. **Exact Monetary Precision:** All monetary fields map to PostgreSQL `numeric(15,2)` and C# `decimal` rounded to 2 decimal places (`multipleOf: 0.01`). Floating-point types (`float`, `double`) are strictly prohibited.
   - `NonNegativeMonetaryAmount`: Used for values that must never be negative (`retailPrice`, `costPrice`, `unitPrice`, `subtotal`, `shopVoucher`, `actualSettlement`, fee amounts, and fee caps).
   - `MonetaryAmount`: Used for values that may legally be negative (`varianceAmount`, `contributionProfit`).
3. **Immutable Baseline Cost Snapshot:** Upon order line creation, the backend queries `product_variants.cost_price` and freezes it permanently into `order_items.unit_cost_snapshot`. Subsequent catalog price mutations never alter historical order line snapshots.
4. **Immutable Fee Snapshot:** Upon transitioning to `DELIVERED`, marketplace fees are evaluated via the Strategy Pattern and permanently frozen into `order_fee_snapshots`. The `order_fee_snapshots` record is immutable after creation by business/persistence policy.
5. **Canonical Variance Formula:**
   $$\text{Variance Amount} = \text{Projected Settlement} - \text{Actual Settlement}$$
   *(Positive variance = payout shortfall / funds withheld by channel; Negative variance = unexpected platform overpayment).*
6. **Separation of Duties (RBAC):** 
   - `Sales & Ops Staff`: Order recording, status progression (`SHIPPED`, `DELIVERED`), cancellation, selectable SKU queries, operational order summary counters. Strictly zero visibility into baseline `costPrice`, unit cost snapshots, merchandise COGS, or financial profit analytics.
   - `Finance Manager`: Order inspection, fee previews, fee schedules read-only inspection, manual settlement reconciliation, discrepancy investigation/resolution, 5 Core Financial KPIs, CSV exports, catalog retail price and baseline cost management. No order creation/editing, no fee schedule mutation.
   - `Shop Owner`: Full system access across all business capabilities, fee schedule versioning (`POST /fee-schedules`), and executive analytics.
7. **Strict Prohibition of "Net Profit":** Creating any column, endpoint, or schema property named `net_profit`, `net_income`, or `operating_profit` is strictly prohibited. Contribution Profit is dynamically derived as $\text{Projected Settlement} - \text{COGS}$ and excludes operational overhead (rent, payroll, marketing OPEX, taxes).
8. **Top SKU Contribution Profit Derivation Rule (Analytics Derivation):**
   Because platform fees and vouchers are recorded at the order level in the canonical P05 database, calculating SKU-level Contribution Profit requires proportional allocation based on line subtotal share:
   $$\text{lineShare} = \frac{\text{lineSubtotal}}{\text{orderSubtotal}}$$
   $$\text{allocatedVoucher} = \text{orderVoucher} \times \text{lineShare}$$
   $$\text{lineGrossRevenue} = \text{lineSubtotal} - \text{allocatedVoucher}$$
   $$\text{allocatedPlatformFees} = \text{orderTotalPlatformFees} \times \text{lineShare}$$
   $$\text{lineContributionProfit} = \text{lineGrossRevenue} - \text{allocatedPlatformFees} - \text{lineCOGS}$$

---

## 2. Master End-to-End Traceability Matrix (100% OpenAPI Coverage — 27 Operations)

| # | Use Case | HTTP & Route | Operation ID | UI Component / Modal | Controller Action | Application Business Service | Repository Interface | Target P05 Tables | RBAC Guard |
|---|---|---|---|---|---|---|---|---|---|
| **01** | `UC01` | `GET /catalog/variants/selectable` | `getSelectableVariants` | `CreateOrderModal` (`ProductSelector`) | `CatalogController.GetSelectableVariants` | `CatalogService.GetSelectableVariantsAsync` | `IProductRepository` | `products`, `product_variants` | `Sales & Ops Staff`, `Shop Owner` |
| **02** | `UC12` | `GET /catalog/products` | `listProducts` | `CatalogPage` (`ProductTable`) | `CatalogController.ListProducts` | `CatalogService.ListProductsAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **03** | `UC12` | `POST /catalog/products` | `createProduct` | `AddProductModal` | `CatalogController.CreateProduct` | `CatalogService.CreateProductAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **04** | `UC12` | `GET /catalog/products/{id}` | `getProductById` | `ProductDetailDrawer` | `CatalogController.GetProductById` | `CatalogService.GetProductByIdAsync` | `IProductRepository` | `products`, `product_variants` | `Finance Manager`, `Shop Owner` |
| **05** | `UC12` | `PATCH /catalog/products/{id}` | `updateProduct` | `EditProductModal` | `CatalogController.UpdateProduct` | `CatalogService.UpdateProductAsync` | `IProductRepository` | `products` | `Finance Manager`, `Shop Owner` |
| **06** | `UC12` | `PATCH /catalog/variants/{id}` | `updateVariant` | `PricingCostEditorModal` | `CatalogController.UpdateVariant` | `CatalogService.UpdateVariantAsync` | `IProductRepository` | `product_variants` | `Finance Manager`, `Shop Owner` |
| **07** | `UC01` | `GET /orders` | `listOrders` | `OrdersPage` (`OrdersTable`) | `OrdersController.ListOrders` | `OrderService.ListOrdersAsync` | `IOrderRepository` | `orders`, `order_items`, `order_fee_snapshots` | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **08** | `UC01` | `POST /orders` | `createOrder` | `CreateOrderModal` | `OrdersController.CreateOrder` | `OrderService.CreateOrderAsync` | `IOrderRepository`, `IProductRepository` | `orders`, `order_items`, `order_status_history` | `Sales & Ops Staff`, `Shop Owner` |
| **09** | `UC01` | `GET /orders/summary` | `getOrderSummary` | `OrdersPage` (`OrderMetricsCards`) | `OrdersController.GetSummary` | `OrderService.GetSummaryAsync` | `IOrderRepository` | `orders` | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **10** | `UC02` | `POST /orders/preview-fee` | `previewOrderFees` | `CreateOrderModal` (`FeePreviewWidget`) | `OrdersController.PreviewFees` | `DynamicFeeEngine.CalculateFees` | `IFeeScheduleRepository` | `fee_schedules` *(In-Memory)* | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **11** | `UC01`, `UC03` | `GET /orders/{id}` | `getOrderById` | `OrderDetailDrawer` | `OrdersController.GetOrderById` | `OrderService.GetOrderByIdAsync` | `IOrderRepository` | `orders`, `order_items`, `order_status_history`, `order_fee_snapshots` | `Sales & Ops Staff`, `Finance Manager`, `Shop Owner` |
| **12** | `UC03` | `PATCH /orders/{id}/status` | `updateOrderStatus` | `OrdersTable` (Status Action) | `OrdersController.UpdateStatus` | `OrderService.UpdateStatusAsync` | `IOrderRepository`, `IFeeScheduleRepository`, `IReconciliationRepository` | `orders`, `order_status_history`, `order_fee_snapshots`, `reconciliation_records` | `Sales & Ops Staff`, `Shop Owner` |
| **13** | `UC04` | `POST /orders/{id}/cancel` | `cancelOrder` | `CancelOrderModal` | `OrdersController.CancelOrder` | `OrderService.CancelOrderAsync` | `IOrderRepository` | `orders`, `order_status_history` | `Sales & Ops Staff`, `Shop Owner` |
| **14** | `UC02` | `GET /fee-schedules` | `listFeeSchedules` | `FeeSettingsPage` / `FeeScheduleDrawer` | `FeeSchedulesController.ListFeeSchedules` | `FeeScheduleService.GetActiveSchedulesAsync` | `IFeeScheduleRepository` | `fee_schedules` | `Finance Manager`, `Shop Owner` |
| **15** | `UC02` | `POST /fee-schedules` | `createFeeSchedule` | `CreateFeeScheduleModal` | `FeeSchedulesController.CreateFeeSchedule` | `FeeScheduleService.CreateScheduleVersionAsync` | `IFeeScheduleRepository` | `fee_schedules` | `Shop Owner` |
| **16** | `UC05` | `GET /settlements` | `getSettlementLedger` | `SettlementPage` (`LedgerTable`) | `SettlementController.GetLedger` | `SettlementService.GetLedgerAsync` | `IReconciliationRepository` | `reconciliation_records`, `orders`, `order_fee_snapshots` | `Finance Manager`, `Shop Owner` |
| **17** | `UC05` | `GET /settlements/summary` | `getSettlementSummary` | `SettlementKPIHeader` | `SettlementController.GetSummary` | `SettlementService.GetSummaryAsync` | `IReconciliationRepository` | `reconciliation_records` | `Finance Manager`, `Shop Owner` |
| **18** | `UC06` | `POST /settlements/{orderId}/reconcile` | `reconcileSettlement` | `RecordSettlementModal` | `SettlementController.Reconcile` | `SettlementService.ReconcileAsync` | `IReconciliationRepository`, `IDiscrepancyRepository` | `reconciliation_records`, `discrepancy_audits` | `Finance Manager`, `Shop Owner` |
| **19** | `UC07` | `GET /discrepancies` | `listDiscrepancies` | `DiscrepancyPanel` (`DiscrepancyTable`) | `DiscrepanciesController.List` | `DiscrepancyService.ListAsync` | `IDiscrepancyRepository` | `discrepancy_audits`, `reconciliation_records`, `orders` | `Finance Manager`, `Shop Owner` |
| **20** | `UC07` | `GET /discrepancies/{id}` | `getDiscrepancyById` | `DiscrepancyDetailDrawer` | `DiscrepanciesController.GetById` | `DiscrepancyService.GetByIdAsync` | `IDiscrepancyRepository` | `discrepancy_audits`, `reconciliation_records` | `Finance Manager`, `Shop Owner` |
| **21** | `UC07` | `PATCH /discrepancies/{id}/resolve` | `resolveDiscrepancy` | `ResolveDiscrepancyModal` | `DiscrepanciesController.Resolve` | `DiscrepancyService.ResolveAsync` | `IDiscrepancyRepository` | `discrepancy_audits` | `Finance Manager`, `Shop Owner` |
| **22** | `UC08`, `UC13` | `GET /analytics/kpis` | `getFinancialKpis` | `ExecutiveKPIHeader` | `AnalyticsController.GetKpis` | `AnalyticsService.GetKpisAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **23** | `UC08`, `UC13` | `GET /analytics/trend` | `getFinancialTrend` | `RevenueProfitTrendChart` | `AnalyticsController.GetTrend` | `AnalyticsService.GetTrendAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **24** | `UC10`, `UC13` | `GET /analytics/channel-breakdown` | `getChannelBreakdown` | `ChannelBreakdownChart` | `AnalyticsController.GetBreakdown` | `AnalyticsService.GetBreakdownAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **25** | `UC10`, `UC13` | `GET /analytics/top-skus` | `getTopSkus` | `TopSkuTable` | `AnalyticsController.GetTopSkus` | `AnalyticsService.GetTopSkusAsync` | `IAnalyticsRepository` | `order_items`, `orders`, `product_variants`, `products` | `Finance Manager`, `Shop Owner` |
| **26** | `UC11` | `GET /analytics/drilldown` | `getDrilldownOrders` | `DrilldownOrderModal` | `AnalyticsController.GetDrilldown` | `AnalyticsService.GetDrilldownAsync` | `IAnalyticsRepository` | `orders`, `order_fee_snapshots`, `order_items` | `Finance Manager`, `Shop Owner` |
| **27** | `UC11` | `GET /analytics/export-csv` | `exportReconciliationCsv` | `ExportCsvButton` | `AnalyticsController.ExportCsv` | `AnalyticsService.ExportCsvAsync` | `IAnalyticsRepository` | `reconciliation_records`, `orders`, `order_fee_snapshots` | `Finance Manager`, `Shop Owner` |

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
- **P05 Schema Alignment:**
  - `products` table in P05 has no `description` column. The API requests (`CreateProductRequest`, `UpdateProductRequest`) and response (`ProductResponse`) strictly omit `description`.
  - Master product deactivation toggles `products.is_active` without cascade deletion.
  - Price constraints `retail_price >= 0` and `cost_price >= 0` are enforced via `chk_product_variants_prices` and `NonNegativeMonetaryAmount`.

---

### 3.2. Module 2: Order Recording & Fee Preview (`/orders`)

#### [07] `GET /orders`
- **Use Case Trace:** `UC01` (Multi-Channel Order Recording).
- **UI Trigger:** `OrdersPage` $\rightarrow$ `OrdersTable`.
- **Backend Invocation:** `OrdersController.ListOrders(channel, status, from, to, search, page, pageSize)` $\rightarrow$ `IOrderService.ListOrdersAsync(...)`.
- **Repository Interface:** `IOrderRepository.GetPagedOrdersAsync(...)`.
- **Query Filter Capabilities:**
  - `channel`: Filter by `ChannelType` (`TIKTOK`, `SHOPEE`, `POS`).
  - `status`: Filter by `OrderStatus` (`PENDING`, `SHIPPED`, `DELIVERED`, `CANCELLED`).
  - `from`, `to`: ISO 8601 created date-time filters.
  - `search`: Case-insensitive substring search matching across `externalOrderId`, `skuCode`, `customerName`, or `customerPhone`.
- **Lightweight Table Response (`OrderListItemResponse`):**
  - Includes: `id`, `externalOrderId`, `channel`, `paymentMethod`, `status`, `orderDate`, `customerName`, `customerPhone`, `subtotal`, `shopVoucher`, `grossRevenue`, `itemCount`, `itemsSummary: [ { skuCode, quantity } ]`, `deliveredAt`, `cancelledAt`, `createdAt`.
  - **Anti-Cost Leakage Guarantee:** `costPrice`, `cogs`, `unitCostSnapshot`, and `contributionProfit` are strictly omitted from `OrderListItemResponse` so `Sales & Ops Staff` can view orders without cost leaks.
  - `orderDate`: Populated from `orders.order_date` (`TIMESTAMPTZ`), providing exact order placement date for UI display.

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
  6. Order is created in `PENDING` status with server-generated `order_date`.
  7. Inserts initial audit record into `order_status_history` (`from_status = NULL`, `to_status = 'PENDING'`).
- **Target Tables:** `orders`, `order_items`, `order_status_history`.
- **Validation Guard:**
  - $0 \le \text{shopVoucher} \le \text{subtotal}$ (HTTP 422 if violated).
  - Channel / Payment compatibility: `TIKTOK` and `SHOPEE` require `MARKETPLACE_WALLET`; `POS` permits `CASH` or `POS_CARD_QR`.
  - Uniqueness: `(channel, external_order_id)` unique constraint (HTTP 409 Conflict if duplicate).

#### [09] `GET /orders/summary`
- **Use Case Trace:** `UC01` (Operational Order Metrics).
- **UI Trigger:** `OrdersPage` $\rightarrow$ `OrderMetricsCards`.
- **Backend Invocation:** `OrdersController.GetSummary(from, to, channel)` $\rightarrow$ `IOrderService.GetSummaryAsync(from, to, channel)`.
- **Repository Interface:** `IOrderRepository.GetSummaryMetricsAsync(...)`.
- **Target Tables:** `orders`.
- **Response Structure (`OrdersSummaryResponse`):**
  - `totalOrders`: Count of all orders in the filtered range.
  - `deliveredOrders`: Count of orders with `status = 'DELIVERED'`.
  - `grossRevenue`: Aggregate gross revenue **strictly recognized from `DELIVERED` orders only** ($\sum(\text{Gross Revenue})$ WHERE `status = 'DELIVERED'`). Other statuses strictly contribute 0 VND to prevent phantom revenue.
  - `inTransitOrders`: Count of orders with `status = 'SHIPPED'`.
  - `cancelledOrders`: Count of orders with `status = 'CANCELLED'`.
- **Filter Alignment:** Supports identical `from`, `to`, and `channel` filters to guarantee consistency between metrics cards and table rows.

#### [10] `POST /orders/preview-fee`
- **Use Case Trace:** `UC02` (Estimate Platform Fees in Real-Time).
- **UI Trigger:** `CreateOrderModal` $\rightarrow$ `FeePreviewWidget` (dynamic debounce calculation).
- **Backend Invocation:** `OrdersController.PreviewFees(FeePreviewRequest)` $\rightarrow$ `DynamicFeeEngine.CalculateFees(...)`.
- **Repository Interface:** `IFeeScheduleRepository.GetActiveScheduleAsync(channel, paymentMethod)`.
- **Canonical Calculation Rules (Consistent with P05 fee_schedules):**
  - **TikTok Shop:**
    $$\text{Commission} = \text{Subtotal} \times 4.0\%$$
    $$\text{Payment} = \text{Gross Revenue} \times 3.0\%$$
    $$\text{Fixed} = 3{,}000 \text{ VND per order}$$
  - **Shopee:**
    $$\text{Commission} = \text{Subtotal} \times 4.5\%$$
    $$\text{Payment} = \text{Gross Revenue} \times 4.0\%$$
    $$\text{Service} = \min(\text{Subtotal} \times 2.0\%, \text{configured serviceFeeCap})$$
    *(The Shopee service fee cap is dynamically resolved from active `fee_schedules.service_fee_cap`; eliminating any hardcoded upper limit).*
  - **POS:**
    - Cash: 0 VND fees.
    - Card/QR: $\text{Gross Revenue} \times 1.0\%$ payment processing fee.
- **Stateless Guarantee:** Zero database write operations. Pure in-memory calculation.

#### [11] `GET /orders/{id}`
- **Use Case Trace:** `UC01`, `UC03`.
- **UI Trigger:** `OrderDetailDrawer`.
- **Backend Invocation:** `OrdersController.GetOrderById(id)` $\rightarrow$ `IOrderService.GetOrderByIdAsync(id)`.
- **Target Tables:** `orders`, `order_items`, `order_status_history`, `order_fee_snapshots`.
- **Field Details:** Returns `orderDate`, item lines, status progression history, and frozen fee snapshot (if delivered). `cogs`, `unitCostSnapshot`, and `contributionProfit` are populated for `Finance Manager` and `Shop Owner`, but stripped for `Sales & Ops Staff`.

#### [12] `PATCH /orders/{id}/status`
- **Use Case Trace:** `UC03` (Track Order Status Progression & Recognize Revenue).
- **UI Trigger:** `OrdersTable` Action Menu $\rightarrow$ "Ship Order" or "Confirm Delivery".
- **Backend Invocation:** `OrdersController.UpdateStatus(id, request)` $\rightarrow$ `IOrderService.UpdateStatusAsync(...)`.
- **State Machine Guard (`OrderProgressStatus`):**
  - `toStatus` in `UpdateOrderStatusRequest` is strictly constrained to enum `OrderProgressStatus`: `SHIPPED` or `DELIVERED`.
  - Order cancellation is decoupled and strictly routed through `POST /orders/{id}/cancel`.
  - Allowed progressions: `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`.
  - Illegal progressions (e.g. `DELIVERED` $\rightarrow$ `SHIPPED` or skipping `SHIPPED`) return HTTP 409 Conflict.
- **Delivery Progression Pipeline (`SHIPPED` $\rightarrow$ `DELIVERED`):**
  1. `DynamicFeeEngine` evaluates final fee deduction breakdown.
  2. Persists immutable snapshot into `order_fee_snapshots` (`order_fee_snapshots is immutable after creation by business/persistence policy`).
  3. Officially recognizes `Gross Revenue` and merchandise `COGS`.
  4. Derives `Contribution Profit = Projected Settlement - Total COGS`.
  5. Inserts initial reconciliation record into `reconciliation_records` in `PENDING_SETTLEMENT` status (`actual_settlement = NULL`, `variance_amount = NULL`).
  6. Inserts transition log into `order_status_history`.
- **Target Tables:** `orders`, `order_status_history`, `order_fee_snapshots`, `reconciliation_records`.

#### [13] `POST /orders/{id}/cancel`
- **Use Case Trace:** `UC04` (Cancel Order & Exclude from Revenue/Profit).
- **UI Trigger:** `CancelOrderModal` Confirmation.
- **Backend Invocation:** `OrdersController.CancelOrder(id, request)` $\rightarrow$ `IOrderService.CancelOrderAsync(...)`.
- **Exclusion Guarantee:**
  - Status transitions to `CANCELLED`.
  - Records `cancelled_at` timestamp and mandatory non-empty `cancellationReason`.
  - Permanently excluded 100% from recognized revenue, COGS accumulation, and analytics.
  - **Cancelling an order that is already in `DELIVERED` status is strictly rejected with HTTP 422 Unprocessable Entity.**
- **Target Tables:** `orders`, `order_status_history`.

---

### 3.3. Module 3: Fee Schedules API (`/fee-schedules`)

#### [14] `GET /fee-schedules`
- **Use Case Trace:** `UC02` (Configure Marketplace & POS Fee Schedules).
- **UI Trigger:** `FeeSettingsPage` / `FeeScheduleDrawer`.
- **Backend Invocation:** `FeeSchedulesController.ListFeeSchedules(channel, paymentMethod)` $\rightarrow$ `IFeeScheduleService.GetActiveSchedulesAsync(...)`.
- **Repository Interface:** `IFeeScheduleRepository.GetSchedulesAsync(...)`.
- **Target Tables:** `fee_schedules`.
- **RBAC Guard:** Restricted to `Finance Manager` (Read) and `Shop Owner` (Read). `Sales & Ops Staff` have **No Access** (HTTP 403 Forbidden).
- **Response Structure (`FeeScheduleResponse`):**
  - Includes: `id`, `channel`, `paymentMethod`, `commissionRate`, `paymentFeeRate`, `serviceFeeRate`, `serviceFeeCap`, `fixedFeePerOrder`, `effectiveFrom`, `effectiveTo`, `isActive`, `createdAt`, `updatedAt`.

#### [15] `POST /fee-schedules`
- **Use Case Trace:** `UC02` (Fee Schedule Versioning & Policy Updates).
- **UI Trigger:** `CreateFeeScheduleModal` Submit button.
- **Backend Invocation:** `FeeSchedulesController.CreateFeeSchedule(CreateFeeScheduleRequest)` $\rightarrow$ `IFeeScheduleService.CreateScheduleVersionAsync(...)`.
- **Repository Interface:** `IFeeScheduleRepository.InsertScheduleVersionAsync(...)`.
- **Target Tables:** `fee_schedules`.
- **RBAC Guard:** Restricted strictly to `Shop Owner`. `Finance Manager` and `Sales & Ops Staff` have **No Access** (HTTP 403 Forbidden).
- **Versioning Business Policy:**
  - Creates a new rate schedule version without altering historical snapshots.
  - Deactivates previous active schedule for the matching channel and payment method by setting `effective_to = new_effective_from` and `is_active = FALSE`.
  - Inserts new active record with `effective_from = effectiveFrom`.
  - Orders already delivered retain their historical `order_fee_snapshots` intact.

---

### 3.4. Module 4: Settlement Ledger & Manual Reconciliation (`/settlements`)

#### [16] `GET /settlements`
- **Use Case Trace:** `UC05` (View Settlement Reconciliation Ledger).
- **UI Trigger:** `SettlementPage` $\rightarrow$ `LedgerTable`.
- **Backend Invocation:** `SettlementController.GetLedger(status, channel, from, to, page, pageSize)` $\rightarrow$ `ISettlementService.GetLedgerAsync(...)`.
- **Repository Interface:** `IReconciliationRepository.GetPagedLedgerAsync(...)`.
- **Target Tables:** `reconciliation_records` (INNER JOIN `orders`, `order_fee_snapshots`).
- **Complete Breakdown in `SettlementLedgerItem`:**
  - Fee breakdown fields: `commissionFee`, `paymentFee`, `serviceFee`, `fixedFee` (read directly from frozen historical `order_fee_snapshots` without invoking dynamic fee engine).
  - Financial reconciliation fields: `grossRevenue`, `totalPlatformFees`, `projectedSettlement`, `actualSettlement`, `varianceAmount`, `reconciliationStatus`, `deliveredAt`, `reconciledAt`.
- **RBAC Guard:** Restricted to `Finance Manager` and `Shop Owner`.

#### [17] `GET /settlements/summary`
- **Use Case Trace:** `UC05` (Settlement Reconciliation Counters).
- **UI Trigger:** `SettlementKPIHeader` (Audit Counter Cards).
- **Backend Invocation:** `SettlementController.GetSummary(from, to, channel)` $\rightarrow$ `ISettlementService.GetSummaryAsync(from, to, channel)`.
- **Repository Interface:** `IReconciliationRepository.GetSummaryCountersAsync(...)`.
- **Target Tables:** `reconciliation_records` (JOIN `orders`).
- **Filter Parity:** Accepts `from`, `to`, and `channel` query parameters matching `GET /settlements` so summary KPI cards reflect the exact same filter scope as the ledger table.
- **Summary Counters:** Returns `pendingSettlementCount`, `reconciledCount`, and `discrepancyCount`.

#### [18] `POST /settlements/{orderId}/reconcile`
- **Use Case Trace:** `UC06` (Record Manual Settlement & Reconcile Variance).
- **UI Trigger:** `RecordSettlementModal` Submit button.
- **Backend Invocation:** `SettlementController.Reconcile(orderId, request)` $\rightarrow$ `ISettlementService.ReconcileAsync(...)`.
- **Authoritative Reconciliation Logic:**
  1. Client sends: `actualSettlement` (`NonNegativeMonetaryAmount`) and optional/mandatory `notes`.
  2. Client **never sends** `varianceAmount` or `reconciliationStatus`.
  3. Backend derives variance:
     $$\text{varianceAmount} = \text{projectedSettlement} - \text{actualSettlement}$$
  4. If `varianceAmount == 0`:
     - Updates `reconciliation_records`: `status = 'RECONCILED'`, `reconciled_at = NOW()`, `reconciled_by = CurrentUser.Username`.
  5. If `varianceAmount != 0`:
     - Updates `reconciliation_records`: `status = 'DISCREPANCY'`, `reconciled_at = NOW()`, `reconciled_by = CurrentUser.Username`.
     - Non-empty `notes` is mandatory (HTTP 422 if empty).
     - System creates/updates discrepancy audit in `discrepancy_audits`:
       $$\text{variance} \ne 0 \longrightarrow \text{DISCREPANCY} \longrightarrow \text{requires explanation / root-cause classification}$$
       *(The backend flags the record for investigation and does not presume or auto-assign a specific root cause until classified by the auditor/user).*
  6. **Order status is NEVER altered:** The underlying order remains `DELIVERED`.
- **Target Tables:** `reconciliation_records`, `discrepancy_audits`.

---

### 3.5. Module 5: Discrepancy Auditing & Dispute Resolution (`/discrepancies`)

#### [19 - 20] `GET /discrepancies` & `GET /discrepancies/{id}`
- **Use Case Trace:** `UC07` (Audit Discrepancies & File Disputes).
- **UI Triggers:** `DiscrepancyPanel` (`DiscrepancyTable`), `DiscrepancyDetailDrawer`.
- **Backend Invocation:** `DiscrepanciesController` $\rightarrow$ `IDiscrepancyService` $\rightarrow$ `IDiscrepancyRepository`.
- **Target Tables:** `discrepancy_audits` (JOIN `reconciliation_records`, `orders`).
- **Resolution State:** Returns `isResolved` derived dynamically as `(resolvedAt != null)`.

#### [21] `PATCH /discrepancies/{id}/resolve`
- **Use Case Trace:** `UC07` (Audit Discrepancies & File Disputes).
- **UI Trigger:** `ResolveDiscrepancyModal` Confirm.
- **Backend Invocation:** `DiscrepanciesController.Resolve(id, request)` $\rightarrow$ `IDiscrepancyService.ResolveAsync(...)`.
- **Resolution Execution:**
  - Captures mandatory `resolutionNotes` (minimum 5 characters).
  - Sets `resolved_at = clock_timestamp()` and `resolved_by = CurrentUser.Username`.
  - The API response field `isResolved` evaluates to `true` (`resolvedAt != null`).
  - No fictitious DB column `is_resolved` is required; P05 canonical schema uses `resolved_at` and `resolved_by`.
- **Target Tables:** `discrepancy_audits`.

---

### 3.6. Module 6: Financial Analytics & Export (`/analytics`)

#### [22] `GET /analytics/kpis`
- **Use Case Trace:** `UC08` (Analyze Multi-Channel Contribution Profit & Margins), `UC13` (Track Margin Trends).
- **UI Trigger:** `ExecutiveKPIHeader` (5 KPI Cards).
- **Backend Invocation:** `AnalyticsController.GetKpis(from, to, channel)` $\rightarrow$ `IAnalyticsService.GetKpisAsync(...)`.
- **The 5 Core Financial KPIs:**
  1. `grossRevenue`: $\sum(\text{Gross Revenue})$ for `DELIVERED` orders only.
  2. `totalPlatformFees`: $\sum(\text{Total Platform Fees})$ from `order_fee_snapshots` for `DELIVERED` orders only.
  3. `projectedSettlement`: $\text{Gross Revenue} - \text{Total Platform Fees}$.
  4. `cogs`: $\sum(\text{Line Total Cost})$ from `order_items.total_cost` for `DELIVERED` orders only.
  5. `contributionProfit`: $\text{Projected Settlement} - \text{COGS}$.
  - Plus: `contributionMarginPct`: $(\text{Contribution Profit} / \text{Gross Revenue}) \times 100$.
- **Zero Phantom Revenue Invariant:** Orders with status `PENDING`, `SHIPPED`, or `CANCELLED` strictly contribute 0 VND.

#### [23] `GET /analytics/trend`
- **Use Case Trace:** `UC08`, `UC13`.
- **UI Trigger:** `RevenueProfitTrendChart`.
- **Metrics Plotted:** Daily time-series points of `Gross Revenue` vs. `Contribution Profit`.

#### [24] `GET /analytics/channel-breakdown`
- **Use Case Trace:** `UC10` (Compare Channel & SKU Profitability), `UC13`.
- **UI Trigger:** `ChannelBreakdownChart` (Donut / Bar).
- **Metrics Segmented:** Channel distribution across `TIKTOK`, `SHOPEE`, and `POS`.

#### [25] `GET /analytics/top-skus`
- **Use Case Trace:** `UC10`, `UC13`.
- **UI Trigger:** `TopSkuTable`.
- **Ranking Capabilities:** Sort by `CONTRIBUTION_PROFIT`, `GROSS_REVENUE`, or `DELIVERED_UNITS`.
- **Mathematical Allocation (Analytics Derivation):**
  $$\text{lineShare} = \frac{\text{lineSubtotal}}{\text{orderSubtotal}}$$
  $$\text{allocatedVoucher} = \text{orderVoucher} \times \text{lineShare}$$
  $$\text{lineGrossRevenue} = \text{lineSubtotal} - \text{allocatedVoucher}$$
  $$\text{allocatedPlatformFees} = \text{orderTotalPlatformFees} \times \text{lineShare}$$
  $$\text{lineContributionProfit} = \text{lineGrossRevenue} - \text{allocatedPlatformFees} - \text{lineCOGS}$$

#### [26] `GET /analytics/drilldown`
- **Use Case Trace:** `UC11` (Drilldown to Source Orders).
- **UI Trigger:** `DrilldownOrderModal` (triggered by clicking KPI cards or chart points).
- **Itemized Audit:** Returns paginated delivered order records backing the aggregated figures.

#### [27] `GET /analytics/export-csv`
- **Use Case Trace:** `UC11` (Export Financial & Settlement Reports).
- **UI Trigger:** `ExportCsvButton`.
- **Stream Format:** RFC 4180 CSV (`text/csv`) containing delivered orders, platform fee breakdowns, actual settlement, and reconciliation variance.

---

## 4. P05 Entity & Schema Cross-Verification

To ensure 100% fidelity to the canonical PostgreSQL schema, the following table confirms how P06 API operations map strictly to the 9 database tables defined in Phase P05:

| Canonical P05 Table | CRUD Operations in P06 | Architectural Integrity Rule |
|---|---|---|
| `products` | C, R, U (via `CatalogController`) | No `description` column in P05 schema; strictly omitted from API DTOs. Soft deactivation via `is_active`. |
| `product_variants` | C, R, U (via `CatalogController`) | Retail and cost prices enforced $\ge 0$ via `NonNegativeMonetaryAmount` and CHECK constraints. |
| `orders` | C, R, U (via `OrdersController`) | Contains `order_date` (`TIMESTAMPTZ`), returned as `orderDate` in DTOs. Zero net profit columns. |
| `order_items` | C, R (via `OrdersController`) | Frozen snapshot `unit_cost_snapshot` set upon creation; never editable by client. |
| `order_status_history` | C, R (via `OrdersController`) | Audit entries logged on lifecycle state transitions. |
| `fee_schedules` | C, R (via `FeeSchedulesController`, `DynamicFeeEngine`) | Rate versioning managed by Shop Owner (`POST /fee-schedules`). Read by Finance & Owner. |
| `order_fee_snapshots` | C, R (via `OrdersController`, `SettlementController`) | `order_fee_snapshots is immutable after creation by business/persistence policy` (no fictitious DB column `is_immutable`). |
| `reconciliation_records`| C, R, U (via `SettlementController`) | Manual payout entry; backend computes variance. Order status remains `DELIVERED`. |
| `discrepancy_audits` | C, R, U (via `DiscrepanciesController`) | `isResolved` derived as `resolvedAt != null` (no fictitious `is_resolved` column). Root cause requires user classification (no auto-assignment). |

> [!NOTE]
> **Eliminated Legacy Concepts:**
> The following legacy tables and concepts from older drafts have been completely purged from the P06 API specification:
> - `channels` table (replaced by canonical enum `ChannelType`: `TIKTOK`, `SHOPEE`, `POS`).
> - `statement_imports` and `statement_lines` tables (statement spreadsheet file parsing is out-of-scope for MVP).
> - `StatementParser` and SHA-256 upload deduplication.
> - Direct marketplace webhooks or background queue ingestion workers.
> - Fictitious columns (`is_immutable`, `is_resolved`, `description`).

---

## 5. Role-Based Access Control (RBAC) & Cost Privacy Matrix

| # | Endpoint Route | Operation ID | Sales & Ops Staff | Finance Manager | Shop Owner | Cost Leakage Guard |
|:---:|---|---|:---:|:---:|:---:|---|
| **01** | `GET /catalog/variants/selectable` | `getSelectableVariants` | ✅ | ❌ | ✅ | **`costPrice` strictly stripped from response.** |
| **02** | `GET /catalog/products` | `listProducts` | ❌ | ✅ | ✅ | Full catalog visibility including baseline cost. |
| **03** | `POST /catalog/products` | `createProduct` | ❌ | ✅ | ✅ | Product creation restricted to finance/owner. |
| **04** | `GET /catalog/products/{id}` | `getProductById` | ❌ | ✅ | ✅ | Product details with variant cost prices. |
| **05** | `PATCH /catalog/products/{id}` | `updateProduct` | ❌ | ✅ | ✅ | Master product updates. |
| **06** | `PATCH /catalog/variants/{id}` | `updateVariant` | ❌ | ✅ | ✅ | Baseline cost modification restricted. |
| **07** | `GET /orders` | `listOrders` | ✅ | ✅ | ✅ | **`OrderListItemResponse` strips all cost/profit data.** |
| **08** | `POST /orders` | `createOrder` | ✅ | ❌ | ✅ | Client enters prices; backend freezes snapshot. |
| **09** | `GET /orders/summary` | `getOrderSummary` | ✅ | ✅ | ✅ | Operational counters; gross revenue DELIVERED only. |
| **10** | `POST /orders/preview-fee` | `previewOrderFees` | ✅ | ✅ | ✅ | Real-time fee preview (no cost data shown). |
| **11** | `GET /orders/{id}` | `getOrderById` | ✅ | ✅ | ✅ | `unitCostSnapshot`, `cogs`, `profit` omitted for Sales. |
| **12** | `PATCH /orders/{id}/status` | `updateOrderStatus` | ✅ | ❌ | ✅ | Lifecycle advancement (`SHIPPED`, `DELIVERED`). |
| **13** | `POST /orders/{id}/cancel` | `cancelOrder` | ✅ | ❌ | ✅ | Active order cancellation with reason. |
| **14** | `GET /fee-schedules` | `listFeeSchedules` | ❌ | ✅ | ✅ | Fee schedule inspection restricted to finance/owner. |
| **15** | `POST /fee-schedules` | `createFeeSchedule` | ❌ | ❌ | ✅ | **Fee schedule versioning restricted strictly to Shop Owner.** |
| **16** | `GET /settlements` | `getSettlementLedger` | ❌ | ✅ | ✅ | Settlement ledger restricted to finance/owner. |
| **17** | `GET /settlements/summary` | `getSettlementSummary` | ❌ | ✅ | ✅ | Audit counters restricted to finance/owner. |
| **18** | `POST /settlements/{orderId}/reconcile` | `reconcileSettlement` | ❌ | ✅ | ✅ | Manual actual payout entry. |
| **19** | `GET /discrepancies` | `listDiscrepancies` | ❌ | ✅ | ✅ | Discrepancy investigation restricted. |
| **20** | `GET /discrepancies/{id}` | `getDiscrepancyById` | ❌ | ✅ | ✅ | Discrepancy investigation restricted. |
| **21** | `PATCH /discrepancies/{id}/resolve` | `resolveDiscrepancy` | ❌ | ✅ | ✅ | Discrepancy resolution restricted. |
| **22** | `GET /analytics/kpis` | `getFinancialKpis` | ❌ | ✅ | ✅ | Executive KPIs restricted to finance/owner. |
| **23** | `GET /analytics/trend` | `getFinancialTrend` | ❌ | ✅ | ✅ | Financial trend restricted to finance/owner. |
| **24** | `GET /analytics/channel-breakdown` | `getChannelBreakdown` | ❌ | ✅ | ✅ | Channel breakdown restricted to finance/owner. |
| **25** | `GET /analytics/top-skus` | `getTopSkus` | ❌ | ✅ | ✅ | Top merchandise performance restricted. |
| **26** | `GET /analytics/drilldown` | `getDrilldownOrders` | ❌ | ✅ | ✅ | Order drilldown restricted to finance/owner. |
| **27** | `GET /analytics/export-csv` | `exportReconciliationCsv` | ❌ | ✅ | ✅ | CSV export restricted to finance/owner. |
