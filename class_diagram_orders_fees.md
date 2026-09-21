# 02 — Sơ Đồ Lớp: Quản Lý Đơn Hàng & Động Cơ Tính Phí Sàn (Orders & Fee Engine)

> **Dự án:** FASHION-WEB — Nền Tảng Quản Lý Doanh Thu & Đối Soát Bán Hàng Đa Kênh  
> **Phân hệ:** Quản lý Đơn hàng (SCR-01 / MOD-01 / MOD-02) & Tính phí sàn linh hoạt  
> **Phạm vi tầng:** Presentation (`FashionWeb.Api`), Business (`FashionWeb.Business`), Data (`FashionWeb.Data`)  
> **Mẫu kiến trúc:** Clean Architecture 3 Tầng, Strategy Pattern, Immutable Snapshot

---

## 1. Phân Rã Kiến Trúc Thành 3 Sơ Đồ Con

Để đảm bảo tính trực quan, rõ ràng và dễ review nhất, phân hệ này được phân rã thành **3 sơ đồ con chuyên biệt**:
1. **Sơ đồ 2.1 — Điều phối Dịch vụ & Luồng API 3 Tầng:** Thể hiện luồng Dependency Injection từ Controller qua Business Service đến Data Repository.
2. **Sơ đồ 2.2 — Động cơ Bóc tách Phí sàn (Strategy Pattern):** Đóng gói công thức chiết khấu của các sàn (TikTok, Shopee, POS) độc lập.
3. **Sơ đồ 2.3 — Mô hình Thực thể & Ảnh chụp Chi phí Bất biến:** Cấu trúc Aggregate Root `Order`, `OrderItem` và bản ghi snapshot chống doanh thu ảo.

---

## 2. Sơ Đồ 2.1: Điều Phối Dịch Vụ & Luồng API 3 Tầng

```mermaid
classDiagram
    direction TB

    class OrdersController {
        <<Controller>>
        -IOrderService _orderService
        +GetOrders(string channel, string status, int page, int pageSize) Task~ActionResult~
        +GetOrderById(Guid id) Task~ActionResult~
        +CreateOrder(CreateOrderRequest request) Task~ActionResult~
        +UpdateStatus(Guid id, UpdateOrderStatusRequest request) Task~ActionResult~
        +CancelOrder(Guid id, CancelOrderRequest request) Task~ActionResult~
    }

    class CreateOrderRequest {
        <<DTO Request>>
        +string ChannelCode
        +string ExternalOrderId
        +string PaymentMethod
        +string CustomerName
        +string CustomerPhone
        +decimal ShopVoucher
        +List~CreateOrderItemRequest~ Items
    }

    class CreateOrderItemRequest {
        <<DTO Request>>
        +string SkuCode
        +string ProductName
        +int Quantity
        +decimal UnitPrice
    }

    class UpdateOrderStatusRequest {
        <<DTO Request>>
        +string NewStatus
    }

    class CancelOrderRequest {
        <<DTO Request>>
        +string Reason
    }

    class OrderDetailResponse {
        <<DTO Response>>
        +Guid Id
        +string OrderCode
        +string ExternalOrderId
        +string ChannelCode
        +string Status
        +decimal GrossSubtotal
        +decimal ShopVoucher
        +decimal CustomerPaid
        +DateTime OrderedAt
        +DateTime DeliveredAt
    }

    class IOrderService {
        <<Interface>>
        +GetOrdersAsync(string channel, string status, int page, int pageSize) Task~PaginatedResult~
        +GetOrderByIdAsync(Guid id) Task~Order~
        +CreateOrderAsync(CreateOrderRequest request) Task~Order~
        +UpdateStatusAsync(Guid id, OrderStatus newStatus) Task~Order~
        +CancelOrderAsync(Guid id, string reason) Task
    }

    class OrderService {
        <<Business Service>>
        -IOrderRepository _orderRepo
        -IFeeEngine _feeEngine
        +GetOrdersAsync(string channel, string status, int page, int pageSize) Task~PaginatedResult~
        +GetOrderByIdAsync(Guid id) Task~Order~
        +CreateOrderAsync(CreateOrderRequest request) Task~Order~
        +UpdateStatusAsync(Guid id, OrderStatus newStatus) Task~Order~
        +CancelOrderAsync(Guid id, string reason) Task
    }

    class IOrderRepository {
        <<Repository Interface>>
        +GetByIdAsync(Guid id) Task~Order~
        +GetByExternalIdAsync(string externalId) Task~Order~
        +GetAllAsync(string channel, string status, int page, int pageSize) Task~List~
        +AddAsync(Order order) Task
        +UpdateAsync(Order order) Task
        +SaveChangesAsync() Task
    }

    class OrderRepository {
        <<Repository Implementation>>
        -AppDbContext _context
        +GetByIdAsync(Guid id) Task~Order~
        +GetByExternalIdAsync(string externalId) Task~Order~
        +GetAllAsync(string channel, string status, int page, int pageSize) Task~List~
        +AddAsync(Order order) Task
        +UpdateAsync(Order order) Task
        +SaveChangesAsync() Task
    }

    class AppDbContext {
        <<EF Core DbContext>>
        +DbSet~Order~ Orders
        +DbSet~OrderItem~ OrderItems
        +DbSet~OrderFeeSnapshot~ OrderFeeSnapshots
        +SaveChangesAsync() Task~int~
    }

    OrdersController ..> IOrderService : calls
    OrdersController ..> CreateOrderRequest : consumes
    OrdersController ..> OrderDetailResponse : produces
    CreateOrderRequest o-- CreateOrderItemRequest : contains

    IOrderService <|.. OrderService : implements
    OrderService --> IOrderRepository : persists via
    IOrderRepository <|.. OrderRepository : implements
    OrderRepository --> AppDbContext : executes EF Core queries
```

