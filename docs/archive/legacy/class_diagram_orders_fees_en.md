# 02 — Class Diagrams: Orders & Fee Engine

> **Project:** FASHION-WEB — Multi-Channel Revenue & Cash Flow Settlement Management  
> **Module:** Orders Management (SCR-01 / MOD-01 / MOD-02) & Dynamic Fee Engine  
> **Layer Scope:** Presentation (`FashionWeb.Api`), Business (`FashionWeb.Business`), Data (`FashionWeb.Data`)  
> **Architecture Pattern:** Clean Architecture 3-Tier, Strategy Pattern, Immutable Snapshot

---

## 1. Architectural Scope & Decomposition

To ensure optimal readability and maintainability, this module is decomposed into **3 focused sub-diagrams**:
1. **Diagram 2.1 — 3-Tier API & Service Orchestration:** End-to-end Dependency Injection flow from Controller through Business Service to Data Repository.
2. **Diagram 2.2 — Platform Fee Strategy Pattern Engine:** Decoupled calculation engine for marketplace deductions (TikTok, Shopee, POS).
3. **Diagram 2.3 — Domain Entities & Immutable Financial Snapshot:** Domain Aggregate Root, line items, and audit-proof frozen snapshots.

---

## 2. Diagram 2.1: 3-Tier API & Service Orchestration

This diagram illustrates how client HTTP requests flow across process and layer boundaries under Clean Architecture rules:

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

## 3. Diagram 2.2: Platform Fee Strategy Pattern Engine

This diagram encapsulates channel-specific deduction algorithms behind abstract contracts, satisfying the **Open/Closed Principle (OCP)**:

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

## 4. Diagram 2.3: Domain Entities & Immutable Snapshot Model

This diagram models the financial state machine and aggregate structure governing the **Zero Phantom Revenue** rule:

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

## 5. Architectural & Financial Invariants Summary

1. **Strategy Pattern Rules:**
   - TikTok Shop: `Commission = 4.0% * Subtotal`, `Payment = 3.0% * CustomerPaid`, `Fixed = 2,000 VND`.
   - Shopee: `Commission = 4.5% * Subtotal`, `Payment = 4.0% * CustomerPaid`, `Freeship Xtra = Min(2.0% * Subtotal, 20,000 VND)`.
   - In-Store POS: `Card/QR = 1.0% * CustomerPaid`, `Cash = 0 VND`.
2. **Zero Phantom Revenue:** Revenue is recognized if and only if `Order.Status == OrderStatus.Delivered`. Pending or Shipped orders contribute exactly 0 VND to financial ledgers.
3. **Immutable Snapshot Guarantee:** Upon transitioning to `DELIVERED`, `OrderFeeSnapshot` is created and locked with `IsImmutable = true`. Future platform fee policy changes will never alter historical settled orders.
4. **Monetary Precision:** 100% of currency fields use C# `decimal` mapped to PostgreSQL `NUMERIC(18,0)` VND. Floating-point types (`float`, `double`) are strictly prohibited.
