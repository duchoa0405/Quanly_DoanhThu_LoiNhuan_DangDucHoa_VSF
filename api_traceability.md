# 07 — Ma Trận Truy Vết API: Ánh Xạ Kiến Trúc Từ Đầu Đến Cuối (API Traceability Matrix)

---

## 1. Phương Pháp Luận & Phạm Vi Truy Vết

Tài liệu này thiết lập **ma trận truy vết từ đầu đến cuối 100% (End-to-End Traceability Matrix)** cho toàn bộ nền tảng FASHION-WEB. Tất cả các endpoint được định nghĩa trong tài liệu đặc tả OpenAPI 3.0 (`openapi_spec.yaml` / `openapi_spec_vi.yaml`) đều được ánh xạ xuyên suốt qua các tầng kiến trúc:

```
[Thành phần Giao diện / Modal] (React Frontend)
       │
       ▼ (HTTP REST / JSON)
[ASP.NET Core Controller Action] (`FashionWeb.Api.Controllers`)
       │
       ▼ (Gọi qua Interface - Nguyên lý DIP)
[Dịch vụ Nghiệp vụ Miền] (`FashionWeb.Business.Services`)
       │
       ├─► [Động cơ Phí Strategy / Parser Sao Kê] (`FashionWeb.Business.Strategies / Parsers`)
       │
       ▼ (Giao diện Truy cập Dữ liệu)
[EF Core Repository] (`FashionWeb.Data.Repositories`)
       │
       ▼ (Ánh xạ Thực thể Quan hệ)
[Các Bảng Cơ Sở Dữ Liệu PostgreSQL] (`orders`, `snapshots`, `settlement`, ...)
       │
       ▼ (Kiểm tra Tuân thủ)
[Bất biến Tài chính & Chính sách Phân quyền RBAC]
```

### Các Nguyên Tắc & Bất Biến Tài Chính Cốt Lõi:
1. **Chống Doanh Thu Ảo (Zero Phantom Revenue Invariant):** Doanh thu **chỉ được ghi nhận khi và chỉ khi** đơn hàng ở trạng thái `DELIVERED` (Đã giao hàng). Các đơn ở trạng thái `PENDING`, `SHIPPED` và `CANCELLED` đóng góp tuyệt đối **0 VNĐ** vào doanh thu.
2. **Độ Chính Xác Số Học Tuyệt Đối:** 100% số tiền lưu kiểu số nguyên PostgreSQL `NUMERIC(18,0)` VNĐ và kiểu C# `decimal` (tuyệt đối không dùng số thực dấu phẩy động float/double).
3. **Đóng Băng Sổ Cái Bất Biến:** Chi phí sàn được bóc tách và đóng băng vĩnh viễn vào `order_fee_snapshots` với cờ `is_immutable = true` ngay khi đơn giao thành công.
4. **Công Thức Sai Lệch Đối Soát:** $\text{Chênh lệch} = \text{Tiền thực nhận} - \text{Tiền dự kiến thu}$. Giá trị âm biểu thị khoản hụt tiền mà sàn khấu trừ thêm.
5. **Chống Trùng Tệp Bằng Mã Băm SHA-256:** Tệp sao kê bảng kê được băm mã SHA-256 trước khi phân tích cú pháp; trùng mã băm sẽ bị chặn với mã lỗi HTTP `409 Conflict`.
6. **Phân Quyền Phê Duyệt Tách Biệt (RBAC):** Nhân viên Vận hành/Bán hàng nhập đơn; Kế toán lập hồ sơ khiếu nại `#DIS-002`; chỉ **Chủ Shop (Shop Owner)** mới có thẩm quyền Ký duyệt (`Approve`) hoặc Bác bỏ (`Reject`) chênh lệch tiền ví sàn.

---

## 2. Ma Trận Ánh Xạ Chi Tiết (100% Endpoint OpenAPI)

