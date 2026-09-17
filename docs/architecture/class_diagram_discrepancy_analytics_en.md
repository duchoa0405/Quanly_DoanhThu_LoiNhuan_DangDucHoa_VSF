# 04 — Class Diagrams: Discrepancy Audit & Analytics Reporting

## 1. Architectural Scope & Decomposition

This module provides financial control governance and executive decision-making intelligence:
1. **Discrepancy Audit Workflow (#DIS-002):** Enables finance accountants to justify cash shortfalls (e.g., carrier weight penalties or hidden marketplace surcharges) and mandates executive board / shop owner approval.
2. **Executive Analytics & Reporting:** Aggregates real-time financial metrics, 7-day cash flow trends, channel revenue shares, and top SKU leaderboards with CSV export capabilities.

To maintain visual clarity, this bounded context is decomposed into **3 focused sub-diagrams**:
1. **Diagram 4.1 — Discrepancy Audit & Approval 3-Tier Workflow:** Creation, file evidence attachment, and RBAC-governed approval flow.
2. **Diagram 4.2 — Executive Analytics & Reporting Service Flow:** KPI metric computation, trend charting, drilldown tracking, and CSV streaming.
3. **Diagram 4.3 — Audit Domain Entities & Analytical Aggregation Model:** Entity relationships, status enumerations, and financial invariants.

---

## 2. Diagram 4.1: Discrepancy Audit & Approval 3-Tier Workflow

This diagram models the dispute resolution lifecycle where finance managers log audit claims and shop owners make binding approval decisions:

```mermaid
classDiagram
    direction TB

    class DiscrepanciesController {
        <<Controller>>
        -IDiscrepancyService _discrepancyService
        +GetDiscrepancies(string status) Task~ActionResult~
        +CreateAudit(CreateDiscrepancyRequest request) Task~ActionResult~
        +Approve(Guid id, ApproveDiscrepancyRequest request) Task~ActionResult~
    }

    class CreateDiscrepancyRequest {
        <<DTO Request>>
        +Guid ReconciliationRecordId
        +string RootCauseCategory
        +string MerchantExplanation
        +string EvidenceAttachmentUrl
    }

    class ApproveDiscrepancyRequest {
        <<DTO Request>>
        +string Decision
        +string ApproverNote
    }

    class DiscrepancyAuditDTO {
        <<DTO Response>>
        +Guid Id
        +string DiscrepancyCode
        +Guid ReconciliationRecordId
        +string RootCauseCategory
        +string MerchantExplanation
        +string EvidenceAttachmentUrl
        +string Status
        +string CreatedBy
        +string ReviewedBy
        +DateTime ReviewedAt
        +string ReviewerNote
    }

    class IDiscrepancyService {
        <<Interface>>
        +GetDiscrepanciesAsync(string status) Task~List~DiscrepancyAuditDTO~~
        +CreateAuditAsync(CreateDiscrepancyRequest request, string createdBy) Task~DiscrepancyAuditDTO~
        +ApproveDiscrepancyAsync(Guid id, ApproveDiscrepancyRequest request, string reviewer) Task~DiscrepancyAuditDTO~
    }

    class DiscrepancyService {
        <<Business Service>>
        -IDiscrepancyRepository _discrepancyRepo
        -IReconciliationRepository _reconRepo
        +GetDiscrepanciesAsync(string status) Task~List~DiscrepancyAuditDTO~~
        +CreateAuditAsync(CreateDiscrepancyRequest request, string createdBy) Task~DiscrepancyAuditDTO~
        +ApproveDiscrepancyAsync(Guid id, ApproveDiscrepancyRequest request, string reviewer) Task~DiscrepancyAuditDTO~
    }

    class IDiscrepancyRepository {
        <<Repository Interface>>
        +GetByIdAsync(Guid id) Task~DiscrepancyAudit~
        +GetByReconciliationRecordIdAsync(Guid reconId) Task~DiscrepancyAudit~
        +GetAllAsync(string status) Task~List~DiscrepancyAudit~~
        +AddAsync(DiscrepancyAudit audit) Task
        +UpdateAsync(DiscrepancyAudit audit) Task
        +SaveChangesAsync() Task
    }

    class DiscrepancyRepository {
        <<Repository Implementation>>
        -AppDbContext _context
        +GetByIdAsync(Guid id) Task~DiscrepancyAudit~
        +GetByReconciliationRecordIdAsync(Guid reconId) Task~DiscrepancyAudit~
        +GetAllAsync(string status) Task~List~DiscrepancyAudit~~
        +AddAsync(DiscrepancyAudit audit) Task
        +UpdateAsync(DiscrepancyAudit audit) Task
        +SaveChangesAsync() Task
    }

    class AppDbContext {
        <<EF Core DbContext>>
        +DbSet~DiscrepancyAudit~ DiscrepancyAudits
        +DbSet~ReconciliationRecord~ ReconciliationRecords
        +SaveChangesAsync() Task~int~
    }

    DiscrepanciesController ..> IDiscrepancyService : calls
    DiscrepanciesController ..> CreateDiscrepancyRequest : consumes
    DiscrepanciesController ..> DiscrepancyAuditDTO : produces

    IDiscrepancyService <|.. DiscrepancyService : implements
    DiscrepancyService --> IDiscrepancyRepository : persists via
    IDiscrepancyRepository <|.. DiscrepancyRepository : implements
    DiscrepancyRepository --> AppDbContext : executes EF Core queries
```

---

## 3. Diagram 4.2: Executive Analytics & Reporting Service Flow

This diagram illustrates how analytical queries strictly aggregate `DELIVERED` orders to prevent phantom revenue reporting:

```mermaid
classDiagram
    direction TB

    class AnalyticsController {
        <<Controller>>
        -IAnalyticsService _analyticsService
        +GetKPIs(DateTime fromDate, DateTime toDate, string channel) Task~ActionResult~
        +GetCashFlowTrend(int days) Task~ActionResult~
        +GetChannelBreakdown(DateTime fromDate, DateTime toDate) Task~ActionResult~
        +GetTopSKUs(int limit) Task~ActionResult~
        +GetDrilldownOrders(DateTime fromDate, DateTime toDate, string channel, int page, int limit) Task~ActionResult~
        +ExportCsv() Task~IActionResult~
    }

    class ExecutiveKPIsResponse {
        <<DTO Response>>
        +decimal TotalGrossRevenue
        +decimal TotalPlatformFees
        +decimal TotalNetPayout
        +int TotalDeliveredOrders
    }

    class DailyTrendPoint {
        <<DTO Response>>
        +string Date
        +decimal GrossRevenue
        +decimal NetPayout
    }

    class ChannelBreakdownResponse {
        <<DTO Response>>
        +string ChannelCode
        +decimal Revenue
        +decimal PercentageShare
    }

    class TopSkuItemResponse {
        <<DTO Response>>
        +string SkuCode
        +string ProductName
        +int UnitsSold
        +decimal TotalRevenue
    }

    class IAnalyticsService {
        <<Interface>>
        +GetKPIsAsync(DateTime fromDate, DateTime toDate, string channel) Task~ExecutiveKPIsResponse~
        +GetCashFlowTrendAsync(int days) Task~List~DailyTrendPoint~~
        +GetChannelBreakdownAsync(DateTime fromDate, DateTime toDate) Task~List~ChannelBreakdownResponse~~
        +GetTopSKUsAsync(int limit) Task~List~TopSkuItemResponse~~
        +ExportReconciliationCsvAsync() Task~byte[]~
    }

    class AnalyticsService {
        <<Business Service>>
        -IAnalyticsRepository _analyticsRepo
        +GetKPIsAsync(DateTime fromDate, DateTime toDate, string channel) Task~ExecutiveKPIsResponse~
        +GetCashFlowTrendAsync(int days) Task~List~DailyTrendPoint~~
        +GetChannelBreakdownAsync(DateTime fromDate, DateTime toDate) Task~List~ChannelBreakdownResponse~~
        +GetTopSKUsAsync(int limit) Task~List~TopSkuItemResponse~~
        +ExportReconciliationCsvAsync() Task~byte[]~
    }

    class IAnalyticsRepository {
        <<Repository Interface>>
        +GetDeliveredOrdersAggregateAsync(DateTime from, DateTime to, string channel) Task~KpiAggregateData~
        +GetDailyCashflowHistoryAsync(int days) Task~List~DailyTrendData~~
        +GetChannelRevenueDistributionAsync(DateTime from, DateTime to) Task~List~ChannelData~~
        +GetTopSellingSkusAsync(int limit) Task~List~SkuSalesData~~
    }

    AnalyticsController ..> IAnalyticsService : calls
    AnalyticsController ..> ExecutiveKPIsResponse : produces
    AnalyticsController ..> DailyTrendPoint : produces
    IAnalyticsService <|.. AnalyticsService : implements
    AnalyticsService --> IAnalyticsRepository : queries metrics via
```

---

## 4. Diagram 4.3: Audit Domain Entities & Analytical Aggregation Model

This diagram models the entities governing variance justifications and financial aggregation constraints:

```mermaid
classDiagram
    direction TB

    class DiscrepancyAudit {
        <<Aggregate Root - Audit>>
        +Guid Id
        +string DiscrepancyCode
        +Guid ReconciliationRecordId
        +DiscrepancyRootCause RootCauseCategory
        +string MerchantExplanation
        +string EvidenceAttachmentUrl
        +ApprovalStatus Status
        +string CreatedBy
        +string ReviewedBy
        +DateTime ReviewedAt
        +string ReviewerNote
        +Approve(string reviewer, string note) void
        +Reject(string reviewer, string note) void
    }

    class ReconciliationRecord {
        <<Domain Entity>>
        +Guid Id
        +Guid OrderId
        +Guid StatementLineId
        +decimal ExpectedNetPayout
        +decimal ActualSettledAmount
        +decimal VarianceAmount
        +ReconciliationRecordStatus Status
        +DiscrepancyAudit AuditCase
    }

    class DiscrepancyRootCause {
        <<Enumeration>>
        WeightSurcharge
        CommissionMismatch
        LostPackage
        Other
    }

    class ApprovalStatus {
        <<Enumeration>>
        PendingApproval
        Approved
        Rejected
    }

    class Order {
        <<Aggregate Root>>
        +Guid Id
        +OrderStatus Status
        +decimal GrossSubtotal
        +decimal ShopVoucher
        +decimal CustomerPaid
        +DateTime DeliveredAt
        +OrderFeeSnapshot FeeSnapshot
        +IsDelivered() bool
    }

    class OrderFeeSnapshot {
        <<Immutable Financial Snapshot>>
        +Guid Id
        +decimal TotalPlatformFees
        +decimal ExpectedNetPayout
    }

    ReconciliationRecord "1" o-- "0..1" DiscrepancyAudit : justified by
    DiscrepancyAudit ..> DiscrepancyRootCause : categorized as
    DiscrepancyAudit ..> ApprovalStatus : tracks decision
    Order "1" *-- "1" OrderFeeSnapshot : freezes when Delivered
    ReconciliationRecord ..> Order : references
```

---

## 5. Architectural & Financial Invariants Summary

1. **Mandatory Justification Rule for Variances:**
   * Whenever `ReconciliationRecord.VarianceAmount != 0`, creating a `DiscrepancyAudit` case is mandatory to close accounting periods.
   * Root causes are formally categorized into: `WeightSurcharge` (carrier dimensional re-weighing), `CommissionMismatch` (unexpected platform policy deduction), `LostPackage`, or `Other`.
2. **Separation of Duties (RBAC Compliance):**
   * **Sales/Ops:** No access to Discrepancy or Analytics modules.
   * **Finance Manager:** Can file dispute justifications (`CreateAuditAsync`) and view ledger metrics.
   * **Shop Owner / Executive Board:** Holds exclusive authority to execute `Approve` or `Reject` actions on audit cases.
3. **Zero Phantom Revenue Accounting Rule:**
   * In `AnalyticsRepository`, all SQL aggregate queries strictly filter:
     ```sql
     WHERE status = 'DELIVERED'
     ```
   * Orders in `PENDING`, `SHIPPED`, or `CANCELLED` states are 100% excluded, contributing exactly **0 VND** to revenue cards or trend graphs.
4. **CSV Export Streaming:**
   * `ExportReconciliationCsvAsync()` streams detailed audit trails directly from the database into `text/csv` bytes without loading entire historical tables into memory.