---

## 3. Sơ Đồ 2.2: Động Cơ Bóc Tách Phí Sàn (Strategy Pattern)

```mermaid
classDiagram
    direction TB

    class OrdersController {
        <<Controller>>
        -IFeeEngine _feeEngine
        +PreviewFee(FeePreviewRequest request) Task~ActionResult~
    }

    class FeePreviewRequest {
        <<DTO Request>>
        +string ChannelCode
        +decimal Subtotal
        +decimal ShopVoucher
    }

    class FeeBreakdownResponse {
        <<DTO Response>>
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal TotalPlatformFees
        +decimal ExpectedNetPayout
    }

    class IFeeEngine {
        <<Interface>>
        +CalculateFees(string channelCode, decimal subtotal, decimal voucher) FeeBreakdown
    }

    class DynamicFeeEngine {
        <<Business Service>>
        -FeeStrategyFactory _factory
        +CalculateFees(string channelCode, decimal subtotal, decimal voucher) FeeBreakdown
    }

    class FeeStrategyFactory {
        <<Factory>>
        -IEnumerable~IPlatformFeeStrategy~ _strategies
        +GetStrategy(string channelCode) IPlatformFeeStrategy
    }

    class IPlatformFeeStrategy {
        <<Strategy Interface>>
        +string ChannelCode
        +CalculateFees(decimal subtotal, decimal voucher) FeeBreakdown
    }

    class TikTokShopFeeStrategy {
        <<Concrete Strategy>>
        -decimal CommissionRate
        -decimal PaymentFeeRate
        -decimal FixedOrderFee
        +CalculateFees(decimal subtotal, decimal voucher) FeeBreakdown
    }

    class ShopeeFeeStrategy {
        <<Concrete Strategy>>
        -decimal CommissionRate
        -decimal PaymentFeeRate
        -decimal FreeshipExtraRate
        -decimal FreeshipCap
        +CalculateFees(decimal subtotal, decimal voucher) FeeBreakdown
    }

    class PosFeeStrategy {
        <<Concrete Strategy>>
        -decimal SwipeCardRate
        +CalculateFees(decimal subtotal, decimal voucher) FeeBreakdown
    }

    class FeeBreakdown {
        <<Value Object>>
        +decimal Subtotal
        +decimal ShopVoucher
        +decimal CustomerPaid
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal TotalFees
        +decimal ExpectedNetPayout
    }

    OrdersController ..> IFeeEngine : delegates preview
    OrdersController ..> FeePreviewRequest : consumes
    OrdersController ..> FeeBreakdownResponse : returns

    IFeeEngine <|.. DynamicFeeEngine : implements
    DynamicFeeEngine --> FeeStrategyFactory : resolves strategy
    FeeStrategyFactory o-- IPlatformFeeStrategy : aggregates

    IPlatformFeeStrategy <|.. TikTokShopFeeStrategy : implements
    IPlatformFeeStrategy <|.. ShopeeFeeStrategy : implements
    IPlatformFeeStrategy <|.. PosFeeStrategy : implements
    IPlatformFeeStrategy ..> FeeBreakdown : produces
```

