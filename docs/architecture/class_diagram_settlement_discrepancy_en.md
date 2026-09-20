# Class Diagram: Settlement Reconciliation & Discrepancy Resolution

> **Target Stack:** ASP.NET Core 8 (.NET 8 3-Tier Architecture) + PostgreSQL 16  
> **Source of Truth Hierarchy:** Requirements $\rightarrow$ Component Backend $\rightarrow$ Database $\rightarrow$ OpenAPI (6 Operations) $\rightarrow$ Folder Structure $\rightarrow$ Class Diagram  

---

## 1. Traceability & Scope Header

| Metadata Attribute | Authoritative Value |
|---|---|
| **Use Cases** | `UC05` (Settlement Ledger & Reconciliation Status Ingestion)<br/>`UC06` (Manual Settlement Reconciliation & Variance Computation)<br/>`UC07` (Discrepancy Audit Investigation & Resolution) |
| **OpenAPI Operations** | `GET /settlements` (`getSettlementLedger`)<br/>`GET /settlements/summary` (`getSettlementSummary`)<br/>`POST /settlements/{orderId}/reconcile` (`reconcileSettlement`)<br/>`GET /discrepancies` (`listDiscrepancies`)<br/>`GET /discrepancies/{id}` (`getDiscrepancyById`)<br/>`PATCH /discrepancies/{id}/resolve` (`resolveDiscrepancy`) |
| **Source Files** | `FashionWeb.Api/Controllers/SettlementController.cs`, `DiscrepanciesController.cs`<br/>`FashionWeb.Api/Contracts/Settlement/*`, `Discrepancies/*`<br/>`FashionWeb.Business/Commands/ReconcileSettlementCommand.cs`, `ResolveDiscrepancyCommand.cs`<br/>`FashionWeb.Business/Results/SettlementLedgerResult.cs`, `SettlementSummaryResult.cs`, `DiscrepancyDetailResult.cs`<br/>`FashionWeb.Business/Interfaces/Services/ISettlementService.cs`, `IDiscrepancyService.cs`<br/>`FashionWeb.Business/Services/SettlementService.cs`, `DiscrepancyService.cs`<br/>`FashionWeb.Business/Interfaces/Repositories/IReconciliationRepository.cs`, `IDiscrepancyRepository.cs`, `IOrderRepository.cs`, `IUnitOfWork.cs`<br/>`FashionWeb.Business/Domain/Entities/ReconciliationRecord.cs`, `DiscrepancyAudit.cs`<br/>`FashionWeb.Business/Domain/Enums/ChannelType.cs`, `ReconciliationStatus.cs`, `DiscrepancyType.cs`<br/>`FashionWeb.Data/Repositories/ReconciliationRepository.cs`, `DiscrepancyRepository.cs`, `UnitOfWork.cs` |
| **Target Database Tables** | `reconciliation_records`, `discrepancy_audits`, `orders`, `order_fee_snapshots` |
| **Actors & RBAC Permissions** | `Sales & Ops Staff` (No Access)<br/>`Finance Manager` (Read ledger/summary, reconcile settlements, view discrepancies, resolve discrepancies)<br/>`Shop Owner` (Full access across settlements and discrepancies) |

---

## 2. Settlement Reconciliation Architecture (`UC05`, `UC06`)

Settlement management provides visibility into platform fees and handles manual payout reconciliation against projected settlements.

### Invariants & Design Rules
1. **Manual Entry Target:** There are no automated batch statement ingestion pipelines, hash deduplication mechanisms, or statement line parsing tables. Finance Managers inspect the frozen projected settlement and manually record the actual payout received from the channel.
2. **Canonical Variance Formula:**
   $$\text{VarianceAmount} = \text{ProjectedSettlement} - \text{ActualSettlement}$$
   *(A positive variance denotes an underpayment/unexpected deduction by the platform; a negative variance indicates an overpayment).*
3. **Reconciliation Outcomes:**
   - When $\text{VarianceAmount} == 0 \implies \text{Status} = \text{RECONCILED}$ (notes & discrepancyType optional)
   - When $\text{VarianceAmount} \ne 0 \implies \text{Status} = \text{DISCREPANCY}$ (`notes` and `discrepancyType` mandatory; automatically spawns a `DiscrepancyAudit` record using `notes` as `explanation_note`).
