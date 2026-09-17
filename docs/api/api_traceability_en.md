# 07 — API Traceability Matrix: End-to-End Architectural Traceability

---

## 1. Scope & Traceability Methodology

This document establishes the **100% end-to-end traceability matrix** for the FASHION-WEB platform. Every single endpoint defined in the OpenAPI 3.0 specification (`openapi_spec.yaml` / `openapi_spec_vi.yaml`) is mapped across all architectural tiers:

```
[UI Trigger / Modal]
       │
       ▼ (HTTP / JSON)
[ASP.NET Core Controller Action] (`FashionWeb.Api.Controllers`)
       │
       ▼ (DIP Interface Invocation)
[Domain Business Service] (`FashionWeb.Business.Services`)
       │
       ├─► [Strategy Engine / File Parser] (`FashionWeb.Business.Strategies / Parsers`)
       │
       ▼ (Data Access Interface)
[EF Core Repository] (`FashionWeb.Data.Repositories`)
       │
       ▼ (Relational Mapping)
[PostgreSQL Relational Tables] (`orders`, `snapshots`, `settlement`, etc.)
       │
       ▼ (Compliance Check)
[Financial Invariants & RBAC Guard Policies]
```

### Core Financial Rules & Invariants:
1. **Zero Phantom Revenue Invariant:** Revenue is recognized **if and only if** order status is `DELIVERED`. `PENDING`, `SHIPPED`, and `CANCELLED` orders strictly contribute **0 VND** to revenue.
2. **Exact Integer Arithmetic:** 100% currency fields use PostgreSQL `NUMERIC(18,0)` and C# `decimal` (no IEEE 754 floating-point numbers).
3. **Immutable Snapshot Guarantee:** Fee deductions are calculated and frozen into `order_fee_snapshots` with `is_immutable = true` upon delivery.
4. **Variance Invariant:** $\text{VarianceAmount} = \text{ActualSettledAmount} - \text{ExpectedNetPayout}$. Negative variance denotes merchant cash shortfall.
5. **Cryptographic Deduplication:** Statement files are hashed via `SHA-256` before parsing; duplicate file hashes are blocked with HTTP `409 Conflict`.
6. **Separation of Duties (RBAC):** Sales/Ops inputs orders; Finance files `#DIS-002` disputes; only `ShopOwner` / Executive has authority to `Approve` or `Reject` settlement variances.

---

## 2. Master Traceability Matrix (100% OpenAPI Coverage)