---

## 4. Sơ Đồ 2.3: Mô Hình Thực Thể & Ảnh Chụp Chi Phí Bất Biến

```mermaid
classDiagram
    direction TB

    class Order {
        <<Aggregate Root>>
        +Guid Id
        +string OrderCode
        +string ExternalOrderId
        +ChannelType Channel
        +OrderStatus Status
        +PaymentMethodType PaymentMethod
        +string CustomerName
        +string CustomerPhone
        +decimal GrossSubtotal
        +decimal ShopVoucher
        +decimal CustomerPaid
        +DateTime OrderedAt
        +DateTime DeliveredAt
        +DateTime CancelledAt
        +string CancellationReason
        +List~OrderItem~ Items
        +OrderFeeSnapshot FeeSnapshot
        +TransitionTo(OrderStatus newStatus) void
        +Cancel(string reason) void
    }

    class OrderItem {
        <<Domain Entity>>
        +Guid Id
        +Guid OrderId
        +string SkuCode
        +string ProductName
        +int Quantity
        +decimal UnitPrice
        +decimal LineTotal
    }

    class OrderFeeSnapshot {
        <<Domain Entity - Immutable>>
        +Guid Id
        +Guid OrderId
        +decimal CommissionFeeAmount
        +decimal PaymentFeeAmount
        +decimal ServiceFeeAmount
        +decimal TotalPlatformFees
        +decimal ExpectedNetPayout
        +bool IsImmutable
        +DateTime FrozenAt
    }

    class OrderStatus {
        <<Enumeration>>
        Pending
        Shipped
        Delivered
        Cancelled
    }

    class ChannelType {
        <<Enumeration>>
        TikTok
        Shopee
        POS
    }

    class PaymentMethodType {
        <<Enumeration>>
        Cash
        CardQR
        PlatformWallet
    }

    Order "1" *-- "1..*" OrderItem : contains
    Order "1" o-- "0..1" OrderFeeSnapshot : freezes upon DELIVERED
    Order ..> OrderStatus : tracks state
    Order ..> ChannelType : belongs to
    Order ..> PaymentMethodType : paid via
```

---

## 5. Tóm Tắt Quy Tắc Nghiệp Vụ Cốt Lõi

1. **Strategy Pattern:**
   - TikTok Shop: `Hoa hồng = 4.0% * Subtotal`, `Phí thanh toán = 3.0% * CustomerPaid`, `Phí cố định = 2.000 VNĐ`.
   - Shopee: `Hoa hồng = 4.5% * Subtotal`, `Phí thanh toán = 4.0% * CustomerPaid`, `Freeship Xtra = Min(2.0% * Subtotal, 20.000 VNĐ)`.
   - Cửa hàng POS: `Quẹt thẻ/QR = 1.0% * CustomerPaid`, `Tiền mặt = 0 VNĐ`.
2. **Chống doanh thu ảo (Zero Phantom Revenue):** Chỉ ghi nhận doanh thu khi `Order.Status == OrderStatus.Delivered`. Các đơn ở trạng thái Pending hoặc Shipped đóng góp đúng 0 VNĐ vào sổ cái.
3. **Bất biến sổ cái (Immutable Snapshot):** Ngay khi đơn hàng hoàn tất giao, `OrderFeeSnapshot` được tạo và khóa vĩnh viễn (`IsImmutable = true`). Mọi thay đổi chính sách biểu phí sau này không làm ảnh hưởng đến dữ liệu quá khứ.
4. **Độ chính xác tiền tệ:** 100% thuộc tính tiền tệ dùng kiểu `decimal` C# và lưu trữ dạng `NUMERIC(18,0)` VNĐ. Nghiêm cấm dùng số thực `float`, `double`.
