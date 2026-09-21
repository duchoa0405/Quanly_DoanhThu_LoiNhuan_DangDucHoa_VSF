# Kế Hoạch Thiết Kế Class Diagram, Sequence Diagram & API Traceability Theo .NET

## 1. Mục tiêu

Tài liệu này mô tả kế hoạch thực hiện phần thiết kế diagrams sau khi đã có folder structure cho frontend ReactJS và backend ASP.NET Core 3-tier.

Mục tiêu là biến folder structure, OpenAPI, ERD và business requirements thành bản thiết kế code rõ ràng trước khi triển khai API. Phần này **không viết code API**; code và unit/integration tests thuộc giai đoạn tiếp theo.

Kiến trúc áp dụng:

```text
ReactJS + ASP.NET Core Web API + Entity Framework Core + PostgreSQL
```

Backend tuân thủ 3 tầng:

```text
FashionWeb.Api -> FashionWeb.Business -> FashionWeb.Data
```

---

## 2. Đầu vào thiết kế

Các diagrams phải bám theo những nguồn sau:

| Tài liệu đầu vào | Vai trò trong thiết kế |
|---|---|
| `folder_structures.md` | Vị trí project, folder, class và module dự kiến |
| `implementation_plan_dotnet.md` | Kiến trúc React + ASP.NET Core 3-tier |
| `openapi_spec.yaml` | Route, HTTP method, request/response, status code |
| `database_erd.md` | Entity, quan hệ dữ liệu, constraint, index |
| `requirements_invest.md` | User story, acceptance criteria và business rule |
| `usecase.md` | Actor, role, use case và RBAC |
| `uiux_specifications.md` | Screen, modal và frontend interaction flow |

Nguyên tắc traceability bắt buộc:

```text
UI/Modal -> Endpoint -> Controller -> Service -> Repository -> Entity/Table
```

Ví dụ:

```text
CreateOrderModal
-> POST /api/v1/orders
-> OrdersController.CreateOrder()
-> OrderService.CreateOrderAsync()
-> OrderRepository.AddAsync()
-> Order + OrderItem tables
```

---

## 3. Giai đoạn 0 — Chuẩn hoá đầu vào

Trước khi vẽ diagrams, cần rà soát và chốt các mâu thuẫn còn tồn tại:

1. Chọn một OpenAPI và ERD làm source of truth.
2. Chốt stack backend là ASP.NET Core Web API + EF Core + PostgreSQL.
3. Chuẩn hoá công thức chênh lệch:

   ```text
   variance_amount = actual_settled_amount - expected_net_payout
   ```

   Giá trị âm biểu thị số tiền thực nhận thấp hơn số tiền kỳ vọng.

4. Chuẩn hoá payment method: `CASH`, `CARD_QR`, `PLATFORM_WALLET`.
5. Chuẩn hoá tên modal theo nghiệp vụ: `CreateOrderModal`, `CancelOrderModal`, `ReconcileSettlementModal`, `ImportStatementModal`, `FeeScheduleModal`, `SourceOrderDrilldownModal`.
6. Chốt mô hình User, Role/Policy và RBAC ở API.

Đầu ra: danh sách quyết định kiến trúc và API contract nhất quán để dùng làm nền cho diagrams.

---

## 4. Giai đoạn 1 — Thiết kế Class Diagrams

Không tạo một class diagram quá lớn. Diagram được tách theo bounded context để dễ đọc và bám đúng cấu trúc folder.

### 4.1. Class Diagram: Orders & Fee Engine

Các class/interface chính:

```text
OrdersController
IOrderService / OrderService
IOrderRepository / OrderRepository
Order, OrderItem, OrderFeeSnapshot
IFeeEngine / FeeEngine
IPlatformFeeStrategy
TikTokShopFeeStrategy
ShopeeFeeStrategy
PosFeeStrategy
FeeStrategyFactory
FeeSchedule
```

Diagram cần thể hiện:

- `OrdersController` phụ thuộc `IOrderService`.
- `OrderService` phụ thuộc `IOrderRepository` và `IFeeEngine`.
- `FeeEngine` sử dụng `FeeStrategyFactory` để chọn strategy theo `ChannelCode`.
- Các class strategy implement `IPlatformFeeStrategy`.
- `Order` chứa nhiều `OrderItem` và có tối đa một `OrderFeeSnapshot` khi được giao thành công.

