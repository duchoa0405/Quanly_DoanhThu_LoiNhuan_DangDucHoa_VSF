# 02 — Class Diagram: Orders & Fee Engine

> **Project:** FASHION-WEB — Multi-Channel Revenue & Cash Flow Settlement Management  
> **Module:** Orders Management (SCR-01 / MOD-01 / MOD-02) & Dynamic Fee Engine  
> **Layer Scope:** Presentation (`FashionWeb.Api`), Business (`FashionWeb.Business`), Data (`FashionWeb.Data`)

---

## 1. Architectural Role & Responsibilities

This bounded context governs multi-channel order ingestion, lifecycle state transitions, live fee unbundling, and immutable financial snapshotting:
1. **Zero Phantom Revenue:** Revenue is recognized strictly upon `DELIVERED` status.
2. **Strategy Pattern for Fees:** Channel deduction formulas (TikTok Shop, Shopee, POS) are decoupled behind `IPlatformFeeStrategy`.
3. **Immutable Snapshot Pattern:** When an order transitions to `DELIVERED`, computed fees are frozen forever into `OrderFeeSnapshot`.

---

## 2. Orders & Fee Engine Class Diagram

```mermaid
classDiagram
    direction TB

    %% ===================================================
    %% PRESENTATION TIER: FashionWeb.Api
    %% ===================================================
    class OrdersController {
        <<Controller>>
        -IOrderService _orderService
        -IFeeEngine _feeEngine
        +PreviewFee(FeePreviewRequest request) Task~ActionResult~
        +GetOrders(string channel, string status, int page, int pageSize) Task~ActionResult~
        +GetOrderById(Guid id) Task~ActionResult~
        +CreateOrder(CreateOrderRequest request) Task~ActionResult~
        +UpdateStatus(Guid id, UpdateOrderStatusRequest request) Task~ActionResult~
        +CancelOrder(Guid id, CancelOrderRequest request) Task~ActionResult~
    }

    class FeePreviewRequest {
        <<DTO Request>>
        +string ChannelCode
        +decimal Subtotal
        +decimal ShopVoucher
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
        +string PaymentMethod
        +decimal GrossSubtotal
        +decimal ShopVoucher
        +decimal CustomerPaid
        +DateTime OrderedAt
        +DateTime DeliveredAt
        +List~OrderItemResponse~ Items
        +FeeBreakdownResponse FeeSnapshot
    }

    class FeeBreakdownResponse {
        <<DTO Response>>
        +decimal CommissionFee
        +decimal PaymentFee
        +decimal ServiceFee
        +decimal TotalPlatformFees
        +decimal ExpectedNetPayout
    }

    %% ===================================================
    %% BUSINESS TIER: FashionWeb.Business
    %% ===================================================
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
        -FreezeFeeSnapshot(Order order, FeeBreakdown fees) OrderFeeSnapshot
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

    class Order {
        <<Domain Entity>>
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

    %% ===================================================
    %% DATA TIER: FashionWeb.Data
    %% ===================================================
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

    %% ===================================================
    %% RELATIONSHIPS & DEPENDENCIES
    %% ===================================================
    OrdersController ..> IOrderService : calls
    OrdersController ..> IFeeEngine : previews fee
    OrdersController ..> CreateOrderRequest : consumes
    OrdersController ..> OrderDetailResponse : produces

    IOrderService <|.. OrderService : implements
    OrderService --> IOrderRepository : persists via
    OrderService --> IFeeEngine : delegates fee calculation
    OrderService ..> Order : manages lifecycle

    IFeeEngine <|.. DynamicFeeEngine : implements
    DynamicFeeEngine --> FeeStrategyFactory : resolves strategy
    FeeStrategyFactory o-- IPlatformFeeStrategy : aggregates

    IPlatformFeeStrategy <|.. TikTokShopFeeStrategy : implements
    IPlatformFeeStrategy <|.. ShopeeFeeStrategy : implements
    IPlatformFeeStrategy <|.. PosFeeStrategy : implements
    IPlatformFeeStrategy ..> FeeBreakdown : returns

    IOrderRepository <|.. OrderRepository : implements
    OrderRepository --> AppDbContext : executes EF Core queries

    Order "1" *-- "1..*" OrderItem : contains
    Order "1" o-- "0..1" OrderFeeSnapshot : freezes upon DELIVERED
```

---

## 3. Core Design Patterns & Business Invariants

### 3.1. Strategy Pattern: Platform Fee Engine
* **Formula Isolation:**
  * **TikTok Shop:** `Commission = 4.0% * Subtotal`, `Payment = 3.0% * CustomerPaid`, `Fixed = 2,000 VND`.
  * **Shopee:** `Commission = 4.5% * Subtotal`, `Payment = 4.0% * CustomerPaid`, `Freeship Xtra = Min(2.0% * Subtotal, 20,000 VND)`.
  * **In-Store POS:** `Card/QR Swipe = 1.0% * CustomerPaid`, `Cash = 0 VND`.
* **Extensibility:** Onboarding a new channel (e.g., Lazada, Tiki) requires creating 1 new class implementing `IPlatformFeeStrategy` without modifying `OrderService` (Open/Closed Principle).

### 3.2. Immutable Snapshot Pattern
* When an order transitions to `DELIVERED`, `OrderService` calls `IFeeEngine`, instantiates `OrderFeeSnapshot`, and sets `IsImmutable = true`.
* Once saved, historical fee deductions remain permanent even if platform commission schedules change in the future.

### 3.3. Financial Arithmetic Constraint
* 100% of monetary properties use C# `decimal` mapped to PostgreSQL `NUMERIC(18,0)` VND. Floating-point types (`float`, `double`) are strictly prohibited.