| # | HTTP & Route | Operation ID | UI Component / Modal | Controller Action | Service Interface & Method | Repository / Component | Target Entities / Tables | RBAC Policy | Invariant / Validation Guard |
|---|---|---|---|---|---|---|---|---|---|
| **01** | `POST /orders/preview-fee` | `previewOrderFees` | `CreateOrderModal` (`MOD-01`) | `OrdersController.PreviewFee` | `FeeStrategyFactory.GetStrategy` | `IPlatformFeeStrategy.CalculateFees` | *In-Memory Only* (No DB persistence) | `RequireSalesOrFinance` | $0 \le \text{Voucher} \le \text{Subtotal}$. Dynamic strategy calculation. |
| **02** | `GET /orders` | `listOrders` | `OrdersPage` / `OrdersTable` (`SCR-01`) | `OrdersController.GetOrders` | `IOrderService.GetOrdersAsync` | `IOrderRepository.GetPagedAsync` | `orders`, `order_items`, `order_fee_snapshots` | `RequireSalesOrFinance` | Filter by `channel`, `status`, search keyword. Default page size = 20. |
| **03** | `POST /orders` | `createOrder` | `CreateOrderModal` (`MOD-01`) | `OrdersController.CreateOrder` | `IOrderService.CreateOrderAsync` | `IOrderRepository.AddAsync` | `orders`, `order_items`, `order_fee_snapshots` (if POS) | `RequireSalesOrFinance` | POS transitions immediately to `DELIVERED` & freezes fee; TikTok/Shopee starts at `PENDING`. |
| **04** | `GET /orders/{id}` | `getOrderById` | `OrderDetailDrawer` (`SCR-01`) | `OrdersController.GetOrderById` | `IOrderService.GetOrderByIdAsync` | `IOrderRepository.GetByIdWithDetailsAsync` | `orders`, `order_items`, `order_fee_snapshots` | `RequireSalesOrFinance` | Returns HTTP `404` if UUID does not exist. Includes snapshot when `DELIVERED`. |
| **05** | `PATCH /orders/{id}/status` | `updateOrderStatus` | `OrdersTable` Actions (`SCR-01`) | `OrdersController.UpdateStatus` | `IOrderService.UpdateStatusAsync` | `IOrderRepository.UpdateAsync`, `FeeStrategyFactory` | `orders`, `order_fee_snapshots` | `RequireSalesOrFinance` | Strict state transitions: `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`. Direct `PENDING` $\rightarrow$ `DELIVERED` returns `409 Conflict`. Freeze snapshot on delivery. |
| **06** | `POST /orders/{id}/cancel` | `cancelOrder` | `CancelOrderModal` (`MOD-02`) | `OrdersController.CancelOrder` | `IOrderService.CancelOrderAsync` | `IOrderRepository.UpdateAsync` | `orders` | `RequireSalesOrFinance` | Non-empty `cancellation_reason` required. Cancelling `DELIVERED` order is blocked with `422 Unprocessable Entity`. |
| **07** | `GET /settlement/fee-schedules` | `listFeeSchedules` | `FeeScheduleModal` (`MOD-04`) | `SettlementController.GetFeeSchedules` | `ISettlementService.GetFeeSchedulesAsync` | `IFeeScheduleRepository.GetAllActiveAsync` | `fee_schedules`, `channels` | `RequireFinance` | Returns all active rate schedules for `TIKTOK`, `SHOPEE`, `POS`. |
| **08** | `PUT /settlement/fee-schedules` | `updateFeeSchedule` | `FeeScheduleModal` (`MOD-04`) | `SettlementController.UpdateFeeSchedule` | `ISettlementService.UpdateFeeScheduleAsync` | `IFeeScheduleRepository.UpdateAsync` | `fee_schedules` | `RequireShopOwner` | Shop Owner only. Validates rate boundaries: $0\% \le \text{rate} \le 100\%$, Fixed fee $\ge 0$. |
| **09** | `GET /settlement/ledger` | `getSettlementLedger` | `SettlementPage` / `LedgerTable` (`SCR-02`) | `SettlementController.GetLedger` | `ISettlementService.GetLedgerAsync` | `IReconciliationRepository.GetLedgerAsync` | `orders`, `order_fee_snapshots`, `reconciliation_records`, `statement_lines` | `RequireFinance` | Calculates `variance_amount = settled - expected`. Filters by `recon_status` and `channel`. |
| **10** | `GET /settlement/summary` | `getSettlementSummary` | `SettlementKPIHeader` (`SCR-02`) | `SettlementController.GetSummary` | `ISettlementService.GetSummaryAsync` | `IReconciliationRepository.GetSummaryAsync` | `reconciliation_records`, `orders` | `RequireFinance` | Aggregates 3 settlement badges: `PendingSettlement` (Grey), `Reconciled` (Green), `Discrepancy` (Red). |
| **11** | `POST /settlement/statements/import` | `importStatement` | `ImportStatementModal` (`MOD-03`) | `SettlementController.ImportStatement` | `IStatementImportService.ImportStatementAsync` | `IStatementImportRepository`, `IStatementParser`, `StatementMatchingService` | `statement_imports`, `statement_lines`, `reconciliation_records` | `RequireFinance` | Computes SHA-256; blocks duplicate files with `409 Conflict`. Completes 2-way matching in $\le 3.0$ seconds. |
| **12** | `GET /discrepancies` | `listDiscrepancies` | `DiscrepancyAuditDrawer` (`SCR-02`) | `DiscrepanciesController.GetDiscrepancies` | `IDiscrepancyService.GetDiscrepanciesAsync` | `IDiscrepancyRepository.GetPagedAsync` | `discrepancy_audits`, `reconciliation_records`, `orders` | `RequireFinance` | Filters by `approval_status` (`PENDING_APPROVAL`, `APPROVED`, `REJECTED`). |
| **13** | `POST /discrepancies` | `createDiscrepancyAudit` | `DiscrepancyModal` (`#DIS-002`) | `DiscrepanciesController.CreateAudit` | `IDiscrepancyService.CreateAuditAsync` | `IDiscrepancyRepository.AddAsync` | `discrepancy_audits`, `reconciliation_records` | `RequireFinance` | Auto-generates `DIS-2026-XXXX`. Mandatory root cause and evidence URL. Updates record to `PENDING_APPROVAL`. |
| **14** | `PATCH /discrepancies/{id}/approve` | `approveDiscrepancy` | `DiscrepancyApprovalDrawer` (`#DIS-002`) | `DiscrepanciesController.ApproveAudit` | `IDiscrepancyService.ApproveAuditAsync` | `IDiscrepancyRepository.UpdateAsync` | `discrepancy_audits`, `reconciliation_records` | `RequireShopOwner` | Shop Owner only. Decision: `APPROVED` (finalizes period) or `REJECTED` (remands for carrier dispute). |
| **15** | `GET /analytics/kpis` | `getAnalyticsKPIs` | `ExecutiveKPIHeader` (`SCR-03`) | `AnalyticsController.GetKpis` | `IAnalyticsService.GetKpisAsync` | `IAnalyticsRepository.GetExecutiveKpisAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | Aggregates 4 KPI cards. Strictly filters by `status = 'DELIVERED'` (Zero Phantom Revenue). |
| **16** | `GET /analytics/trend` | `getCashFlowTrend` | `CashFlowBarChart` (`SCR-03`) | `AnalyticsController.GetTrend` | `IAnalyticsService.GetDailyCashflowTrendAsync` | `IAnalyticsRepository.GetTrendAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | 7-day daily grouped comparison: Gross Revenue vs. Net Wallet Cash Inflow (`DELIVERED` only). |
| **17** | `GET /analytics/channel-breakdown` | `getChannelBreakdown` | `ChannelShareDonutChart` (`SCR-03`) | `AnalyticsController.GetChannelShare` | `IAnalyticsService.GetChannelShareAsync` | `IAnalyticsRepository.GetChannelShareAsync` | `orders` | `RequireExecutive` | Percentage and gross amount breakdown across `TIKTOK`, `SHOPEE`, and `POS` (`DELIVERED` only). |
| **18** | `GET /analytics/top-skus` | `getTopSKUs` | `TopProductsTable` (`SCR-03`) | `AnalyticsController.GetTopSkus` | `IAnalyticsService.GetTopSkusAsync` | `IAnalyticsRepository.GetTopSkusAsync` | `order_items`, `orders` | `RequireExecutive` | Top 5 SKUs ranked by delivered quantity and net revenue contribution (`DELIVERED` only). |
| **19** | `GET /analytics/drilldown` | `getDrilldownOrders` | `SourceOrderDrilldownModal` (`MOD-05`) | `AnalyticsController.GetDrilldownOrders` | `IAnalyticsService.GetDrilldownOrdersAsync` | `IAnalyticsRepository.GetDrilldownOrdersAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | Full itemized audit list of delivered orders backing the KPI aggregated amounts. |
| **20** | `GET /analytics/export-csv` | `exportReconciliationCSV` | `ExportButton` (`SCR-03` / `SCR-02`) | `AnalyticsController.ExportCsv` | `IAnalyticsService.ExportReconciliationCsvAsync` | `IAnalyticsRepository.GetExportDataAsync` | `orders`, `order_fee_snapshots`, `reconciliation_records` | `RequireExecutive` | Streams standardized RFC 4180 CSV bytes for tax/accounting filing. Media type: `text/csv`. |

---

## 3. Detailed Endpoint Breakdown by Module

### 3.1. Module 1: Orders & Real-time Fee Engine (`/orders`)

#### [01] `POST /orders/preview-fee`
- **Controller Action:** `OrdersController.PreviewFee([FromBody] FeePreviewRequest request)`
- **Service Invocation:** `FeeStrategyFactory.GetStrategy(request.ChannelCode).CalculateFees(request.Subtotal, request.ShopVoucher)`
- **Request DTO:**
  ```csharp
  public record FeePreviewRequest(
      [Required] string ChannelCode,
      [Range(0, 1_000_000_000)] decimal Subtotal,
      [Range(0, 1_000_000_000)] decimal ShopVoucher
  );
  ```
- **Response DTO:** `FeeBreakdownResponse` (Commission, PaymentFee, FreeshipExtra, FixedFee, ExpectedNetPayout)
- **Status Codes:** `200 OK`, `422 Unprocessable Entity` (Voucher > Subtotal).
- **Business Rule:** In-memory calculation; zero database reads/writes.

#### [02] `GET /orders`
- **Controller Action:** `OrdersController.GetOrders([FromQuery] string? channel, [FromQuery] string? status, [FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)`
- **Service Invocation:** `IOrderService.GetOrdersAsync(channel, status, search, page, pageSize)`
- **Repository Method:** `IOrderRepository.GetPagedAsync(spec)`
- **Response DTO:** `PagedResult<OrderListItemDto>`
- **Status Codes:** `200 OK`, `400 Bad Request`.
- **Business Rule:** Dynamic EF Core `IQueryable` filter; eagerly loads `OrderItems` and `OrderFeeSnapshot`.

#### [03] `POST /orders`
- **Controller Action:** `OrdersController.CreateOrder([FromBody] CreateOrderRequest request)`
- **Service Invocation:** `IOrderService.CreateOrderAsync(request)`
- **Repository Method:** `IOrderRepository.AddAsync(order)`
- **Target Tables:** `orders`, `order_items`, `order_fee_snapshots` (conditional)
- **Status Codes:** `201 Created` (`Location: /api/v1/orders/{id}`), `400 Bad Request`, `422 Unprocessable Entity`.
- **Business Rule:** 
  - If `ChannelCode == "POS"`, status initializes to `DELIVERED`, `OrderFeeSnapshot` is calculated and frozen immediately.
  - If `ChannelCode in ("TIKTOK", "SHOPEE")`, status initializes to `PENDING` (recognized revenue = 0 VND).

#### [04] `GET /orders/{id}`
- **Controller Action:** `OrdersController.GetOrderById(Guid id)`
- **Service Invocation:** `IOrderService.GetOrderByIdAsync(id)`
- **Repository Method:** `IOrderRepository.GetByIdWithDetailsAsync(id)`
- **Response DTO:** `OrderDetailResponse`
- **Status Codes:** `200 OK`, `404 Not Found`.

#### [05] `PATCH /orders/{id}/status`
- **Controller Action:** `OrdersController.UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)`
- **Service Invocation:** `IOrderService.UpdateStatusAsync(id, request.Status)`
- **Target Tables:** `orders`, `order_fee_snapshots`
- **Status Codes:** `200 OK`, `409 Conflict` (Illegal state jump), `422 Unprocessable Entity`.
- **Business Rule:** 
  - `PENDING` $\rightarrow$ `SHIPPED`: Courier handover recorded. Revenue remains 0 VND.
  - `SHIPPED` $\rightarrow$ `DELIVERED`: Triggers `FeeStrategyFactory`, computes all fees, and inserts `OrderFeeSnapshot` with `IsImmutable = true`. Official revenue recognized.
  - Transition from `PENDING` directly to `DELIVERED` returns `409 Conflict`.

#### [06] `POST /orders/{id}/cancel`
- **Controller Action:** `OrdersController.CancelOrder(Guid id, [FromBody] CancelOrderRequest request)`
- **Service Invocation:** `IOrderService.CancelOrderAsync(id, request.Reason)`
- **Target Tables:** `orders`
- **Status Codes:** `200 OK`, `422 Unprocessable Entity`.
- **Business Rule:** 
  - Allowed from `PENDING` or `SHIPPED`.
  - Strictly blocked if order is `DELIVERED` (`422 Unprocessable Entity`).
  - Recorded cancellation reason is required; revenue excluded 100%.

---

### 3.2. Module 2: Fee Schedule Management (`/settlement/fee-schedules`)

#### [07] `GET /settlement/fee-schedules`
- **Controller Action:** `SettlementController.GetFeeSchedules()`
- **Service Invocation:** `ISettlementService.GetFeeSchedulesAsync()`
- **Repository Method:** `IFeeScheduleRepository.GetAllActiveAsync()`
- **Target Tables:** `fee_schedules`, `channels`
- **Response DTO:** `List<FeeScheduleDto>`
- **Status Codes:** `200 OK`, `401 Unauthorized`.
- **Business Rule:** Returns active rate schedules used by Strategy Engine for TikTok, Shopee, and POS.

#### [08] `PUT /settlement/fee-schedules`
- **Controller Action:** `SettlementController.UpdateFeeSchedule([FromBody] UpdateFeeScheduleRequest request)`
- **Service Invocation:** `ISettlementService.UpdateFeeScheduleAsync(request)`
- **Repository Method:** `IFeeScheduleRepository.UpdateAsync(schedule)`
- **Target Tables:** `fee_schedules`
- **Response DTO:** `FeeScheduleDto`
- **Status Codes:** `200 OK`, `400 Bad Request`, `403 Forbidden` (`RequireShopOwner`).
- **Business Rule:** Restricted to `ShopOwner`. Inactivates previous schedule and inserts new version to preserve historical audit trail.

---

### 3.3. Module 3: Wallet Settlement & Automated Reconciliation (`/settlement`)

#### [09] `GET /settlement/ledger`
- **Controller Action:** `SettlementController.GetLedger([FromQuery] string? channel, [FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)`
- **Service Invocation:** `ISettlementService.GetLedgerAsync(channel, status, page, pageSize)`
- **Repository Method:** `IReconciliationRepository.GetLedgerAsync(spec)`
- **Target Tables:** `orders`, `order_fee_snapshots`, `reconciliation_records`, `statement_lines`
- **Response DTO:** `PagedResult<SettlementLedgerItemDto>`
- **Status Codes:** `200 OK`, `401 Unauthorized`.
- **Business Rule:** Computes `VarianceAmount = ActualSettledAmount - ExpectedNetPayout`. Displays badge: Green (0 VND), Red (Shortfall).

#### [10] `GET /settlement/summary`
- **Controller Action:** `SettlementController.GetSummary()`
- **Service Invocation:** `ISettlementService.GetSummaryAsync()`
- **Repository Method:** `IReconciliationRepository.GetSummaryAsync()`
- **Response DTO:** `SettlementSummaryResponse` (PendingSettlementCount/Amount, ReconciledCount/Amount, DiscrepancyCount/Amount)
- **Status Codes:** `200 OK`.

#### [11] `POST /settlement/statements/import`
- **Controller Action:** `SettlementController.ImportStatement([FromForm] IFormFile file, [FromForm] string channelCode)`
- **Service Invocation:** `IStatementImportService.ImportStatementAsync(stream, file.FileName, channelCode)`
- **Underlying Components:** `SHA256CryptoServiceProvider`, `IStatementParserFactory`, `ClosedXMLParser`, `StatementMatchingService`
- **Target Tables:** `statement_imports`, `statement_lines`, `reconciliation_records`
- **Response DTO:** `StatementImportResultDto` (TotalLines, MatchedCount, DiscrepancyCount, ExecutionTimeMs)
- **Status Codes:** `201 Created`, `400 Bad Request`, `409 Conflict` (Duplicate SHA-256 hash).
- **Business Rule:** Computes SHA-256 hash of stream. If hash exists in `statement_imports`, aborts with `409 Conflict`. Performs 2-way reconciliation matching in $\le 3.0$ seconds.

---

### 3.4. Module 4: Discrepancy Auditing & Executive Sign-off (`/discrepancies`)

#### [12] `GET /discrepancies`
- **Controller Action:** `DiscrepanciesController.GetDiscrepancies([FromQuery] string? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)`
- **Service Invocation:** `IDiscrepancyService.GetDiscrepanciesAsync(status, page, pageSize)`
- **Repository Method:** `IDiscrepancyRepository.GetPagedAsync(spec)`
- **Target Tables:** `discrepancy_audits`, `reconciliation_records`, `orders`
- **Response DTO:** `PagedResult<DiscrepancyAuditDto>`
- **Status Codes:** `200 OK`.

#### [13] `POST /discrepancies`
- **Controller Action:** `DiscrepanciesController.CreateAudit([FromBody] CreateDiscrepancyRequest request)`
- **Service Invocation:** `IDiscrepancyService.CreateAuditAsync(request)`
- **Repository Method:** `IDiscrepancyRepository.AddAsync(audit)`
- **Target Tables:** `discrepancy_audits`, `reconciliation_records`
- **Response DTO:** `DiscrepancyAuditDto`
- **Status Codes:** `201 Created`, `400 Bad Request`, `404 Not Found`.
- **Business Rule:** Generated case code `DIS-2026-XXXX`. Mandates RootCause category and EvidenceURL. Transitions reconciliation status to `PENDING_APPROVAL`.

#### [14] `PATCH /discrepancies/{id}/approve`
- **Controller Action:** `DiscrepanciesController.ApproveAudit(Guid id, [FromBody] ApproveDiscrepancyRequest request)`
- **Service Invocation:** `IDiscrepancyService.ApproveAuditAsync(id, currentUserId, request.Decision, request.ApproverNote)`
- **Repository Method:** `IDiscrepancyRepository.UpdateAsync(audit)`
- **Target Tables:** `discrepancy_audits`, `reconciliation_records`
- **Response DTO:** `DiscrepancyAuditDto`
- **Status Codes:** `200 OK`, `403 Forbidden` (`RequireShopOwner`), `404 Not Found`.
- **Business Rule:** RBAC check: **Only Shop Owner**. `APPROVED` accepts variance as legitimate business surcharge and locks period. `REJECTED` remands case for carrier dispute.

---

### 3.5. Module 5: Executive Analytics & Financial Reporting (`/analytics`)

#### [15] `GET /analytics/kpis`
- **Controller Action:** `AnalyticsController.GetKpis([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? channel)`
- **Service Invocation:** `IAnalyticsService.GetKpisAsync(fromDate, toDate, channel)`
- **Repository Method:** `IAnalyticsRepository.GetExecutiveKpisAsync(spec)`
- **Target Tables:** `orders`, `order_fee_snapshots`
- **Response DTO:** `ExecutiveKPIsResponse` (GrossRevenue, PlatformFees, NetTakeHome, DeliveredOrderCount)
- **Status Codes:** `200 OK`.
- **Business Rule:** **Zero Phantom Revenue Invariant:** `WHERE orders.status = 'DELIVERED'`. Orders with `PENDING`, `SHIPPED`, or `CANCELLED` are filtered out.

#### [16] `GET /analytics/trend`
- **Controller Action:** `AnalyticsController.GetTrend([FromQuery] int days = 7)`
- **Service Invocation:** `IAnalyticsService.GetDailyCashflowTrendAsync(days)`
- **Repository Method:** `IAnalyticsRepository.GetTrendAsync(days)`
- **Target Tables:** `orders`, `order_fee_snapshots`
- **Response DTO:** `List<DailyTrendPoint>` (Date, GrossRevenue, NetCashflow, PlatformFees)
- **Status Codes:** `200 OK`.
- **Business Rule:** Daily grouped time-series for delivered orders.

#### [17] `GET /analytics/channel-breakdown`
- **Controller Action:** `AnalyticsController.GetChannelShare([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)`
- **Service Invocation:** `IAnalyticsService.GetChannelShareAsync(fromDate, toDate)`
- **Repository Method:** `IAnalyticsRepository.GetChannelShareAsync(fromDate, toDate)`
- **Target Tables:** `orders`
- **Response DTO:** `List<ChannelBreakdownDto>` (ChannelCode, GrossRevenue, Percentage)
- **Status Codes:** `200 OK`.

#### [18] `GET /analytics/top-skus`
- **Controller Action:** `AnalyticsController.GetTopSkus([FromQuery] int limit = 5, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)`
- **Service Invocation:** `IAnalyticsService.GetTopSkusAsync(limit, fromDate, toDate)`
- **Repository Method:** `IAnalyticsRepository.GetTopSkusAsync(limit, fromDate, toDate)`
- **Target Tables:** `order_items`, `orders`
- **Response DTO:** `List<TopSkuDto>` (SkuCode, ProductName, UnitsSold, TotalRevenue)
- **Status Codes:** `200 OK`.
- **Business Rule:** Joined with `orders` where `status = 'DELIVERED'`.

#### [19] `GET /analytics/drilldown`
- **Controller Action:** `AnalyticsController.GetDrilldownOrders([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate, [FromQuery] string? channel)`
- **Service Invocation:** `IAnalyticsService.GetDrilldownOrdersAsync(fromDate, toDate, channel)`
- **Repository Method:** `IAnalyticsRepository.GetDrilldownOrdersAsync(spec)`
- **Target Tables:** `orders`, `order_fee_snapshots`
- **Response DTO:** `List<DrilldownOrderDto>`
- **Status Codes:** `200 OK`.
- **Business Rule:** Line-item audit drilldown modal (`MOD-05`) verifying the exact orders that constitute the aggregated KPI numbers.

#### [20] `GET /analytics/export-csv`
- **Controller Action:** `AnalyticsController.ExportCsv([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)`
- **Service Invocation:** `IAnalyticsService.ExportReconciliationCsvAsync(fromDate, toDate)`
- **Repository Method:** `IAnalyticsRepository.GetExportDataAsync(fromDate, toDate)`
- **Response Type:** `FileContentResult` (`text/csv`, `Reconciliation_Report_{yyyyMMdd}.csv`)
- **Status Codes:** `200 OK`.
- **Business Rule:** Streams RFC 4180 compliant CSV bytes with UTF-8 BOM encoding for direct Excel compatibility.

---

## 4. Role-Based Access Control (RBAC) Enforcement Matrix

| API Route Group | Minimal Role Required | Sales & Ops (`SalesOps`) | Finance Accountant (`Finance`) | Shop Owner (`ShopOwner`) | Unauthorized Behavior |
|---|---|:---:|:---:|:---:|---|
| `/orders/*` | `SalesOps` | ✅ Read/Write | ✅ Read Only | ✅ Full Access | `401 Unauthorized` |
| `/settlement/ledger` | `Finance` | ❌ No Access | ✅ Read Only | ✅ Full Access | `403 Forbidden` |
| `/settlement/statements/import` | `Finance` | ❌ No Access | ✅ Upload & Match | ✅ Full Access | `403 Forbidden` |
| `/settlement/fee-schedules` (GET) | `Finance` | ❌ No Access | ✅ Read Only | ✅ Full Access | `403 Forbidden` |
| `/settlement/fee-schedules` (PUT) | `ShopOwner` | ❌ No Access | ❌ No Access | ✅ Update Rates | `403 Forbidden` |
| `/discrepancies` (GET / POST) | `Finance` | ❌ No Access | ✅ File Case | ✅ Full Access | `403 Forbidden` |
| `/discrepancies/{id}/approve` | `ShopOwner` | ❌ No Access | ❌ No Access | ✅ Approve/Reject | `403 Forbidden` |
| `/analytics/*` | `Finance` | ❌ No Access | ✅ Read Only | ✅ Full Access | `403 Forbidden` |

---

## 5. Entity Framework Core Relational Mapping Cross-Reference

| EF Core Entity Class (`FashionWeb.Data.Entities`) | PostgreSQL Table | Primary Key | Foreign Keys | Key Audited Columns |
|---|---|---|---|---|
| `OrderEntity` | `orders` | `id` (UUID) | `channel_code` $\rightarrow$ `channels.channel_code` | `status`, `gross_subtotal`, `shop_voucher`, `customer_paid`, `cancellation_reason` |
| `OrderItemEntity` | `order_items` | `id` (UUID) | `order_id` $\rightarrow$ `orders.id` | `sku_code`, `product_name`, `quantity`, `unit_price`, `line_total` |
| `OrderFeeSnapshotEntity` | `order_fee_snapshots` | `id` (UUID) | `order_id` $\rightarrow$ `orders.id`, `fee_schedule_id` $\rightarrow$ `fee_schedules.id` | `commission_fee`, `payment_processing_fee`, `service_freeship_fee`, `fixed_platform_fee`, `expected_net_payout`, `is_immutable` |
| `FeeScheduleEntity` | `fee_schedules` | `id` (UUID) | `channel_code` $\rightarrow$ `channels.channel_code` | `commission_rate`, `payment_fee_rate`, `fixed_fee_per_order`, `freeship_extra_rate`, `effective_from`, `is_current` |
| `StatementImportEntity` | `statement_imports` | `id` (UUID) | `channel_code` $\rightarrow$ `channels.channel_code` | `file_name`, `file_hash` (Unique SHA-256), `total_lines`, `total_settled_amount`, `uploaded_by` |
| `StatementLineEntity` | `statement_lines` | `id` (UUID) | `import_id` $\rightarrow$ `statement_imports.id` | `external_order_id`, `transaction_date`, `settled_amount`, `line_status` |
| `ReconciliationRecordEntity` | `reconciliation_records` | `id` (UUID) | `order_id` $\rightarrow$ `orders.id`, `statement_line_id` $\rightarrow$ `statement_lines.id` | `expected_amount`, `actual_settled_amount`, `variance_amount`, `recon_status` |
| `DiscrepancyAuditEntity` | `discrepancy_audits` | `id` (UUID) | `reconciliation_id` $\rightarrow$ `reconciliation_records.id` | `dispute_code`, `discrepancy_value`, `root_cause_category`, `justification_note`, `evidence_file_url`, `approval_status`, `approved_by` |

---

## 6. Definition of Done & Quality Gate

This API Traceability specification satisfies all conditions for **Milestone 7**:
- [x] 100% of the 20 OpenAPI endpoints mapped to Controllers, Services, Repositories, and PostgreSQL Tables.
- [x] Complete alignment with the 3-Tier .NET architecture (`Api` $\rightarrow$ `Business` $\rightarrow$ `Data`).
- [x] Explicit enforcement of the **Zero Phantom Revenue Invariant** across Orders and Analytics.
- [x] Clear RBAC authorization rules distinguishing Sales/Ops, Finance, and Shop Owner roles.
- [x] Strict validation guard conditions and error responses documented for every endpoint.
