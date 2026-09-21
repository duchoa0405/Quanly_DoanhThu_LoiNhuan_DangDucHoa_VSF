# Kế Hoạch Thiết Kế Cấu Trúc Thư Mục & API Theo .NET

## 1. Mục tiêu

Tài liệu này là kế hoạch triển khai ba hạng mục còn lại của dự án **FASHION-WEB**:

1. Thiết kế cấu trúc thư mục frontend ReactJS và backend .NET theo kiến trúc 3 tầng.
2. Thiết kế API documents, class diagrams và sequence diagrams bám theo cấu trúc đó.
3. Viết API và unit/integration tests.

Kiến trúc mục tiêu được chốt là:

```text
ReactJS + ASP.NET Core Web API + Entity Framework Core + PostgreSQL
```

Yêu cầu ASP.NET Core thay thế các đề xuất FastAPI/Node.js không nhất quán trong một số tài liệu cũ. Các quy tắc nghiệp vụ, API REST, UI/UX và mô hình dữ liệu hiện có vẫn được giữ làm cơ sở.

---

## 2. Phạm vi giữ nguyên và thay đổi

| Giữ nguyên | Điều chỉnh theo .NET |
|---|---|
| Frontend ReactJS theo các domain `orders`, `fees`, `settlements`, `discrepancies`, `analytics` | Backend triển khai bằng ASP.NET Core Web API |
| Ba workspace SCR-01, SCR-02, SCR-03 và các modal nghiệp vụ | Mô hình 3 tầng: `Api -> Business -> Data` |
| REST/OpenAPI, PostgreSQL, RBAC, các business rules tài chính | EF Core, `DbContext`, repository pattern, DI và Swagger/OpenAPI |
| Quy tắc DELIVERED, snapshot bất biến, voucher, đối soát | Tiền dùng C# `decimal` và EF Core `.HasPrecision(18, 0)`; không dùng `double` |
| Prototype HTML là tham chiếu giao diện | React component thay thế prototype; Swagger là tài liệu API chạy được |

---

## 3. Cấu trúc thư mục mục tiêu

```text
Quanly_DoanhThu_LoiNhuan_DangDucHoa_VSF/
|
|-- frontend/
|   |-- src/
|   |   |-- app/                         # Router, providers, query client, app bootstrap
|   |   |-- layouts/                     # AppShell, Sidebar, Topbar, Navigation
|   |   |-- features/
|   |   |   |-- orders/                  # SCR-01: order list, create, status, cancel
|   |   |   |-- fees/                    # Fee preview, fee schedule configuration
|   |   |   |-- settlements/             # SCR-02: ledger, import statement, matching
|   |   |   |-- discrepancies/           # Audit, justification, approval
|   |   |   `-- analytics/               # SCR-03: KPI, charts, drilldown, export
|   |   |-- shared/
|   |   |   |-- api/                     # HTTP client, API errors, DTO mappers
|   |   |   |-- ui/                      # Button, Table, Modal, Badge, Money components
|   |   |   |-- auth/                    # PermissionGate, role helpers
|   |   |   |-- hooks/
|   |   |   |-- lib/                     # formatMoney, date, validation helpers
|   |   |   |-- constants/
|   |   |   `-- types/
|   |   |-- styles/                      # tokens.css, globals.css
|   |   `-- assets/
|   `-- tests/
|
|-- backend/
|   |-- FashionWeb.sln
|   |-- src/
|   |   |-- FashionWeb.Api/              # Presentation tier
|   |   |   |-- Controllers/
|   |   |   |-- Contracts/                # Request/Response DTOs
|   |   |   |-- Authorization/            # JWT, Roles, Policies
|   |   |   |-- Middleware/
|   |   |   `-- Program.cs
|   |   |
|   |   |-- FashionWeb.Business/         # Business tier
|   |   |   |-- Services/
|   |   |   |-- Interfaces/               # IOrderService, IOrderRepository, ...
|   |   |   |-- Domain/
|   |   |   |   |-- Entities/
|   |   |   |   |-- Enums/
|   |   |   |   `-- ValueObjects/         # Money, FeeBreakdown
|   |   |   `-- Strategies/              # TikTok, Shopee, POS fee strategies
|   |   |
|   |   `-- FashionWeb.Data/             # Data tier
|   |       |-- Context/                  # AppDbContext
|   |       |-- Configurations/           # EF Core Fluent configurations
|   |       |-- Repositories/
|   |       |-- Migrations/
|   |       |-- Storage/                  # Statement file + SHA-256 storage
|   |       `-- Parsers/                  # CSV/XLSX statement parsers
|   |
|   `-- tests/
|       |-- FashionWeb.Business.Tests/
|       |-- FashionWeb.Api.Tests/
|       `-- FashionWeb.Data.Tests/
|
`-- docs/
    |-- architecture/
    |-- api/
    `-- adr/