### 4.2. Class Diagram: Settlement & Reconciliation

Các class/interface chính:

```text
SettlementsController
ISettlementService / SettlementService
IStatementImportService / StatementImportService
IStatementMatchingService / StatementMatchingService
IStatementParser
ExcelStatementParser
CsvStatementParser
IStatementImportRepository
IReconciliationRepository
StatementImport, StatementLine, ReconciliationRecord
IFileStorageService
```

Diagram cần thể hiện parser, SHA-256 file storage, quá trình tạo statement lines và matching với `OrderFeeSnapshot`.

### 4.3. Class Diagram: Discrepancy Audit

Các class/interface chính:

```text
DiscrepanciesController
IDiscrepancyService / DiscrepancyService
IDiscrepancyRepository / DiscrepancyRepository
DiscrepancyAudit
ReconciliationRecord
ApprovalStatus
```

Diagram cần thể hiện một `ReconciliationRecord` có chênh lệch có thể tạo một hồ sơ `DiscrepancyAudit` và chỉ Owner/Executive được phê duyệt.

### 4.4. Class Diagram: Analytics & Reporting

Các class/interface chính:

```text
AnalyticsController
IAnalyticsService / AnalyticsService
IAnalyticsRepository / AnalyticsRepository
ExecutiveKpisResponse
DailyTrendPointResponse
ChannelBreakdownResponse
TopSkuResponse
DrilldownOrderResponse
```

Diagram phải nêu rõ Analytics chỉ tổng hợp đơn có trạng thái `DELIVERED`.

### 4.5. Class Diagram: Shared Domain & Security

Các thành phần chung:

```text
AppUser, Role, Policy
Money
OrderStatus
ReconciliationStatus
ApprovalStatus
ChannelCode
GlobalExceptionMiddleware
```

### 4.6. Nội dung bắt buộc của mọi class diagram

1. Tên class/interface C# và project/folder tương ứng.
2. Public method chính; không cần liệt kê toàn bộ private method.
3. Quan hệ `implements`, `depends on`, `has-a`, `one-to-many`.
4. DTO, entity, enum và repository liên quan.
5. Dependency rule: `FashionWeb.Business` không phụ thuộc `FashionWeb.Api` hoặc `FashionWeb.Data`.
6. Strategy Pattern, Repository Pattern và dependency injection thể hiện đúng vị trí.

---

## 5. Giai đoạn 2 — Thiết kế Sequence Diagrams

Sequence diagram mô tả thứ tự xử lý từ thao tác React đến response từ API/database. Mỗi diagram cần có happy path, validation/error path và RBAC khi phù hợp.

### 5.1. Danh sách sequence diagrams

1. Preview phí và tạo đơn đa kênh.
2. Chuyển trạng thái `PENDING -> SHIPPED -> DELIVERED` và tạo immutable fee snapshot.
3. Hủy đơn và chặn hủy đơn `DELIVERED`.
4. Import CSV/XLSX, SHA-256 deduplication, parse và reconciliation.
5. Tạo discrepancy audit và Owner phê duyệt/từ chối.
6. Dashboard KPI, filter, drilldown và export CSV.

### 5.2. Cấu trúc chung của sequence diagram

```text
React Component
-> ASP.NET Controller
-> Business Service
-> Strategy/Parser khi có
-> Repository
-> AppDbContext/PostgreSQL
-> Response DTO
-> React refresh hoặc invalidate query
```

### 5.3. Ví dụ sequence: giao thành công

```text
OrdersPage / OrdersTable
-> OrdersController.UpdateStatus()
-> OrderService.TransitionToDeliveredAsync()
-> kiểm tra trạng thái hiện tại là SHIPPED
-> FeeStrategyFactory.Resolve()
-> IPlatformFeeStrategy.CalculateFees()
-> tạo OrderFeeSnapshot bất biến
-> OrderRepository.SaveAsync()
-> AppDbContext.SaveChangesAsync()
-> OrderDetailResponse
```

Các nhánh cần mô tả:

