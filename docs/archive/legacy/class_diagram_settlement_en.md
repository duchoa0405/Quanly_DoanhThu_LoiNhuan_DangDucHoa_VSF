# 03 — Class Diagrams: Settlement & Reconciliation

## 1. Architectural Scope & Decomposition

This module unbundles platform deductions, ingests bank/marketplace payout spreadsheets (`.xlsx`/`.csv`), performs automated 2-way matching against internal delivery snapshots, and audits discrepancy variances.

To maintain visual clarity, this bounded context is decomposed into **3 focused sub-diagrams**:
1. **Diagram 3.1 — 3-Tier Settlement Ledger & Summary Orchestration:** Querying the settlement ledger, KPI summary cards, and active fee schedules.
2. **Diagram 3.2 — Statement Ingestion, Parsing & Automated Matching Engine:** Multipart upload, SHA-256 cryptographic deduplication, spreadsheet parsing, and two-way reconciliation algorithm.
3. **Diagram 3.3 — Domain Entities & Reconciliation State Model:** Relational schema, variance calculation invariant, and audit traceability.

---

## 2. Diagram 3.1: 3-Tier Settlement Ledger & Summary Orchestration

This diagram illustrates how finance managers query itemized deductions, net cash expectations, and KPI summary counters:

```mermaid
classDiagram
    direction TB

    class SettlementController {
        <<Controller>>
        -ISettlementService _settlementService
        +GetLedger(string channel, string reconStatus, int page, int limit) Task~ActionResult~
        +GetSummary() Task~ActionResult~
        +GetFeeSchedules() Task~ActionResult~
        +UpdateFeeSchedule(UpdateFeeScheduleRequest request) Task~ActionResult~
    }

    class SettlementSummaryResponse {
        <<DTO Response>>
        +int PendingCount
        +decimal PendingAmount
        +int ReconciledCount
        +decimal ReconciledAmount
        +int DiscrepancyCount
        +decimal DiscrepancyAmount
    }

    class SettlementLedgerResponse {
        <<DTO Response>>
        +int TotalItems
        +int Page
        +int PageSize
        +List~SettlementLedgerItemResponse~ Items
    }

    class SettlementLedgerItemResponse {
        <<DTO Response>>
        +Guid OrderId
        +string OrderCode
        +string ExternalOrderId
        +string ChannelCode
        +decimal GrossRevenue
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal TotalPlatformFees
        +decimal ProjectedNetPayout
        +decimal ActualSettledAmount
        +decimal VarianceAmount
        +string ReconciliationStatus
    }

    class ISettlementService {
        <<Interface>>
        +GetLedgerAsync(string channel, string status, int page, int pageSize) Task~PaginatedResult~
        +GetSummaryAsync() Task~SettlementSummaryResponse~
        +GetFeeSchedulesAsync() Task~List~
        +UpdateFeeScheduleAsync(UpdateFeeScheduleRequest request) Task
    }

    class SettlementService {
        <<Business Service>>
        -IReconciliationRepository _reconRepo
        -IFeeScheduleRepository _feeScheduleRepo
        +GetLedgerAsync(string channel, string status, int page, int pageSize) Task~PaginatedResult~
        +GetSummaryAsync() Task~SettlementSummaryResponse~
        +GetFeeSchedulesAsync() Task~List~
        +UpdateFeeScheduleAsync(UpdateFeeScheduleRequest request) Task
    }

    class IReconciliationRepository {
        <<Repository Interface>>
        +GetLedgerAsync(string channel, string status, int page, int pageSize) Task~List~
        +GetSummaryMetricsAsync() Task~ReconciliationMetrics~
        +GetRecordByOrderIdAsync(Guid orderId) Task~ReconciliationRecord~
    }

    class ReconciliationRepository {
        <<Repository Implementation>>
        -AppDbContext _context
        +GetLedgerAsync(string channel, string status, int page, int pageSize) Task~List~
        +GetSummaryMetricsAsync() Task~ReconciliationMetrics~
        +GetRecordByOrderIdAsync(Guid orderId) Task~ReconciliationRecord~
    }

    class AppDbContext {
        <<EF Core DbContext>>
        +DbSet~ReconciliationRecord~ ReconciliationRecords
        +DbSet~FeeSchedule~ FeeSchedules
        +SaveChangesAsync() Task~int~
    }

    SettlementController ..> ISettlementService : calls
    SettlementController ..> SettlementSummaryResponse : returns
    SettlementController ..> SettlementLedgerResponse : returns
    SettlementLedgerResponse o-- SettlementLedgerItemResponse : contains

    ISettlementService <|.. SettlementService : implements
    SettlementService --> IReconciliationRepository : queries ledger via
    IReconciliationRepository <|.. ReconciliationRepository : implements
    ReconciliationRepository --> AppDbContext : executes EF Core queries
```

---

## 3. Diagram 3.2: Statement Ingestion, Parsing & Automated Matching Engine

This diagram captures the ingestion pipeline, SHA-256 duplicate detection, parser abstraction, and batch reconciliation algorithm:

