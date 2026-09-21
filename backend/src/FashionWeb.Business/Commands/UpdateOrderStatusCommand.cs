using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Commands;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    OrderProgressStatus ToStatus,
    string ActorIdentity
);
