# 05 — Sơ Đồ Tuần Tự: Các Luồng Nghiệp Vụ API Trọng Yếu (Sequence Diagrams)

> **Dự án:** FASHION-WEB — Nền Tảng Quản Lý Doanh Thu & Đối Soát Bán Hàng Đa Kênh  
> **Phạm vi tầng:** Client SPA, Presentation (`FashionWeb.Api`), Business (`FashionWeb.Business`), Data (`FashionWeb.Data`), PostgreSQL 16  
> **Mẫu kiến trúc:** Clean Architecture 3 Tầng, Mẫu Chiến lược (Strategy), Kiểm soát giao dịch ACID, Kiểm tra bảo mật RBAC

---

## 1. Danh Mục Các Kịch Bản Nghiệp Vụ

Tài liệu này mô tả chi tiết trình tự gọi phương thức, điều kiện kiểm tra dữ liệu và các nhánh lỗi HTTP cho 6 luồng hoạt động chính:
1. **Kịch bản 1:** Xem trước phí sàn trực tiếp & Tạo đơn hàng đa kênh (`MOD-01`)
2. **Kịch bản 2:** Chuyển trạng thái giao hàng `DELIVERED` & Đóng băng Snapshot chi phí (`SCR-01`)
3. **Kịch bản 3:** Hủy đơn hàng & Loại trừ 100% doanh thu ảo (`MOD-02`)
4. **Kịch bản 4:** Nạp sao kê Excel, băm SHA-256 chống trùng & So khớp 2 chiều tự động (`MOD-03`)
5. **Kịch bản 5:** Lập biên bản giải trình chênh lệch & Phê duyệt cấp Chủ shop (`#DIS-002`)
6. **Kịch bản 6:** Tổng hợp 4 KPI tài chính & Truy xuất tệp CSV kiểm toán dạng Stream (`SCR-03`)

---

## 2. Kịch Bản 1: Xem Trước Phí Sàn Trực Tiếp & Tạo Đơn Hàng Đa Kênh (MOD-01)

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Nhân viên Bán hàng (Sales/Ops)
    participant UI as CreateOrderModal (MOD-01)
    participant API as OrdersController
    participant Svc as OrderService
    participant Factory as FeeStrategyFactory
    participant Strat as TikTokShopFeeStrategy
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    %% Phần A: Xem trước phí sàn
    Note over Staff, Strat: Phần A: Tính nhẩm chi phí sàn theo thời gian thực (Debounced)
    Staff->>UI: Chọn kênh "TIKTOK", nhập Tiền hàng 500.000đ, Voucher 50.000đ
    UI->>API: POST /api/v1/orders/preview-fee (FeePreviewRequest)
    API->>Factory: GetStrategy("TIKTOK")
    Factory-->>API: trả về TikTokShopFeeStrategy
    API->>Strat: CalculateFees(Subtotal: 500k, Voucher: 50k)
    Strat-->>API: trả về FeeBreakdown (Hoa hồng 4%, TT 3%, Cố định 2k, Thực thu 414k)
    API-->>UI: HTTP 200 OK (FeeBreakdownResponse)
    UI-->>Staff: Hiển thị dòng tiền dự kiến (Phí: Màu đỏ, Thực thu: Màu xanh)

    %% Phần B: Tạo và lưu đơn hàng
    Note over Staff, DB: Phần B: Xác nhận và Lưu đơn hàng mới
    Staff->>UI: Bấm nút [Tạo đơn hàng]
    UI->>API: POST /api/v1/orders (CreateOrderRequest JSON)
    
    alt Kiểm tra Voucher thất bại (Voucher > Tiền hàng)
        API-->>UI: HTTP 422 Unprocessable Entity ("Voucher không được vượt quá tổng tiền hàng")
        UI-->>Staff: Báo đỏ ô nhập voucher
    else Kiểm tra thành công
        API->>Svc: CreateOrderAsync(requestDto)
        
        alt Kênh Bán tại quầy POS (Tiền mặt / Quẹt thẻ QR)
            Note over Svc: Quy tắc lấy hàng ngay: Giao thành công tức thì
            Svc->>Svc: Thiết lập Status = DELIVERED, DeliveredAt = DateTime.UtcNow
        else Kênh Sàn TikTok Shop hoặc Shopee
            Note over Svc: Quy tắc chống doanh thu ảo: Chờ vận chuyển
            Svc->>Svc: Thiết lập Status = PENDING, Ghi nhận doanh thu = 0đ
        end
        
        Svc->>Repo: AddAsync(newOrder)
        Repo->>DB: INSERT INTO orders, order_items
        DB-->>Repo: Ghi nhận thành công
        Repo-->>Svc: Trả về Order có Id tạo tự động
        Svc-->>API: OrderDetailResponse DTO
        API-->>UI: HTTP 201 Created (OrderDetailResponse)
        UI-->>Staff: Đóng modal & nạp lại bảng đơn hàng SCR-01
    end
