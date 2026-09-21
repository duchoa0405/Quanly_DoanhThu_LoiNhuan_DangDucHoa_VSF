namespace FashionWeb.Business.Commands;

public record CreateVariantItem(
    string SkuCode,
    string? Color,
    string? Size,
    decimal RetailPrice,
    decimal CostPrice
);

public record CreateProductCommand(
    string Name,
    string? Category,
    List<CreateVariantItem> Variants,
    string ActorIdentity
);
