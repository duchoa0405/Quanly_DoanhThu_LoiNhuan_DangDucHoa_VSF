namespace FashionWeb.Api.Contracts.Catalog;

public record CreateProductVariantRequest(
    string SkuCode,
    string? Color,
    string? Size,
    decimal RetailPrice,
    decimal CostPrice
);

public record CreateProductRequest(
    string Name,
    string? Category,
    List<CreateProductVariantRequest> Variants
);

public record UpdateProductRequest(
    string? Name,
    string? Category,
    bool? IsActive
);

public record UpdateVariantRequest(
    decimal? RetailPrice,
    decimal? CostPrice,
    bool? IsActive
);

public record ProductVariantResponse(
    Guid Id,
    Guid ProductId,
    string SkuCode,
    string? Color,
    string? Size,
    decimal RetailPrice,
    decimal CostPrice,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductResponse(
    Guid Id,
    string Name,
    string? Category,
    bool IsActive,
    List<ProductVariantResponse> Variants,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record SelectableVariantResponse(
    Guid Id,
    string SkuCode,
    string ProductName,
    string? Color,
    string? Size,
    decimal RetailPrice,
    bool IsActive
);

public record SelectableVariantListResponse(
    List<SelectableVariantResponse> Variants
);

public record PagedProductListResponse(
    List<ProductResponse> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
