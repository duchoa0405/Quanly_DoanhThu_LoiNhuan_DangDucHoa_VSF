using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Commands;

public record CreateOrderItemCommand(
    Guid ProductVariantId,
    int Quantity,
    decimal UnitPrice
);

public record CreateOrderCommand(
    string ExternalOrderId,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    string? CustomerName,
    string? CustomerPhone,
    decimal ShopVoucher,
    List<CreateOrderItemCommand> Items,
    string ActorIdentity
);
