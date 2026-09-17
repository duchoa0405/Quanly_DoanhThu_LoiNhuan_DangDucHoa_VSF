# 05 — Sequence Diagrams: Core API Workflows

---

## 1. Scope & Execution Scenarios

This document details the exact sequence of cross-tier calls, validation guards, and error responses for the 6 core operational workflows:
1. **Scenario 1:** Real-Time Fee Preview & Multi-Channel Order Creation (`MOD-01`)
2. **Scenario 2:** Order Lifecycle State Transitions & Immutable Snapshot Freezing (`SCR-01`)
3. **Scenario 3:** Order Cancellation & Zero Phantom Revenue Enforcement (`MOD-02`)
4. **Scenario 4:** Statement Ingestion, SHA-256 Deduplication & 2-Way Batch Reconciliation (`MOD-03`)
5. **Scenario 5:** Discrepancy Auditing & Executive Board Approval (`#DIS-002`)
6. **Scenario 6:** Executive KPI Aggregation & Streaming CSV Audit Export (`SCR-03`)

---

## 2. Scenario 1: Real-Time Fee Preview & Multi-Channel Order Creation (MOD-01)

This scenario demonstrates live fee calculation while typing, voucher boundary verification, and initial order state assignment:

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Sales & Ops Staff
    participant UI as CreateOrderModal (MOD-01)
    participant API as OrdersController
    participant Svc as OrderService
    participant Factory as FeeStrategyFactory
    participant Strat as TikTokShopFeeStrategy
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    %% Part A: Real-Time Fee Preview
    Note over Staff, Strat: Part A: Real-Time Fee Deduction Preview (Debounced)
    Staff->>UI: Selects "TIKTOK", inputs Subtotal 500.000d, Voucher 50.000d
    UI->>API: POST /api/v1/orders/preview-fee (FeePreviewRequest)
    API->>Factory: GetStrategy("TIKTOK")
    Factory-->>API: returns TikTokShopFeeStrategy
    API->>Strat: CalculateFees(Subtotal: 500k, Voucher: 50k)
    Strat-->>API: returns FeeBreakdown (Comm 4%, Pay 3%, Fixed 2k, Net 414k)
    API-->>UI: HTTP 200 OK (FeeBreakdownResponse)
    UI-->>Staff: Displays live deduction preview (Fees: Crimson Red, Net: Dark Green)

    %% Part B: Persisting Order
    Note over Staff, DB: Part B: Submitting & Persisting New Order
    Staff->>UI: Clicks [Create Order]
    UI->>API: POST /api/v1/orders (CreateOrderRequest JSON)
    
    alt Voucher Validation Fails (ShopVoucher > GrossSubtotal)
        API-->>UI: HTTP 422 Unprocessable Entity ("Voucher cannot exceed gross items subtotal")
        UI-->>Staff: Highlights voucher input in red
    else Validation Successful
        API->>Svc: CreateOrderAsync(requestDto)
        
        alt Channel is In-Store POS (Cash / Card Swipe)
            Note over Svc: In-store take-home rule: Instant Fulfillment
            Svc->>Svc: Set Status = OrderStatus.Delivered, DeliveredAt = DateTime.UtcNow
        else Channel is TikTok Shop or Shopee
            Note over Svc: Anti-Phantom Revenue rule: Awaits delivery
            Svc->>Svc: Set Status = OrderStatus.Pending, RecognizedRevenue = 0d
        end
        
        Svc->>Repo: AddAsync(newOrder)
        Repo->>DB: INSERT INTO orders, order_items
        DB-->>Repo: Committed successfully
        Repo-->>Svc: Order with generated Guid Id
        Svc-->>API: OrderDetailResponse DTO
        API-->>UI: HTTP 201 Created (OrderDetailResponse)
        UI-->>Staff: Closes modal & refreshes SCR-01 order table
    end