| # | Phương Thức & Route | Mã Thao Tác (Operation ID) | Màn Hình / Modal React | Controller Action | Service Interface & Method | Repository / Component | Bảng Dữ Liệu Tác Động | Quyền Hạn (RBAC) | Ràng Buộc Nghiệp Vụ & Bất Biến |
|---|---|---|---|---|---|---|---|---|---|
| **01** | `POST /orders/preview-fee` | `previewOrderFees` | `CreateOrderModal` (`MOD-01`) | `OrdersController.PreviewFee` | `FeeStrategyFactory.GetStrategy` | `IPlatformFeeStrategy.CalculateFees` | *Chỉ xử lý trên RAM* (Không ghi DB) | `RequireSalesOrFinance` | $0 \le \text{Voucher} \le \text{Tiền hàng}$. Tính toán biểu phí động theo Strategy. |
| **02** | `GET /orders` | `listOrders` | `OrdersPage` / `OrdersTable` (`SCR-01`) | `OrdersController.GetOrders` | `IOrderService.GetOrdersAsync` | `IOrderRepository.GetPagedAsync` | `orders`, `order_items`, `order_fee_snapshots` | `RequireSalesOrFinance` | Lọc theo `channel`, `status`, từ khóa. Phân trang mặc định 20 dòng. |
| **03** | `POST /orders` | `createOrder` | `CreateOrderModal` (`MOD-01`) | `OrdersController.CreateOrder` | `IOrderService.CreateOrderAsync` | `IOrderRepository.AddAsync` | `orders`, `order_items`, `order_fee_snapshots` (nếu POS) | `RequireSalesOrFinance` | Đơn POS chuyển ngay sang `DELIVERED` & đóng băng phí; Đơn TikTok/Shopee khởi tạo `PENDING`. |
| **04** | `GET /orders/{id}` | `getOrderById` | `OrderDetailDrawer` (`SCR-01`) | `OrdersController.GetOrderById` | `IOrderService.GetOrderByIdAsync` | `IOrderRepository.GetByIdWithDetailsAsync` | `orders`, `order_items`, `order_fee_snapshots` | `RequireSalesOrFinance` | Trả về HTTP `404` nếu không tìm thấy UUID. Đính kèm snapshot khi đã `DELIVERED`. |
| **05** | `PATCH /orders/{id}/status` | `updateOrderStatus` | Thao tác dòng `OrdersTable` (`SCR-01`) | `OrdersController.UpdateStatus` | `IOrderService.UpdateStatusAsync` | `IOrderRepository.UpdateAsync`, `FeeStrategyFactory` | `orders`, `order_fee_snapshots` | `RequireSalesOrFinance` | Chuyển trạng thái nghiêm ngặt: `PENDING` $\rightarrow$ `SHIPPED` $\rightarrow$ `DELIVERED`. Nhảy cóc từ `PENDING` lên `DELIVERED` trả về `409 Conflict`. Đóng băng snapshot khi giao thành công. |
| **06** | `POST /orders/{id}/cancel` | `cancelOrder` | `CancelOrderModal` (`MOD-02`) | `OrdersController.CancelOrder` | `IOrderService.CancelOrderAsync` | `IOrderRepository.UpdateAsync` | `orders` | `RequireSalesOrFinance` | Bắt buộc nhập lý do `cancellation_reason`. Nghiêm cấm hủy đơn đã `DELIVERED` (`422 Unprocessable Entity`). |
| **07** | `GET /settlement/fee-schedules` | `listFeeSchedules` | `FeeScheduleModal` (`MOD-04`) | `SettlementController.GetFeeSchedules` | `ISettlementService.GetFeeSchedulesAsync` | `IFeeScheduleRepository.GetAllActiveAsync` | `fee_schedules`, `channels` | `RequireFinance` | Trả về toàn bộ chính sách biểu phí đang hiệu lực của `TIKTOK`, `SHOPEE`, `POS`. |
| **08** | `PUT /settlement/fee-schedules` | `updateFeeSchedule` | `FeeScheduleModal` (`MOD-04`) | `SettlementController.UpdateFeeSchedule` | `ISettlementService.UpdateFeeScheduleAsync` | `IFeeScheduleRepository.UpdateAsync` | `fee_schedules` | `RequireShopOwner` | Chỉ Chủ Shop. Kiểm tra biên độ: $0\% \le \text{tỷ lệ} \le 100\%$, Phí cố định $\ge 0$. |
| **09** | `GET /settlement/ledger` | `getSettlementLedger` | `SettlementPage` / `LedgerTable` (`SCR-02`) | `SettlementController.GetLedger` | `ISettlementService.GetLedgerAsync` | `IReconciliationRepository.GetLedgerAsync` | `orders`, `order_fee_snapshots`, `reconciliation_records`, `statement_lines` | `RequireFinance` | Tính `chênh lệch = thực nhận - kỳ vọng`. Lọc theo `recon_status` và `channel`. |
| **10** | `GET /settlement/summary` | `getSettlementSummary` | Thẻ KPI Đầu Màn `SCR-02` | `SettlementController.GetSummary` | `ISettlementService.GetSummaryAsync` | `IReconciliationRepository.GetSummaryAsync` | `reconciliation_records`, `orders` | `RequireFinance` | Tổng hợp 3 thẻ badge đối soát: Chờ đối soát (Xám), Đã khớp 100% (Xanh lá), Lệch tiền (Đỏ). |
| **11** | `POST /settlement/statements/import` | `importStatement` | `ImportStatementModal` (`MOD-03`) | `SettlementController.ImportStatement` | `IStatementImportService.ImportStatementAsync` | `IStatementImportRepository`, `IStatementParser`, `StatementMatchingService` | `statement_imports`, `statement_lines`, `reconciliation_records` | `RequireFinance` | Tính mã băm SHA-256; chặn nạp trùng file trả về `409 Conflict`. So khớp 2 chiều tự động $\le 3.0$ giây. |
| **12** | `GET /discrepancies` | `listDiscrepancies` | Drawer Hồ Sơ Kiểm Toán (`SCR-02`) | `DiscrepanciesController.GetDiscrepancies` | `IDiscrepancyService.GetDiscrepanciesAsync` | `IDiscrepancyRepository.GetPagedAsync` | `discrepancy_audits`, `reconciliation_records`, `orders` | `RequireFinance` | Lọc theo trạng thái phê duyệt: `PENDING_APPROVAL`, `APPROVED`, `REJECTED`. |
| **13** | `POST /discrepancies` | `createDiscrepancyAudit` | `DiscrepancyModal` (`#DIS-002`) | `DiscrepanciesController.CreateAudit` | `IDiscrepancyService.CreateAuditAsync` | `IDiscrepancyRepository.AddAsync` | `discrepancy_audits`, `reconciliation_records` | `RequireFinance` | Tự sinh mã `#DIS-2026-XXXX`. Bắt buộc phân loại nguyên nhân và link ảnh phiếu cân. Chuyển trạng thái sang `PENDING_APPROVAL`. |
| **14** | `PATCH /discrepancies/{id}/approve` | `approveDiscrepancy` | Drawer Duyệt Khiếu Nại (`#DIS-002`) | `DiscrepanciesController.ApproveAudit` | `IDiscrepancyService.ApproveAuditAsync` | `IDiscrepancyRepository.UpdateAsync` | `discrepancy_audits`, `reconciliation_records` | `RequireShopOwner` | Chỉ Chủ Shop. Quyết định: `APPROVED` (chấp thuận chi phí, đóng sổ) hoặc `REJECTED` (yêu cầu khiếu nại bưu cục). |
| **15** | `GET /analytics/kpis` | `getAnalyticsKPIs` | `ExecutiveKPIHeader` (`SCR-03`) | `AnalyticsController.GetKpis` | `IAnalyticsService.GetKpisAsync` | `IAnalyticsRepository.GetExecutiveKpisAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | Tổng hợp 4 thẻ KPI cấp cao. Tuyệt đối chỉ lấy đơn `status = 'DELIVERED'` (Chống doanh thu ảo). |
| **16** | `GET /analytics/trend` | `getCashFlowTrend` | Biểu đồ cột dòng tiền (`SCR-03`) | `AnalyticsController.GetTrend` | `IAnalyticsService.GetDailyCashflowTrendAsync` | `IAnalyticsRepository.GetTrendAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | So sánh chuỗi thời gian 7 ngày: Doanh thu gộp (Gross) vs. Thực thu ví sàn (Net) (chỉ đơn `DELIVERED`). |
| **17** | `GET /analytics/channel-breakdown` | `getChannelBreakdown` | Biểu đồ tròn tỷ trọng (`SCR-03`) | `AnalyticsController.GetChannelShare` | `IAnalyticsService.GetChannelShareAsync` | `IAnalyticsRepository.GetChannelShareAsync` | `orders` | `RequireExecutive` | Tỷ lệ phần trăm và tổng doanh thu gộp theo `TIKTOK`, `SHOPEE`, và `POS` (chỉ đơn `DELIVERED`). |
| **18** | `GET /analytics/top-skus` | `getTopSKUs` | Bảng xếp hạng Top 5 SKU (`SCR-03`) | `AnalyticsController.GetTopSkus` | `IAnalyticsService.GetTopSkusAsync` | `IAnalyticsRepository.GetTopSkusAsync` | `order_items`, `orders` | `RequireExecutive` | Xếp hạng giảm dần theo số lượng bán và doanh thu thực nhận (chỉ đơn `DELIVERED`). |
| **19** | `GET /analytics/drilldown` | `getDrilldownOrders` | `SourceOrderDrilldownModal` (`MOD-05`) | `AnalyticsController.GetDrilldownOrders` | `IAnalyticsService.GetDrilldownOrdersAsync` | `IAnalyticsRepository.GetDrilldownOrdersAsync` | `orders`, `order_fee_snapshots` | `RequireExecutive` | Danh sách kiểm toán chi tiết từng đơn hàng cấu thành nên số liệu tổng hợp trên thẻ KPI. |
| **20** | `GET /analytics/export-csv` | `exportReconciliationCSV` | Nút Xuất Báo Cáo (`SCR-03` / `SCR-02`) | `AnalyticsController.ExportCsv` | `IAnalyticsService.ExportReconciliationCsvAsync` | `IAnalyticsRepository.GetExportDataAsync` | `orders`, `order_fee_snapshots`, `reconciliation_records` | `RequireExecutive` | Trả về luồng byte CSV chuẩn RFC 4180 (UTF-8 BOM) để nộp kiểm toán, kế toán và thuế. Media type: `text/csv`. |