4. **Order State Invariance:** After reconciliation, the corresponding `Order` status remains `DELIVERED`. Reconciliation updates the settlement ledger and financial state, not the logistical order status.
5. **Transaction Boundary (`IUnitOfWork`):** Reconciling an order with a variance atomically updates `reconciliation_records` and inserts `discrepancy_audits`.

```mermaid
classDiagram
    direction TB

    %% Presentation Tier
    class SettlementController {
        <<Controller>>
        -ISettlementService _settlementService
        +GetSettlementLedger(SettlementLedgerFilterRequest filter) Task~ActionResult~PagedSettlementLedgerResponse~~
        +GetSettlementSummary(DateTime? fromDate, DateTime? toDate) Task~ActionResult~SettlementSummaryResponse~~
        +ReconcileSettlement(Guid orderId, ReconcileSettlementRequest request) Task~ActionResult~ReconciliationResponse~~
    }

    class ReconcileSettlementRequest {
        <<Request DTO>>
        +decimal ActualSettlement
        +string? Notes
        +DiscrepancyType? DiscrepancyType
    }

    class SettlementLedgerItemResponse {
        <<Response DTO>>
        +Guid OrderId
        +string ExternalOrderId
        +ChannelType Channel
        +decimal GrossRevenue
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal FixedFee
        +decimal TotalPlatformFees
        +decimal ProjectedSettlement
        +decimal? ActualSettlement
        +decimal? VarianceAmount
        +ReconciliationStatus ReconciliationStatus
        +DateTime? ReconciledAt
    }

    class SettlementSummaryResponse {
        <<Response DTO>>
        +int PendingSettlementCount
        +int ReconciledCount
        +int DiscrepancyCount
    }

    class ReconciliationResponse {
        <<Response DTO>>
        +Guid Id
        +Guid OrderId
        +decimal ProjectedSettlement
        +decimal ActualSettlement
        +decimal VarianceAmount
        +ReconciliationStatus ReconciliationStatus
        +DateTime ReconciledAt
        +string? ReconciledBy
    }

    %% Application / Business Commands & Results
    class ReconcileSettlementCommand {
        <<Command>>
        +Guid OrderId
        +decimal ActualSettlement
        +string? Notes
        +DiscrepancyType? DiscrepancyType
        +string ActorIdentity
    }

    class SettlementLedgerResult {
        <<Result>>
        +Guid OrderId
        +string ExternalOrderId
        +ChannelType Channel
        +decimal GrossRevenue
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal FixedFee
        +decimal TotalPlatformFees
        +decimal ProjectedSettlement
        +decimal? ActualSettlement
        +decimal? VarianceAmount
        +ReconciliationStatus Status
        +DateTime? ReconciledAt
    }

    class SettlementSummaryResult {
        <<Result>>
        +int PendingSettlementCount
        +int ReconciledCount
        +int DiscrepancyCount
    }

    %% Service Contracts
    class ISettlementService {
        <<Service Interface>>
        +GetLedgerAsync(SettlementQueryFilter filter) Task~PagedResult~SettlementLedgerResult~~
        +GetSummaryAsync(DateTime? fromDate, DateTime? toDate) Task~SettlementSummaryResult~
        +ReconcileAsync(ReconcileSettlementCommand command) Task~ReconciliationRecord~
    }

    %% Business Service Implementations
    class SettlementService {
        <<Business Service>>
        -IReconciliationRepository _reconRepo
        -IDiscrepancyRepository _discrepancyRepo
        -IOrderRepository _orderRepo
        -IUnitOfWork _unitOfWork
        +GetLedgerAsync(SettlementQueryFilter filter) Task~PagedResult~SettlementLedgerResult~~
        +GetSummaryAsync(DateTime? fromDate, DateTime? toDate) Task~SettlementSummaryResult~
        +ReconcileAsync(ReconcileSettlementCommand command) Task~ReconciliationRecord~
    }

    %% Persistence Ports & UnitOfWork
    class IReconciliationRepository {
        <<Repository Port>>
        +GetByOrderIdAsync(Guid orderId) Task~ReconciliationRecord?~
        +GetByIdAsync(Guid id) Task~ReconciliationRecord?~
        +ListLedgerAsync(SettlementQueryFilter filter) Task~PagedResult~SettlementLedgerResult~~
        +GetSummaryAsync(DateTime? fromDate, DateTime? toDate) Task~SettlementSummaryResult~
        +AddAsync(ReconciliationRecord record) Task
        +UpdateAsync(ReconciliationRecord record) Task
    }

    class IUnitOfWork {
        <<Repository Port>>
        +ExecuteTransactionAsync(Func~Task~ action) Task
        +SaveChangesAsync() Task~int~
    }

    class ReconciliationRepository {
        <<Repository Adapter>>
        -AppDbContext _context
    }

    class UnitOfWork {
        <<Repository Adapter>>
        -AppDbContext _context
        +ExecuteTransactionAsync(Func~Task~ action) Task
        +SaveChangesAsync() Task~int~
    }

    class AppDbContext {
        <<Infrastructure>>
    }

    %% Relationships
    SettlementController ..> ISettlementService : invokes
    SettlementController ..> ReconcileSettlementRequest : binds
    SettlementController ..> ReconcileSettlementCommand : maps to
    SettlementController ..> SettlementLedgerItemResponse : returns
    SettlementController ..> SettlementSummaryResponse : returns
    SettlementController ..> ReconciliationResponse : returns

    ISettlementService <|.. SettlementService : implements
    SettlementService --> IReconciliationRepository : persists/reads records
    SettlementService --> IDiscrepancyRepository : spawns audit if variance != 0
    SettlementService --> IOrderRepository : verifies DELIVERED status
    SettlementService --> IUnitOfWork : atomic reconciliation transaction

    IReconciliationRepository <|.. ReconciliationRepository : implements
    IUnitOfWork <|.. UnitOfWork : implements
    ReconciliationRepository --> AppDbContext : executes SQL
    UnitOfWork --> AppDbContext : manages transaction
```