```

---

## 3. Scenario 2: Order Lifecycle Transitions & Immutable Snapshot Freezing

This scenario enforces the **Zero Phantom Revenue** invariant — revenue is recognized and platform fees are immutably frozen if and only if an order transitions to `DELIVERED`:

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Fulfillment Staff
    participant UI as OrdersTable (SCR-01)
    participant API as OrdersController
    participant Svc as OrderService
    participant Factory as FeeStrategyFactory
    participant Strat as IPlatformFeeStrategy
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    Staff->>UI: Clicks [Mark as Delivered] on Order ORD-2026-001
    UI->>API: PATCH /api/v1/orders/{id}/status (NewStatus: "DELIVERED")

    API->>Svc: UpdateStatusAsync(id, OrderStatus.Delivered)
    Svc->>Repo: GetByIdAsync(id)
    Repo->>DB: SELECT * FROM orders WHERE id = @id
    DB-->>Repo: returns Order entity

    alt Invalid Transition (Current Status != SHIPPED)
        Note over Svc: Enforces state machine guard: PENDING -> SHIPPED -> DELIVERED
        Svc-->>API: Throws InvalidOperationException ("Order must be SHIPPED before DELIVERED")
        API-->>UI: HTTP 409 Conflict ("Invalid state transition")
        UI-->>Staff: Displays error toast notification
    else Valid Transition (Current Status == SHIPPED)
        Svc->>Svc: Update order.Status = OrderStatus.Delivered
        Svc->>Svc: Update order.DeliveredAt = DateTime.UtcNow
        
        Note over Svc, Strat: Freeze Immutable Snapshot
        Svc->>Factory: GetStrategy(order.Channel)
        Factory-->>Svc: returns concrete strategy
        Svc->>Strat: CalculateFees(order.GrossSubtotal, order.ShopVoucher)
        Strat-->>Svc: returns FeeBreakdown
        
        Svc->>Svc: Instantiate OrderFeeSnapshot (IsImmutable = true, FrozenAt = UtcNow)
        Svc->>Repo: UpdateAsync(order)
        Repo->>DB: BEGIN TRANSACTION<br/>UPDATE orders SET status = 'DELIVERED', delivered_at = NOW()<br/>INSERT INTO order_fee_snapshots (...)<br/>COMMIT
        DB-->>Repo: Transaction Committed
        Repo-->>Svc: Persisted Order entity
        Svc-->>API: OrderDetailResponse DTO
        API-->>UI: HTTP 200 OK (Updated Order)
        UI-->>Staff: Shows green success badge & triggers SCR-03 KPI refresh
    end
```

---

## 4. Scenario 3: Order Cancellation & Phantom Revenue Exclusion (MOD-02)

This scenario demonstrates order cancellation handling and blocks cancellation for already finalized accounting ledgers:

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Sales & Ops Staff
    participant UI as CancelOrderModal (MOD-02)
    participant API as OrdersController
    participant Svc as OrderService
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    Staff->>UI: Selects reason "Out of Stock" & clicks [Confirm Cancel]
    UI->>API: POST /api/v1/orders/{id}/cancel (CancelOrderRequest)

    API->>Svc: CancelOrderAsync(id, reason)
    Svc->>Repo: GetByIdAsync(id)
    Repo->>DB: SELECT * FROM orders WHERE id = @id
    DB-->>Repo: returns Order entity

    alt Order is Already DELIVERED
        Note over Svc: Financial Immutability rule: Finalized ledgers cannot be cancelled
        Svc-->>API: Throws BusinessRuleException ("Delivered orders cannot be cancelled")
        API-->>UI: HTTP 422 Unprocessable Entity ("Cannot cancel finalized delivery; requires formal refund journal")
        UI-->>Staff: Blocks action with warning modal
    else Order is PENDING or SHIPPED
        Svc->>Svc: Update order.Status = OrderStatus.Cancelled
        Svc->>Svc: Record order.CancellationReason & CancelledAt
        Svc->>Repo: UpdateAsync(order)
        Repo->>DB: UPDATE orders SET status = 'CANCELLED', cancellation_reason = @reason
        DB-->>Repo: Success
        Svc-->>API: Completed
        API-->>UI: HTTP 204 No Content
        UI-->>Staff: Displays cancellation confirmation toast
        Note over DB: Cancelled order contributes 0 VND to all financial revenue analytics
    end