```

---

## 3. Kịch Bản 2: Chuyển Trạng Thái Giao Hàng DELIVERED & Đóng Băng Snapshot Chi Phí

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Nhân viên Giao vận / Điều hành
    participant UI as OrdersTable (SCR-01)
    participant API as OrdersController
    participant Svc as OrderService
    participant Factory as FeeStrategyFactory
    participant Strat as IPlatformFeeStrategy
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    Staff->>UI: Bấm nút [Giao thành công] trên đơn ORD-2026-001
    UI->>API: PATCH /api/v1/orders/{id}/status (NewStatus: "DELIVERED")

    API->>Svc: UpdateStatusAsync(id, OrderStatus.Delivered)
    Svc->>Repo: GetByIdAsync(id)
    Repo->>DB: SELECT * FROM orders WHERE id = @id
    DB-->>Repo: trả về Entity Order

    alt Chuyển bước không hợp lệ (Trạng thái hiện tại khác SHIPPED)
        Note over Svc: Ràng buộc máy trạng thái: PENDING -> SHIPPED -> DELIVERED
        Svc-->>API: Throws InvalidOperationException ("Đơn phải ở trạng thái Đang giao")
        API-->>UI: HTTP 409 Conflict ("Trạng thái chuyển đổi không hợp lệ")
        UI-->>Staff: Báo lỗi toast cảnh báo
    else Chuyển bước hợp lệ (Trạng thái hiện tại là SHIPPED)
        Svc->>Svc: Cập nhật order.Status = DELIVERED
        Svc->>Svc: Ghi nhận thời điểm order.DeliveredAt = DateTime.UtcNow
        
        Note over Svc, Strat: Đóng băng Snapshot chi phí bất biến
        Svc->>Factory: GetStrategy(order.Channel)
        Factory-->>Svc: trả về Strategy tương ứng
        Svc->>Strat: CalculateFees(order.GrossSubtotal, order.ShopVoucher)
        Strat-->>Svc: trả về FeeBreakdown
        
        Svc->>Svc: Khởi tạo OrderFeeSnapshot (IsImmutable = true, FrozenAt = UtcNow)
        Svc->>Repo: UpdateAsync(order)
        Repo->>DB: BEGIN TRANSACTION<br/>UPDATE orders SET status = 'DELIVERED', delivered_at = NOW()<br/>INSERT INTO order_fee_snapshots (...)<br/>COMMIT
        DB-->>Repo: Transaction Committed
        Repo-->>Svc: Entity Order hoàn tất
        Svc-->>API: OrderDetailResponse DTO
        API-->>UI: HTTP 200 OK (Cập nhật thành công)
        UI-->>Staff: Hiển thị badge xanh lá & kích hoạt cập nhật KPI SCR-03
    end
```

---

## 4. Kịch Bản 3: Hủy Đơn Hàng & Loại Trừ 100% Doanh Thu Ảo (MOD-02)

