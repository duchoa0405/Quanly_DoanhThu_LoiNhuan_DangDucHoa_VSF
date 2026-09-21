using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Services;

public class CatalogService : ICatalogService
{
    private readonly IProductRepository _productRepository;

    public CatalogService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<SelectableVariantResult>> GetSelectableVariantsAsync(string? search = null, CancellationToken ct = default)
    {
        var variants = await _productRepository.GetSelectableVariantsAsync(search, ct);

        return variants.Select(v => new SelectableVariantResult(
            Id: v.Id,
            SkuCode: v.SkuCode,
            ProductName: v.Product?.Name ?? string.Empty,
            Color: v.Color,
            Size: v.Size,
            RetailPrice: v.RetailPrice,
            IsActive: v.IsActive
        ));
    }

    public async Task<PagedResult<Product>> ListProductsAsync(string? search = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        return await _productRepository.GetPagedProductsAsync(search, page, pageSize, ct);
    }

    public async Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _productRepository.GetByIdAsync(id, ct);
    }

    public async Task<Product> CreateProductAsync(CreateProductCommand cmd, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(cmd.Name))
            throw new ArgumentException("Product name cannot be empty.", nameof(cmd));

        if (cmd.Variants == null || cmd.Variants.Count == 0)
            throw new ArgumentException("At least one product SKU variant must be provided.", nameof(cmd));

        // Check for duplicate SKU codes within the command
        var distinctSkus = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var v in cmd.Variants)
        {
            if (string.IsNullOrWhiteSpace(v.SkuCode))
                throw new ArgumentException("SKU code cannot be empty.", nameof(cmd));

            if (!distinctSkus.Add(v.SkuCode.Trim()))
                throw new InvalidOperationException($"Duplicate SKU code '{v.SkuCode}' within product variant list.");

            if (await _productRepository.ExistsSkuCodeAsync(v.SkuCode, ct))
                throw new InvalidOperationException($"SKU code '{v.SkuCode}' already exists in catalog.");
        }

        var product = new Product
        {
            Id = Guid.NewGuid(),
            Name = cmd.Name.Trim(),
            Category = cmd.Category?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var v in cmd.Variants)
        {
            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = product.Id,
                SkuCode = v.SkuCode.Trim(),
                Color = v.Color?.Trim(),
                Size = v.Size?.Trim(),
                RetailPrice = v.RetailPrice,
                CostPrice = v.CostPrice,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            product.Variants.Add(variant);
        }

        await _productRepository.AddProductAsync(product, ct);
        return product;
    }

    public async Task<Product> UpdateProductAsync(UpdateProductCommand cmd, CancellationToken ct = default)
    {
        var product = await _productRepository.GetByIdAsync(cmd.Id, ct);
        if (product == null)
            throw new KeyNotFoundException($"Product with ID '{cmd.Id}' was not found.");

        var name = !string.IsNullOrWhiteSpace(cmd.Name) ? cmd.Name.Trim() : product.Name;
        var category = cmd.Category != null ? cmd.Category.Trim() : product.Category;
        product.UpdateMetadata(name, category);

        if (cmd.IsActive.HasValue)
        {
            if (cmd.IsActive.Value) product.Activate();
            else product.Deactivate();
        }

        await _productRepository.UpdateProductAsync(product, ct);
        return product;
    }

    public async Task<ProductVariant> UpdateVariantAsync(UpdateVariantCommand cmd, CancellationToken ct = default)
    {
        var variant = await _productRepository.GetVariantByIdAsync(cmd.VariantId, ct);
        if (variant == null)
            throw new KeyNotFoundException($"Product variant with ID '{cmd.VariantId}' was not found.");

        variant.UpdatePricing(cmd.RetailPrice, cmd.CostPrice);

        if (cmd.IsActive.HasValue)
        {
            if (cmd.IsActive.Value) variant.Activate();
            else variant.Deactivate();
        }

        await _productRepository.UpdateVariantAsync(variant, ct);
        return variant;
    }
}