```

---

## 5. Scenario 4: Statement Ingestion, SHA-256 Deduplication & 2-Way Batch Reconciliation (MOD-03)

This scenario models the automated spreadsheet ingestion, cryptographic duplicate detection, and two-way reconciliation pipeline:

```mermaid
sequenceDiagram
    autonumber
    actor Fin as Finance Manager
    participant UI as ImportStatementModal (MOD-03)
    participant API as SettlementController
    participant Svc as StatementMatchingService
    participant Storage as FileStorageService
    participant Parser as ExcelStatementParser
    participant Repo as ReconciliationRepository
    participant DB as PostgreSQL 16

    Fin->>UI: Drops file "TikTok_Payout_20260917.xlsx" & selects "TIKTOK"
    UI->>API: POST /api/v1/settlement/statements/import (multipart/form-data)

    API->>Svc: ImportAndReconcileAsync(fileStream, fileName, channelCode, user)
    Svc->>Storage: ComputeSha256(fileStream)
    Storage-->>Svc: returns sha256Checksum

    Svc->>Repo: ExistsByHashAsync(sha256Checksum)
    Repo->>DB: SELECT COUNT(1) FROM statement_imports WHERE file_hash_sha256 = @hash
    DB-->>Repo: returns count

    alt File Hash Exists (Duplicate Statement Detected)
        Svc-->>API: Throws DuplicateFileException ("Statement has already been imported")
        API-->>UI: HTTP 409 Conflict ("Duplicate statement detected. Re-upload rejected.")
        UI-->>Fin: Displays duplicate upload error alert
    else File is New (Deduplication Check Passed)
        Svc->>Storage: SaveStatementFileAsync(fileStream, fileName)
        Storage-->>Svc: returns storedFilePath
        
        Svc->>Parser: ParseAsync(fileStream)
        Note over Parser: Parses 2,000 spreadsheet rows via ClosedXML (<= 1.5s)
        Parser-->>Svc: returns List of StatementLine entities
        
        Note over Svc, Repo: 2-Way Automated Matching Engine (<= 1.5s)
        loop For each StatementLine
            Svc->>Repo: Query matching DELIVERED order by external_order_id
            alt Order Found with FeeSnapshot
                Svc->>Svc: VarianceAmount = line.PayoutAmount - snapshot.ExpectedNetPayout
                alt VarianceAmount == 0
                    Svc->>Svc: Status = ReconciliationStatus.Reconciled (100% Match)
                else VarianceAmount != 0
                    Svc->>Svc: Status = ReconciliationStatus.Discrepancy (Flagged #DIS)
                end
            else Order Not Found or Pending
                Svc->>Svc: Status = ReconciliationStatus.PendingSettlement
            end
        end

        Svc->>Repo: AddBatchAsync(StatementImport, StatementLines, ReconciliationRecords)
        Repo->>DB: Batch INSERT with transaction
        DB-->>Repo: Batch committed successfully
        Svc-->>API: StatementImportResult DTO (TotalRows, Matched, Discrepancies)
        API-->>UI: HTTP 201 Created (StatementImportResult)
        UI-->>Fin: Displays reconciliation metrics: 1,995 Matched | 5 Discrepancies
    end
```

---

## 6. Scenario 5: Discrepancy Auditing & Executive Board Approval (#DIS-002)

This scenario models the audit justification workflow where finance staff logs dispute claims and only the shop owner holds binding approval authority:

```mermaid
sequenceDiagram
    autonumber
    actor Fin as Finance Manager
    actor Owner as Shop Owner / Executive
    participant UI as SCR-02 Settlement Ledger
    participant API as DiscrepanciesController
    participant Svc as DiscrepancyService
    participant Repo as DiscrepancyRepository
    participant DB as PostgreSQL 16

    %% Part A: Filing Audit Case
    Note over Fin, DB: Part A: Filing Discrepancy Justification Claim
    Fin->>UI: Clicks on Discrepancy row (Variance: -25.000d)
    UI-->>Fin: Opens DiscrepancyAuditModal
    Fin->>UI: Inputs Root Cause: WEIGHT_SURCHARGE, Note: "Carrier re-weighed +300g", Evidence URL
    UI->>API: POST /api/v1/discrepancies (CreateDiscrepancyRequest)
    API->>Svc: CreateAuditAsync(requestDto, createdBy: Fin.Username)
    Svc->>Repo: AddAsync(new DiscrepancyAudit, Status = PendingApproval)
    Repo->>DB: INSERT INTO discrepancy_audits (code: "#DIS-002", ...)
    DB-->>Repo: Committed
    Svc-->>API: DiscrepancyAuditDTO
    API-->>UI: HTTP 201 Created
    UI-->>Fin: Row status changes to PENDING_APPROVAL (Yellow Tag)

    %% Part B: Executive Review & Approval
    Note over Owner, DB: Part B: Executive Review & Final Approval
    Owner->>UI: Opens SCR-02 & filters by PENDING_APPROVAL
    UI-->>Owner: Displays #DIS-002 audit card with carrier evidence URL
    Owner->>UI: Clicks [Approve Dispute] with note: "Accepted carrier chargeback"
    UI->>API: PATCH /api/v1/discrepancies/{id}/approve (decision: "APPROVED")

    alt Caller Role is Sales or Finance (Unauthorized)
        Note over API: RBAC Security Policy: RequireOwnerPolicy
        API-->>UI: HTTP 403 Forbidden ("Only Shop Owners can approve financial disputes")
        UI-->>Owner: Access denied
    else Caller is Shop Owner (Authorized)
        API->>Svc: ApproveDiscrepancyAsync(id, requestDto, reviewer: Owner.Username)
        Svc->>Repo: GetByIdAsync(id)
        Repo->>DB: SELECT * FROM discrepancy_audits WHERE id = @id
        DB-->>Repo: returns audit record
        
        Svc->>Svc: Update Status = ApprovalStatus.Approved
        Svc->>Svc: Set ReviewedBy = Owner, ReviewedAt = UtcNow
        Svc->>Repo: UpdateAsync(audit)
        Repo->>DB: UPDATE discrepancy_audits SET status = 'APPROVED', ...
        DB-->>Repo: Committed
        Svc-->>API: DiscrepancyAuditDTO
        API-->>UI: HTTP 200 OK (Approved)
        UI-->>Owner: Status turns Green (APPROVED) & settlement period is officially locked
    end
```

---

## 7. Scenario 6: Executive KPI Aggregation & Streaming CSV Export (SCR-03)

This scenario models real-time analytics aggregation over `DELIVERED` orders and high-volume CSV streaming:

```mermaid
sequenceDiagram
    autonumber
    actor Owner as Shop Owner / Executive
    participant UI as RevenueDashboard (SCR-03)
    participant API as AnalyticsController
    participant Svc as AnalyticsService
    participant Repo as AnalyticsRepository
    participant DB as PostgreSQL 16

    Owner->>UI: Selects Filter: "This Month", Channel: "ALL"
    UI->>API: GET /api/v1/analytics/kpis?from=2026-09-01&to=2026-09-30&channel=ALL

    API->>Svc: GetKPIsAsync(fromDate, toDate, channel)
    Svc->>Repo: GetDeliveredOrdersAggregateAsync(fromDate, toDate, channel)
    
    Note over Repo, DB: Zero Phantom Revenue: Filters strictly by DELIVERED
    Repo->>DB: SELECT SUM(gross_subtotal - shop_voucher) as Gross,<br/>SUM(total_platform_fees) as Fees,<br/>SUM(expected_net_payout) as Net<br/>FROM orders o JOIN order_fee_snapshots s ON o.id = s.order_id<br/>WHERE o.status = 'DELIVERED' AND o.delivered_at BETWEEN @from AND @to
    DB-->>Repo: returns aggregate row
    Repo-->>Svc: KpiAggregateData
    Svc-->>API: ExecutiveKPIsResponse DTO
    API-->>UI: HTTP 200 OK (ExecutiveKPIsResponse)
    UI-->>Owner: Renders 4 KPI cards (Total Sales, Total Fees, Net Cash, Delivered Count)

    %% CSV Streaming Export
    Note over Owner, DB: Streaming CSV Export
    Owner->>UI: Clicks [Export CSV Audit Report]
    UI->>API: GET /api/v1/analytics/export-csv
    API->>Svc: ExportReconciliationCsvAsync()
    Svc->>Repo: StreamReconciliationRecordsAsync()
    Repo->>DB: Open read-only cursor on reconciliation_records
    DB-->>Repo: Stream row chunks
    Repo-->>Svc: Yield line records
    Svc-->>API: Byte array stream (text/csv)
    API-->>UI: HTTP 200 OK with Content-Disposition: attachment; filename="Reconciliation_Report_20260917.csv"
    UI-->>Owner: Triggers browser file download (Zero RAM blowup)
```