---

## 3. Discrepancy Investigation & Resolution Architecture (`UC07`)

When manual reconciliation encounters a difference between projected and actual payouts, the system captures a structured discrepancy audit log. Finance Managers and Shop Owners investigate and record corrective resolution notes.

### Invariants & Design Rules
1. **No Multi-Stage Approval Workflows:** Obsolete multi-stage governance statuses, two-man sign-off requests, and file attachment URLs from discarded drafts are completely eliminated. Both Finance Managers and Shop Owners possess direct operational authority to review and resolve discrepancies.
2. **Simplified RBAC Resolution:** Both Finance Managers and Shop Owners possess permissions to review and resolve discrepancies via `PATCH /discrepancies/{id}/resolve`.
3. **Resolution State Derivation:** The entity does not maintain an unnecessary `isResolved` boolean column. The resolution status is derived:
   $$\text{IsResolved} = (\text{ResolvedAt} \ne \text{null})$$
4. **Resolution Immutability:** Resolving a discrepancy updates `ResolutionNotes`, `ResolvedBy`, and `ResolvedAt`.

```mermaid
classDiagram
    direction TB

    %% Presentation Tier
    class DiscrepanciesController {
        <<Controller>>
        -IDiscrepancyService _discrepancyService
        +ListDiscrepancies(DiscrepancyFilterRequest filter) Task~ActionResult~PagedDiscrepancyResponse~~
        +GetDiscrepancyById(Guid id) Task~ActionResult~DiscrepancyDetailResponse~~
        +ResolveDiscrepancy(Guid id, ResolveDiscrepancyRequest request) Task~ActionResult~DiscrepancyDetailResponse~~
    }

    class ResolveDiscrepancyRequest {
        <<Request DTO>>
        +string ResolutionNotes
    }

    class DiscrepancyListItemResponse {
        <<Response DTO>>
        +Guid Id
        +Guid ReconciliationRecordId
        +Guid OrderId
        +string ExternalOrderId
        +ChannelType Channel
        +decimal ProjectedSettlement
        +decimal ActualSettlement
        +decimal VarianceAmount
        +DiscrepancyType DiscrepancyType
        +bool IsResolved
        +DateTime CreatedAt
    }

    class DiscrepancyDetailResponse {
        <<Response DTO>>
        +Guid Id
        +Guid ReconciliationRecordId
        +Guid OrderId
        +string ExternalOrderId
        +ChannelType Channel
        +decimal ProjectedSettlement
        +decimal ActualSettlement
        +decimal VarianceAmount
        +DiscrepancyType DiscrepancyType
        +string ExplanationNote
        +string? ResolutionNotes
        +string? ResolvedBy
        +DateTime? ResolvedAt
        +bool IsResolved
        +DateTime CreatedAt
    }

    %% Application / Business Commands & Results
    class ResolveDiscrepancyCommand {
        <<Command>>
        +Guid DiscrepancyId
        +string ResolutionNotes
        +string ActorIdentity
    }

    class DiscrepancyDetailResult {
        <<Result>>
        +Guid Id
        +Guid ReconciliationRecordId
        +Guid OrderId
        +string ExternalOrderId
        +ChannelType Channel
        +decimal ProjectedSettlement
        +decimal ActualSettlement
        +decimal VarianceAmount
        +DiscrepancyType DiscrepancyType
        +string ExplanationNote
        +string? ResolutionNotes
        +string? ResolvedBy
        +DateTime? ResolvedAt
        +DateTime CreatedAt
        +bool IsResolved
    }

    %% Service Contracts
    class IDiscrepancyService {
        <<Service Interface>>
        +ListDiscrepanciesAsync(DiscrepancyQueryFilter filter) Task~PagedResult~DiscrepancyAudit~~
        +GetByIdAsync(Guid id) Task~DiscrepancyDetailResult?~
        +ResolveDiscrepancyAsync(ResolveDiscrepancyCommand command) Task~DiscrepancyAudit~
    }

    %% Business Service Implementations
    class DiscrepancyService {
        <<Business Service>>
        -IDiscrepancyRepository _discrepancyRepo
        +ListDiscrepanciesAsync(DiscrepancyQueryFilter filter) Task~PagedResult~DiscrepancyAudit~~
        +GetByIdAsync(Guid id) Task~DiscrepancyDetailResult?~
        +ResolveDiscrepancyAsync(ResolveDiscrepancyCommand command) Task~DiscrepancyAudit~
    }

    %% Persistence Ports & Adapters
    class IDiscrepancyRepository {
        <<Repository Port>>
        +GetByIdAsync(Guid id) Task~DiscrepancyAudit?~
        +GetDetailByIdAsync(Guid id) Task~DiscrepancyDetailResult?~
        +ListAsync(DiscrepancyQueryFilter filter) Task~PagedResult~DiscrepancyAudit~~
        +AddAsync(DiscrepancyAudit audit) Task
        +UpdateAsync(DiscrepancyAudit audit) Task
    }

    class DiscrepancyRepository {
        <<Repository Adapter>>
        -AppDbContext _context
    }

    %% Relationships
    DiscrepanciesController ..> IDiscrepancyService : invokes
    DiscrepanciesController ..> ResolveDiscrepancyRequest : binds
    DiscrepanciesController ..> ResolveDiscrepancyCommand : maps to
    DiscrepanciesController ..> DiscrepancyDetailResponse : returns

    IDiscrepancyService <|.. DiscrepancyService : implements
    DiscrepancyService --> IDiscrepancyRepository : queries / updates

    IDiscrepancyRepository <|.. DiscrepancyRepository : implements
    DiscrepancyRepository --> AppDbContext : executes SQL
```