```mermaid
sequenceDiagram
    autonumber
    actor Staff as Nhân viên Bán hàng
    participant UI as CancelOrderModal (MOD-02)
    participant API as OrdersController
    participant Svc as OrderService
    participant Repo as OrderRepository
    participant DB as PostgreSQL 16

    Staff->>UI: Chọn lý do "Hết hàng tồn kho" & bấm [Xác nhận hủy]
    UI->>API: POST /api/v1/orders/{id}/cancel (CancelOrderRequest)

    API->>Svc: CancelOrderAsync(id, reason)
    Svc->>Repo: GetByIdAsync(id)
    Repo->>DB: SELECT * FROM orders WHERE id = @id
    DB-->>Repo: trả về Entity Order

    alt Đơn hàng đã ở trạng thái DELIVERED
        Note over Svc: Tính bất biến sổ cái: Đơn đã giao không được hủy trực tiếp
        Svc-->>API: Throws BusinessRuleException ("Không được hủy đơn đã giao")
        API-->>UI: HTTP 422 Unprocessable Entity ("Đơn đã giao thành công, phải lập phiếu hoàn tiền/trả hàng")
        UI-->>Staff: Khóa chức năng kèm cảnh báo
    else Đơn hàng ở trạng thái PENDING hoặc SHIPPED
        Svc->>Svc: Cập nhật order.Status = CANCELLED
        Svc->>Svc: Ghi nhận order.CancellationReason & CancelledAt
        Svc->>Repo: UpdateAsync(order)
        Repo->>DB: UPDATE orders SET status = 'CANCELLED', cancellation_reason = @reason
        DB-->>Repo: Thành công
        Svc-->>API: Hoàn tất
        API-->>UI: HTTP 204 No Content
        UI-->>Staff: Thông báo hủy thành công
        Note over DB: Đơn hủy đóng góp 0 VNĐ vào toàn bộ báo cáo doanh thu tài chính
    end
```

---

## 5. Kịch Bản 4: Nạp Sao Kê Excel, Băm SHA-256 & So Khớp 2 Chiều Tự Động (MOD-03)

```mermaid
sequenceDiagram
    autonumber
    actor Fin as Kế toán viên (Finance Manager)
    participant UI as ImportStatementModal (MOD-03)
    participant API as SettlementController
    participant Svc as StatementMatchingService
    participant Storage as FileStorageService
    participant Parser as ExcelStatementParser
    participant Repo as ReconciliationRepository
    participant DB as PostgreSQL 16

    Fin->>UI: Kéo thả tệp "TikTok_Payout_20260917.xlsx" & chọn kênh "TIKTOK"
    UI->>API: POST /api/v1/settlement/statements/import (multipart/form-data)

    API->>Svc: ImportAndReconcileAsync(fileStream, fileName, channelCode, user)
    Svc->>Storage: ComputeSha256(fileStream)
    Storage-->>Svc: trả về chuỗi băm sha256Checksum

    Svc->>Repo: ExistsByHashAsync(sha256Checksum)
    Repo->>DB: SELECT COUNT(1) FROM statement_imports WHERE file_hash_sha256 = @hash
    DB-->>Repo: trả về số lượng

    alt Phát hiện trùng lặp tệp sao kê
        Svc-->>API: Throws DuplicateFileException ("Tệp sao kê đã được nạp trước đó")
        API-->>UI: HTTP 409 Conflict ("Trùng lặp tệp bảng kê. Từ chối nạp lại.")
        UI-->>Fin: Báo lỗi đỏ chặn trùng lặp
    else Tệp hợp lệ (Vượt qua kiểm tra chống trùng)
        Svc->>Storage: SaveStatementFileAsync(fileStream, fileName)
        Storage-->>Svc: trả về đường dẫn lưu tệp vật lý
        
        Svc->>Parser: ParseAsync(fileStream)
        Note over Parser: Đọc 2.000 dòng Excel qua ClosedXML (thời gian <= 1.5s)
        Parser-->>Svc: trả về Danh sách StatementLine
        
        Note over Svc, Repo: Thuật toán so khớp 2 chiều tự động (thời gian <= 1.5s)
        loop Với từng dòng StatementLine
            Svc->>Repo: Truy vấn đơn DELIVERED theo external_order_id
            alt Tìm thấy đơn khớp có FeeSnapshot
                Svc->>Svc: VarianceAmount = line.PayoutAmount - snapshot.ExpectedNetPayout
                alt VarianceAmount == 0
                    Svc->>Svc: Gán Status = RECONCILED (Khớp 100%)
                else VarianceAmount != 0
                    Svc->>Svc: Gán Status = DISCREPANCY (Cảnh báo lệch tiền #DIS)
                end
            else Không tìm thấy đơn hoặc đơn chưa giao
                Svc->>Svc: Gán Status = PENDING_SETTLEMENT
            end
        end

        Svc->>Repo: AddBatchAsync(StatementImport, StatementLines, ReconciliationRecords)
        Repo->>DB: Ghi dữ liệu hàng loạt theo Transaction
        DB-->>Repo: Batch committed thành công
        Svc-->>API: StatementImportResult DTO (Tổng số dòng, Đã khớp, Bị lệch)
        API-->>UI: HTTP 201 Created (StatementImportResult)
        UI-->>Fin: Cập nhật giao diện: 1.995 Khớp 100% | 5 Lệch tiền
    end
```