```

### 3.1. Quy tắc dependency của backend

```text
FashionWeb.Api      -> FashionWeb.Business
FashionWeb.Data     -> FashionWeb.Business
FashionWeb.Business -> không phụ thuộc FashionWeb.Api hoặc FashionWeb.Data
```

Ba project `FashionWeb.Api`, `FashionWeb.Business` và `FashionWeb.Data` là ba tầng chính. Các project test không được tính là tầng kiến trúc.

### 3.2. Quy tắc tổ chức frontend

Mỗi feature frontend có cấu trúc con thống nhất:

```text
features/orders/
|-- api/
|-- components/
|-- hooks/
|-- schemas/
|-- types/
|-- OrdersPage.tsx
`-- index.ts
```

Không tạo một thư mục `services/` phẳng chứa toàn bộ API. API theo domain phải ở feature tương ứng, ví dụ `features/orders/api/ordersApi.ts`; `shared/api` chỉ chứa HTTP client, xử lý lỗi và mapper dùng chung.

---

## 4. Mapping domain, UI và backend

| Domain | UI/API chính | Frontend | Business layer | Data layer |
|---|---|---|---|---|
| Orders | SCR-01; tạo đơn, đổi trạng thái, hủy đơn | `features/orders` | `OrderService` | `OrderRepository`, `OrderItemRepository` |
| Fees | Preview phí, cấu hình biểu phí | `features/fees` | `FeeEngine`, fee strategies | `FeeScheduleRepository` |
| Settlements | SCR-02; ledger, import sao kê, matching | `features/settlements` | `SettlementService`, `ReconciliationService` | statement/reconciliation repositories, parser, storage |
| Discrepancies | Audit, giải trình, phê duyệt | `features/discrepancies` | `DiscrepancyService` | `DiscrepancyRepository` |
| Analytics | SCR-03; KPI, chart, drilldown, CSV | `features/analytics` | `AnalyticsService` | `AnalyticsRepository` |

Luồng chung của một API:

```text
React feature -> ASP.NET Controller -> Business Service/Strategy -> Repository -> EF Core/PostgreSQL
```

Ví dụ `POST /api/v1/orders`:

```text
OrdersController
  -> IOrderService / OrderService
    -> FeeStrategyFactory
    -> IOrderRepository / OrderRepository
      -> AppDbContext
```

Controller không chứa business rule; repository không quyết định trạng thái đơn hàng hoặc phép tính phí.

---

## 5. Kế hoạch thực hiện

### Giai đoạn 0 — Chuẩn hoá quyết định kiến trúc

1. Chốt ASP.NET Core Web API, EF Core và PostgreSQL là stack backend chính.
2. Chọn một bản OpenAPI và ERD làm source of truth; hiện repository có bản trong `docs/architecture/` và bản khác ở thư mục gốc.
3. Chuẩn hoá công thức:

   ```text
   variance_amount = actual_settled_amount - expected_net_payout
   ```

   Giá trị âm biểu thị số thực nhận thấp hơn kỳ vọng.

4. Chuẩn hoá payment method: `CASH`, `CARD_QR`, `PLATFORM_WALLET`.
5. Dùng tên modal theo nghiệp vụ thay vì chỉ dùng số MOD: `CreateOrderModal`, `CancelOrderModal`, `ReconcileSettlementModal`, `ImportStatementModal`, `FeeScheduleModal`, `SourceOrderDrilldownModal`.
6. Bổ sung identity/RBAC vào phạm vi thiết kế: User, Role/Policy và authorization ở API.

