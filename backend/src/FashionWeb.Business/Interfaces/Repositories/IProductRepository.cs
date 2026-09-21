using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Results;

namespace FashionWeb.Business.Interfaces.Repositories;

public interface IProductRepository
{
    Task<IEnumerable<ProductVariant>> GetSelectableVariantsAsync(string? search = null, CancellationToken ct = default);
    Task<PagedResult<Product>> GetPagedProductsAsync(string? search = null, int page = 1, int pageSize = 20, CancellationToken ct = default);
    Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken ct = default);
    Task<List<ProductVariant>> GetVariantsByIdsAsync(IEnumerable<Guid> variantIds, CancellationToken ct = default);
    Task<bool> ExistsSkuCodeAsync(string skuCode, CancellationToken ct = default);
    Task AddProductAsync(Product product, CancellationToken ct = default);
    Task UpdateProductAsync(Product product, CancellationToken ct = default);
    Task UpdateVariantAsync(ProductVariant variant, CancellationToken ct = default);
}