---

## 6. Kịch Bản 5: Lập Biên Bản Giải Trình Chênh Lệch & Phê Duyệt Cấp Chủ Shop (#DIS-002)

```mermaid
sequenceDiagram
    autonumber
    actor Fin as Kế toán viên
    actor Owner as Chủ Shop / Ban Giám Đốc
    participant UI as Bảng SCR-02 Sổ Cái Đối Soát
    participant API as DiscrepanciesController
    participant Svc as DiscrepancyService
    participant Repo as DiscrepancyRepository
    participant DB as PostgreSQL 16

    %% Phần A: Lập biên bản giải trình
    Note over Fin, DB: Phần A: Kế toán lập hồ sơ giải trình lệch tiền
    Fin->>UI: Bấm vào dòng lệch tiền (Chênh lệch: -25.000đ)
    UI-->>Fin: Mở modal DiscrepancyAuditModal
    Fin->>UI: Nhập nguyên nhân: WEIGHT_SURCHARGE, Ghi chú: "Bưu cục phạt vượt cân 300g", URL ảnh phiếu cân
    UI->>API: POST /api/v1/discrepancies (CreateDiscrepancyRequest)
    API->>Svc: CreateAuditAsync(requestDto, createdBy: Fin.Username)
    Svc->>Repo: AddAsync(new DiscrepancyAudit, Status = PENDING_APPROVAL)
    Repo->>DB: INSERT INTO discrepancy_audits (code: "#DIS-002", ...)
    DB-->>Repo: Ghi thành công
    Svc-->>API: DiscrepancyAuditDTO
    API-->>UI: HTTP 201 Created
    UI-->>Fin: Đổi trạng thái dòng thành PENDING_APPROVAL (Thẻ vàng)

    %% Phần B: Chủ shop ký duyệt
    Note over Owner, DB: Phần B: Chủ shop kiểm tra bằng chứng và ký duyệt
    Owner->>UI: Mở màn hình SCR-02 lọc đơn CHỜ PHÊ DUYỆT
    UI-->>Owner: Hiển thị hồ sơ #DIS-002 kèm link ảnh biên bản bưu cục
    Owner->>UI: Bấm [Ký duyệt giải trình] kèm ý kiến: "Đồng ý trừ chi phí bưu cục"
    UI->>API: PATCH /api/v1/discrepancies/{id}/approve (decision: "APPROVED")

    alt Người gọi không phải Chủ Shop (Sales hoặc Finance)
        Note over API: Kiểm tra phân quyền RBAC: RequireOwnerPolicy
        API-->>UI: HTTP 403 Forbidden ("Chỉ Chủ shop mới có quyền duyệt lệch tiền")
        UI-->>Owner: Từ chối thao tác
    else Người gọi là Chủ Shop (Hợp lệ)
        API->>Svc: ApproveDiscrepancyAsync(id, requestDto, reviewer: Owner.Username)
        Svc->>Repo: GetByIdAsync(id)
        Repo->>DB: SELECT * FROM discrepancy_audits WHERE id = @id
        DB-->>Repo: trả về hồ sơ kiểm toán
        
        Svc->>Svc: Cập nhật Status = APPROVED
        Svc->>Svc: Ghi nhận ReviewedBy = Owner, ReviewedAt = UtcNow
        Svc->>Repo: UpdateAsync(audit)
        Repo->>DB: UPDATE discrepancy_audits SET status = 'APPROVED', ...
        DB-->>Repo: Hoàn tất
        Svc-->>API: DiscrepancyAuditDTO
        API-->>UI: HTTP 200 OK (Đã duyệt)
        UI-->>Owner: Thẻ chuyển sang màu xanh (APPROVED) & chính thức khóa sổ đối soát
    end
```

