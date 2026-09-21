namespace FashionWeb.Business.Commands;

public record UpdateVariantCommand(
    Guid VariantId,
    decimal? RetailPrice,
    decimal? CostPrice,
    bool? IsActive,
    string ActorIdentity
);
