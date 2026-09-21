namespace FashionWeb.Business.Results;

public record SelectableVariantResult(
    Guid Id,
    string SkuCode,
    string ProductName,
    string? Color,
    string? Size,
    decimal RetailPrice,
    bool IsActive
);