---

## 7. Kịch Bản 6: Tổng Hợp 4 KPI Tài Chính & Xuất Tệp CSV Dạng Stream (SCR-03)

```mermaid
sequenceDiagram
    autonumber
    actor Owner as Chủ Shop / Điều Hành
    participant UI as RevenueDashboard (SCR-03)
    participant API as AnalyticsController
    participant Svc as AnalyticsService
    participant Repo as AnalyticsRepository
    participant DB as PostgreSQL 16

    Owner->>UI: Chọn bộ lọc: "Tháng này", Kênh: "TẤT CẢ"
    UI->>API: GET /api/v1/analytics/kpis?from=2026-09-01&to=2026-09-30&channel=ALL

    API->>Svc: GetKPIsAsync(fromDate, toDate, channel)
    Svc->>Repo: GetDeliveredOrdersAggregateAsync(fromDate, toDate, channel)
    
    Note over Repo, DB: Chống doanh thu ảo: Lọc chặt chẽ điều kiện DELIVERED
    Repo->>DB: SELECT SUM(gross_subtotal - shop_voucher) as Gross,<br/>SUM(total_platform_fees) as Fees,<br/>SUM(expected_net_payout) as Net<br/>FROM orders o JOIN order_fee_snapshots s ON o.id = s.order_id<br/>WHERE o.status = 'DELIVERED' AND o.delivered_at BETWEEN @from AND @to
    DB-->>Repo: trả về dòng số liệu tổng hợp
    Repo-->>Svc: KpiAggregateData
    Svc-->>API: ExecutiveKPIsResponse DTO
    API-->>UI: HTTP 200 OK (ExecutiveKPIsResponse)
    UI-->>Owner: Renders 4 thẻ KPI (Doanh thu gộp, Phí sàn, Thực thu, Số đơn đã giao)

    %% Xuất tệp CSV
    Note over Owner, DB: Xuất báo cáo CSV dạng Stream trực tiếp
    Owner->>UI: Bấm nút [Xuất Sổ Cái CSV]
    UI->>API: GET /api/v1/analytics/export-csv
    API->>Svc: ExportReconciliationCsvAsync()
    Svc->>Repo: StreamReconciliationRecordsAsync()
    Repo->>DB: Mở con trỏ đọc tuần tự (Read-only cursor)
    DB-->>Repo: Trả về từng khối dòng dữ liệu
    Repo-->>Svc: Chuyển đổi thành dòng văn bản CSV
    Svc-->>API: Luồng Byte array stream (text/csv)
    API-->>UI: HTTP 200 OK kèm Content-Disposition: attachment; filename="Reconciliation_Report_20260917.csv"
    UI-->>Owner: Tự động tải tệp xuống máy tính (Không gây tràn bộ nhớ RAM)
```
