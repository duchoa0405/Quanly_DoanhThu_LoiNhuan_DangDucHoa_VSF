namespace FashionWeb.Business.Commands;

public record CancelOrderCommand(
    Guid OrderId,
    string CancellationReason,
    string ActorIdentity
);