```mermaid
classDiagram
    direction TB

    class SettlementController {
        <<Controller>>
        -IStatementMatchingService _matchingService
        +ImportStatement(IFormFile file, string channelCode) Task~ActionResult~
    }

    class StatementImportResult {
        <<DTO Response>>
        +Guid ImportId
        +string FileName
        +string ChannelCode
        +string FileHashSha256
        +int TotalRows
        +int MatchedRows
        +int DiscrepancyRows
        +decimal TotalSettledAmount
        +DateTime ImportedAt
    }

    class IStatementMatchingService {
        <<Interface>>
        +ImportAndReconcileAsync(Stream fileStream, string fileName, string channelCode, string importedBy) Task~StatementImportResult~
    }

    class StatementMatchingService {
        <<Business Service>>
        -IFileStorageService _storageService
        -IStatementParserFactory _parserFactory
        -IStatementImportRepository _importRepo
        -IOrderRepository _orderRepo
        -IReconciliationRepository _reconRepo
        +ImportAndReconcileAsync(Stream fileStream, string fileName, string channelCode, string importedBy) Task~StatementImportResult~
        -MatchSingleRow(StatementLine line, Order order) ReconciliationRecord
    }

    class IFileStorageService {
        <<Interface>>
        +ComputeSha256(Stream stream) string
        +SaveStatementFileAsync(Stream stream, string fileName) Task~string~
    }

    class FileStorageService {
        <<Data Storage Service>>
        -string _storagePath
        +ComputeSha256(Stream stream) string
        +SaveStatementFileAsync(Stream stream, string fileName) Task~string~
    }

    class IStatementParser {
        <<Strategy Interface>>
        +bool CanParse(string fileName)
        +ParseAsync(Stream stream) Task~List~StatementLine~~
    }

    class ExcelStatementParser {
        <<Parser (ClosedXML)>>
        +bool CanParse(string fileName)
        +ParseAsync(Stream stream) Task~List~StatementLine~~
    }

    class CsvStatementParser {
        <<Parser (CsvHelper)>>
        +bool CanParse(string fileName)
        +ParseAsync(Stream stream) Task~List~StatementLine~~
    }

    class IStatementImportRepository {
        <<Repository Interface>>
        +ExistsByHashAsync(string sha256Hash) Task~bool~
        +AddImportAsync(StatementImport statementImport) Task
        +SaveChangesAsync() Task
    }

    SettlementController ..> IStatementMatchingService : dispatches upload
    SettlementController ..> StatementImportResult : produces
    IStatementMatchingService <|.. StatementMatchingService : implements

    StatementMatchingService --> IFileStorageService : checks hash & saves
    StatementMatchingService --> IStatementParser : parses lines
    StatementMatchingService --> IStatementImportRepository : persists import record
    IFileStorageService <|.. FileStorageService : implements
    IStatementParser <|.. ExcelStatementParser : implements
    IStatementParser <|.. CsvStatementParser : implements
```

---

## 4. Diagram 3.3: Domain Entities & Reconciliation State Model

This diagram details entity relationships and the mathematical invariant governing variance resolution:

```mermaid
classDiagram
    direction TB

    class StatementImport {
        <<Aggregate Root>>
        +Guid Id
        +string FileName
        +string FileHashSha256
        +string ChannelCode
        +int TotalRows
        +int MatchedRows
        +int DiscrepancyRows
        +decimal TotalWalletSettled
        +DateTime ImportedAt
        +string ImportedBy
        +List~StatementLine~ Lines
    }

    class StatementLine {
        <<Domain Entity>>
        +Guid Id
        +Guid StatementImportId
        +string ExternalOrderId
        +decimal PayoutAmount
        +DateTime PayoutDate
        +string RawLineContent
        +ReconciliationRecord MatchRecord
    }

    class ReconciliationRecord {
        <<Domain Entity - Audit>>
        +Guid Id
        +Guid OrderId
        +Guid StatementLineId
        +decimal ExpectedNetPayout
        +decimal ActualSettledAmount
        +decimal VarianceAmount
        +ReconciliationStatus Status
        +DateTime ReconciledAt
        +CalculateVariance() void
        +IsDiscrepancy() bool
    }

    class OrderFeeSnapshot {
        <<Immutable Domain Snapshot>>
        +Guid Id
        +Guid OrderId
        +decimal ExpectedNetPayout
        +bool IsImmutable
    }

    class ReconciliationStatus {
        <<Enumeration>>
        PendingSettlement
        Reconciled
        Discrepancy
    }

    StatementImport "1" *-- "1..*" StatementLine : contains
    StatementLine "1" o-- "0..1" ReconciliationRecord : matched to
    OrderFeeSnapshot "1" <-- "1" ReconciliationRecord : cross-references expected payout
    ReconciliationRecord ..> ReconciliationStatus : tracks status
```

---

## 5. Architectural & Financial Invariants Summary

1. **Cryptographic Deduplication (C4 Compliance):**
   * Before parsing, `FileStorageService` computes the `SHA-256` hash of the statement file.
   * `IStatementImportRepository.ExistsByHashAsync(hash)` blocks duplicate file uploads with HTTP `409 Conflict`, guaranteeing non-repudiation and idempotent ledgers.
2. **Reconciliation Invariant Equation:**
   $$\text{VarianceAmount} = \text{ActualSettledAmount} - \text{ExpectedNetPayout}$$
   * If $\text{VarianceAmount} = 0$: Marked as `Reconciled` (Green status tag).
   * If $\text{VarianceAmount} \neq 0$: Marked as `Discrepancy` (Red status tag) and immediately routed to the Discrepancy Audit module (`#DIS-002`). A negative variance indicates a merchant cash shortfall (e.g. carrier dimensional re-weighing fee or unexpected platform penalty).
3. **Execution SLA:**
   * Automated batch matching across a 2,000-line spreadsheet completes in $\le 3.0$ seconds via indexed in-memory dictionary lookups against `external_order_id`.
4. **Monetary Storage Rigor:**
   * All monetary quantities (`PayoutAmount`, `ExpectedNetPayout`, `ActualSettledAmount`, `VarianceAmount`) strictly employ C# `decimal` mapped to `NUMERIC(18,0)` VND in PostgreSQL.