Đầu ra: ADRs trong `docs/adr/` và OpenAPI/ERD đã thống nhất.

### Giai đoạn 1 — Thiết kế và scaffold folder structure

1. Tạo React application theo cấu trúc feature-first ở mục 3.
2. Tạo .NET solution gồm ba project: `FashionWeb.Api`, `FashionWeb.Business`, `FashionWeb.Data`.
3. Cấu hình reference project, dependency injection và convention đặt tên.
4. Tạo các folder rỗng/placeholder cho domain, repository, DTO, strategy, storage/parser và tests.
5. Đưa design tokens UI/UX vào `frontend/src/styles/tokens.css`; AppShell phản ánh sidebar/topbar/navigation của prototype.

Đầu ra: cấu trúc thư mục hoàn chỉnh, chưa triển khai nghiệp vụ/API thực tế.

### Giai đoạn 2 — Thiết kế API documents theo ASP.NET Core

Tạo `docs/api/api-design.md` và chuẩn hoá OpenAPI. Mỗi endpoint phải mô tả:

1. Controller/action và route ASP.NET Core.
2. Request/response DTO C#.
3. Role/policy được phép gọi.
4. Status codes: `200`, `201`, `400`, `403`, `404`, `409`, `422`.
5. Validation và business rules.
6. Mapping `Controller -> Service -> Repository`.
7. Entity/table tác động.

Catalogue controller dự kiến:

```text
OrdersController        -> IOrderService        -> IOrderRepository
SettlementsController   -> ISettlementService   -> IStatementImportRepository
                                             -> IReconciliationRepository
DiscrepanciesController -> IDiscrepancyService  -> IDiscrepancyRepository
AnalyticsController     -> IAnalyticsService    -> IAnalyticsRepository
```

Sau khi code ở giai đoạn 3, Swagger/OpenAPI do ASP.NET Core sinh ra sẽ là tài liệu API chạy được. Không duy trì nhiều file OpenAPI độc lập gây lệch contract.

### Giai đoạn 3 — Thiết kế diagrams

1. Class diagram theo C#: Controller, service interface/implementation, repository interface/implementation, EF Core entities, `AppDbContext`, fee strategies.
2. Sequence diagram cho các luồng:
   - Tạo đơn và xem trước phí.
   - Chuyển đơn sang `DELIVERED` và tạo immutable fee snapshot.
   - Import sao kê, SHA-256 deduplication và reconciliation.
   - Tạo/duyệt discrepancy audit.
   - Dashboard KPI, drilldown và CSV export.
3. Cập nhật C4/container diagram: thay FastAPI/Node bằng ASP.NET Core Web API.

### Giai đoạn 4 — Code và kiểm thử

Thứ tự triển khai:

1. Orders và lifecycle.
2. Fee Strategy Engine.
3. Settlement/import/reconciliation.
4. Discrepancy audit và approval.
5. Analytics/reporting.

Định hướng test:

- Unit tests cho Business layer bằng xUnit và mock repositories.
- Integration tests cho API controllers và EF Core/PostgreSQL test database.
- Test bắt buộc cho: voucher boundary, order lifecycle, POS auto-delivered, immutable snapshot, duplicate statement hash, variance/audit, RBAC.

---

## 6. Definition of Done cho phần 1

Phần thiết kế folder structure được xem là hoàn thành khi:

1. Frontend và backend có cây thư mục như mục 3.
2. Mỗi endpoint OpenAPI có thể map rõ `Controller -> Service -> Repository`.
3. Mỗi screen/modal có một frontend feature chịu trách nhiệm.
4. Business rules tài chính nằm trong `FashionWeb.Business`, không nằm trong controller/repository/React component.
5. Test project/folder đã được chuẩn bị để triển khai phần code và unit test sau này.
6. Stack ASP.NET Core và các quyết định nghiệp vụ mâu thuẫn đã được ghi thành ADR.

---

## 7. Tài liệu tham chiếu

- `docs/architecture/arc42_c4_en.md`
- `docs/architecture/openapi_spec.yaml`
- `docs/architecture/database_erd.md`
- `docs/uiux_specifications.md`
- `docs/information_architecture.md`
- `docs/requirements_invest.md`
- `docs/usecase.md`
- `docs/stitch_prototype.html`