- Nếu đơn không ở trạng thái `SHIPPED`, trả `409 Conflict`.
- Nếu role không đủ quyền, trả `403 Forbidden`.
- Nếu thao tác thành công, snapshot được lưu trong cùng transaction với cập nhật trạng thái đơn.

---

## 6. Giai đoạn 3 — State và Component Diagrams

### 6.1. State Diagram: Order Lifecycle

```text
PENDING -> SHIPPED -> DELIVERED
PENDING -> CANCELLED
SHIPPED -> CANCELLED
```

Quy tắc: `DELIVERED` là trạng thái cuối; doanh thu chỉ được ghi nhận khi đơn đạt `DELIVERED`.

### 6.2. State Diagram: Settlement Lifecycle

```text
PENDING_SETTLEMENT
    -> RECONCILED
    -> DISCREPANCY
DISCREPANCY -> PENDING_APPROVAL -> APPROVED | REJECTED
```

### 6.3. Component/Layer Diagram

```text
React SPA
   -> REST/JSON
FashionWeb.Api
   -> FashionWeb.Business
FashionWeb.Data
   -> PostgreSQL / File Storage
```

Diagram phải thể hiện rõ `Api` và `Data` cùng phụ thuộc vào `Business`; Business layer không phụ thuộc ngược lại.

---

## 7. Giai đoạn 4 — API Traceability và kiểm tra chéo

Tạo bảng mapping cho tất cả endpoint trong OpenAPI.

| Endpoint | Controller | Service | Repository | Entity/Table | Rule chính |
|---|---|---|---|---|---|
| `POST /orders` | `OrdersController.CreateOrder` | `OrderService` | `OrderRepository` | `orders`, `order_items` | Voucher hợp lệ; POS có thể tự DELIVERED |
| `PATCH /orders/{id}/status` | `OrdersController.UpdateStatus` | `OrderService` | `OrderRepository` | `orders`, `order_fee_snapshots` | Chỉ SHIPPED được chuyển DELIVERED |
| `POST /settlement/statements/import` | `SettlementsController.ImportStatement` | `StatementImportService` | import/reconciliation repositories | statement imports/lines | SHA-256, parse, matching |
| `POST /discrepancies` | `DiscrepanciesController.CreateAudit` | `DiscrepancyService` | `DiscrepancyRepository` | discrepancy audits | Lệch tiền bắt buộc có giải trình |
| `GET /analytics/kpis` | `AnalyticsController.GetKpis` | `AnalyticsService` | `AnalyticsRepository` | delivered orders/snapshots | Chỉ tổng hợp DELIVERED |

Với mỗi endpoint, kiểm tra đủ các điểm:

1. Route và HTTP method.
2. Controller action.
3. Request/response DTO.
4. Role/policy.
5. Service và repository.
6. Entity/table tác động.
7. Status code, validation và business rule.

---

## 8. Đầu ra cần nộp

```text
docs/architecture/
|-- component_diagram_dotnet.md
|-- class_diagram_orders_fees.md
|-- class_diagram_settlement.md
|-- class_diagram_discrepancy_analytics.md
|-- sequence_diagrams.md
`-- state_diagrams.md

docs/api/
`-- api_traceability.md
```

Tất cả diagrams nên sử dụng Mermaid để có thể review, render trực tiếp trên GitHub/VS Code và cập nhật cùng mã nguồn.

---

## 9. Definition of Done

Phần thiết kế diagrams được xem là hoàn thành khi:

1. Mỗi endpoint OpenAPI map rõ `Controller -> Service -> Repository -> Entity/Table`.
2. Mỗi class trong diagram có vị trí tương ứng trong folder structure .NET.
3. Có class diagrams riêng cho Orders/Fees, Settlement, Discrepancy và Analytics.
4. Có sequence diagrams cho toàn bộ nghiệp vụ trọng yếu.
5. Có state diagram cho Order và Settlement.
6. Diagrams tuân thủ React + ASP.NET Core 3-tier + EF Core/PostgreSQL.
7. Các financial invariants và RBAC được thể hiện rõ.
8. Diagrams render thành công và sẵn sàng để mentor review trước khi bước sang phần code/test.
