namespace FashionWeb.Business.Commands;

public record UpdateProductCommand(
    Guid Id,
    string? Name,
    string? Category,
    bool? IsActive,
    string ActorIdentity
);