---

## 4. Settlement Domain Model & Canonical Relationships

```mermaid
classDiagram
    direction TB

    %% Entities
    class ReconciliationRecord {
        <<Entity>>
        +Guid Id
        +Guid OrderId
        +decimal ProjectedSettlement
        +decimal? ActualSettlement
        +decimal? VarianceAmount
        +ReconciliationStatus Status
        +string? ReconciliationNotes
        +DateTime? ReconciledAt
        +string? ReconciledBy
        +DateTime CreatedAt
        +DateTime? UpdatedAt
        +List~DiscrepancyAudit~ Audits
        +Reconcile(decimal actual, string actorIdentity, string? notes, DiscrepancyType? type)
    }

    class DiscrepancyAudit {
        <<Entity>>
        +Guid Id
        +Guid ReconciliationRecordId
        +DiscrepancyType DiscrepancyType
        +string ExplanationNote
        +string? ResolutionNotes
        +string? ResolvedBy
        +DateTime? ResolvedAt
        +DateTime CreatedAt
        +Resolve(string notes, string actorIdentity)
        +bool IsResolved()
    }

    class Order {
        <<Entity>>
        +Guid Id
        +string ExternalOrderId
        +ChannelType Channel
        +OrderStatus Status
        +decimal Subtotal
        +decimal GrossRevenue
    }

    class OrderFeeSnapshot {
        <<Entity>>
        +Guid Id
        +Guid OrderId
        +decimal TotalPlatformFees
        +decimal ProjectedSettlement
        +DateTime SnapshotAt
    }

    %% Enumerations
    class ReconciliationStatus {
        <<Enumeration>>
        PENDING_SETTLEMENT
        RECONCILED
        DISCREPANCY
    }

    class DiscrepancyType {
        <<Enumeration>>
        COMMISSION_RATE_MISMATCH
        PAYMENT_FEE_MISMATCH
        SERVICE_FEE_MISMATCH
        UNEXPECTED_PLATFORM_CHARGE
        OTHER
    }

    class ChannelType {
        <<Enumeration>>
        TIKTOK
        SHOPEE
        POS
    }

    %% Cardinality & Relationships
    Order "1" -- "0..1" ReconciliationRecord : linked when DELIVERED
    Order "1" -- "0..1" OrderFeeSnapshot : provides ProjectedSettlement
    ReconciliationRecord "1" *-- "0..*" DiscrepancyAudit : tracks investigations
    ReconciliationRecord --> ReconciliationStatus : current state
    DiscrepancyAudit --> DiscrepancyType : classification
    Order --> ChannelType : originating channel
```

