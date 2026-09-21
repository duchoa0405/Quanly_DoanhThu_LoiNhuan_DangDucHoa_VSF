using FashionWeb.Api.Contracts.Catalog;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Results;

namespace FashionWeb.Api.Mappings;

public static class CatalogContractMapper
{
    public static ProductResponse MapToProductResponse(Product p)
    {
        return new ProductResponse(
            Id: p.Id,
            Name: p.Name,
            Category: p.Category,
            IsActive: p.IsActive,
            Variants: p.Variants.Select(MapToVariantResponse).ToList(),
            CreatedAt: p.CreatedAt,
            UpdatedAt: p.UpdatedAt
        );
    }

    public static ProductVariantResponse MapToVariantResponse(ProductVariant v)
    {
        return new ProductVariantResponse(
            Id: v.Id,
            ProductId: v.ProductId,
            SkuCode: v.SkuCode,
            Color: v.Color,
            Size: v.Size,
            RetailPrice: v.RetailPrice,
            CostPrice: v.CostPrice,
            IsActive: v.IsActive,
            CreatedAt: v.CreatedAt,
            UpdatedAt: v.UpdatedAt
        );
    }

    public static SelectableVariantResponse MapToSelectableVariantResponse(SelectableVariantResult v)
    {
        return new SelectableVariantResponse(
            Id: v.Id,
            SkuCode: v.SkuCode,
            ProductName: v.ProductName,
            Color: v.Color,
            Size: v.Size,
            RetailPrice: v.RetailPrice,
            IsActive: v.IsActive
        );
    }
}
