using FashionWeb.Business.Commands;
using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Services;

public interface ICatalogService
{
    Task<IEnumerable<SelectableVariantResult>> GetSelectableVariantsAsync(string? search = null, CancellationToken ct = default);
    Task<PagedResult<Product>> ListProductsAsync(string? search = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Product?> GetProductByIdAsync(Guid id, CancellationToken ct = default);
    Task<Product> CreateProductAsync(CreateProductCommand cmd, CancellationToken ct = default);
    Task<Product> UpdateProductAsync(UpdateProductCommand cmd, CancellationToken ct = default);
    Task<ProductVariant> UpdateVariantAsync(UpdateVariantCommand cmd, CancellationToken ct = default);
}