---

## 5. OpenAPI Operations Traceability Table

| Endpoint | Method | Controller Action | Command / Parameter | Service Invocations | Database Operations |
|---|---|---|---|---|---|
| `/settlements` | `GET` | `SettlementController.GetSettlementLedger` | `SettlementLedgerFilterRequest` | `ISettlementService.GetLedgerAsync` | Joined read: `reconciliation_records`, `orders`, `order_fee_snapshots` |
| `/settlements/summary` | `GET` | `SettlementController.GetSettlementSummary` | `fromDate`, `toDate` | `ISettlementService.GetSummaryAsync` | Aggregated status count query over `reconciliation_records` |
| `/settlements/{orderId}/reconcile` | `POST` | `SettlementController.ReconcileSettlement` | `ReconcileSettlementCommand` | `ISettlementService.ReconcileAsync`<br/>`IUnitOfWork.ExecuteTransactionAsync` | Atomic transaction: Update `reconciliation_records` (`ActualSettlement`, `VarianceAmount`, `Status`, `ReconciledAt`, `ReconciledBy`).<br/>If variance != 0: Insert `discrepancy_audits` |
| `/discrepancies` | `GET` | `DiscrepanciesController.ListDiscrepancies` | `DiscrepancyFilterRequest` | `IDiscrepancyService.ListDiscrepanciesAsync` | Joined read: `discrepancy_audits`, `reconciliation_records`, `orders` |
| `/discrepancies/{id}` | `GET` | `DiscrepanciesController.GetDiscrepancyById` | `Guid id` | `IDiscrepancyService.GetByIdAsync` | Read `discrepancy_audits` with parent `reconciliation_records` & `orders` |
| `/discrepancies/{id}/resolve` | `PATCH` | `DiscrepanciesController.ResolveDiscrepancy` | `ResolveDiscrepancyCommand` | `IDiscrepancyService.ResolveDiscrepancyAsync` | Update `discrepancy_audits` (`ResolutionNotes`, `ResolvedBy`, `ResolvedAt`) |