---

## 3. Bảng Ma Trận Phân Quyền Truy Cập (RBAC Matrix)

| Nhóm Endpoint | Quyền Tối Thiểu Bắt Buộc | Nhân Viên Bán Hàng & Vận Hành (`SalesOps`) | Kế Toán Viên Tài Chính (`Finance`) | Chủ Shop / Giám Đốc (`ShopOwner`) | Hành Vi Khi Không Có Quyền |
|---|---|:---:|:---:|:---:|---|
| `/orders/*` | `SalesOps` | ✅ Đọc / Ghi | ✅ Chỉ Đọc | ✅ Toàn Quyền | Trả lỗi HTTP `401 Unauthorized` |
| `/settlement/ledger` | `Finance` | ❌ Bị Chặn | ✅ Chỉ Đọc | ✅ Toàn Quyền | Trả lỗi HTTP `403 Forbidden` |
| `/settlement/statements/import` | `Finance` | ❌ Bị Chặn | ✅ Nạp & Khớp | ✅ Toàn Quyền | Trả lỗi HTTP `403 Forbidden` |
| `/settlement/fee-schedules` (GET) | `Finance` | ❌ Bị Chặn | ✅ Chỉ Đọc | ✅ Toàn Quyền | Trả lỗi HTTP `403 Forbidden` |
| `/settlement/fee-schedules` (PUT) | `ShopOwner` | ❌ Bị Chặn | ❌ Bị Chặn | ✅ Cập Nhật Biểu Phí | Trả lỗi HTTP `403 Forbidden` |
| `/discrepancies` (GET / POST) | `Finance` | ❌ Bị Chặn | ✅ Lập Biên Bản | ✅ Toàn Quyền | Trả lỗi HTTP `403 Forbidden` |
| `/discrepancies/{id}/approve` | `ShopOwner` | ❌ Bị Chặn | ❌ Bị Chặn | ✅ Phê Duyệt / Bác Bỏ | Trả lỗi HTTP `403 Forbidden` |
| `/analytics/*` | `Finance` | ❌ Bị Chặn | ✅ Chỉ Đọc | ✅ Toàn Quyền | Trả lỗi HTTP `403 Forbidden` |

---

## 4. Tiêu Chuẩn Hoàn Thành (Definition of Done)

Ma trận truy vết API cho **Giai đoạn nhỏ 7** đã đạt toàn bộ các tiêu chí nghiệm thu:
- [x] Ánh xạ đầy đủ **100% (20/20)** endpoint trong `openapi_spec_vi.yaml`.
- [x] Khớp nối chính xác với kiến trúc 3 tầng .NET (`FashionWeb.Api` $\rightarrow$ `FashionWeb.Business` $\rightarrow$ `FashionWeb.Data`).
- [x] Ràng buộc chặt chẽ nguyên tắc **Chống Doanh Thu Ảo (Zero Phantom Revenue)** trên các endpoint Đơn hàng và Phân tích.
- [x] Phân định quyền hạn RBAC rõ ràng giữa Vận hành, Kế toán và Chủ shop.
- [x] Sẵn sàng làm tiêu chuẩn kiểm thử (Acceptance Criteria) cho các bộ Test xUnit ở Bước 7.
