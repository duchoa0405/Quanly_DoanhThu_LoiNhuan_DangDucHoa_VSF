using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Interfaces.Repositories;
using FashionWeb.Business.Results;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ProductVariant>> GetSelectableVariantsAsync(string? search = null, CancellationToken ct = default)
    {
        var query = _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => v.IsActive && (v.Product == null || v.Product.IsActive))
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim().ToLower();
            query = query.Where(v => v.SkuCode.ToLower().Contains(pattern) ||
                                     (v.Product != null && v.Product.Name.ToLower().Contains(pattern)));
        }

        return await query.OrderBy(v => v.SkuCode).ToListAsync(ct);
    }

    public async Task<PagedResult<Product>> GetPagedProductsAsync(string? search = null, int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        var query = _context.Products
            .Include(p => p.Variants)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = search.Trim().ToLower();
            query = query.Where(p => p.Name.ToLower().Contains(pattern) ||
                                     (p.Category != null && p.Category.ToLower().Contains(pattern)) ||
                                     p.Variants.Any(v => v.SkuCode.ToLower().Contains(pattern)));
        }

        var totalItems = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        return new PagedResult<Product>(items, totalItems, page, pageSize);
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task<ProductVariant?> GetVariantByIdAsync(Guid variantId, CancellationToken ct = default)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == variantId, ct);
    }

    public async Task<List<ProductVariant>> GetVariantsByIdsAsync(IEnumerable<Guid> variantIds, CancellationToken ct = default)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .Where(v => variantIds.Contains(v.Id))
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsSkuCodeAsync(string skuCode, CancellationToken ct = default)
    {
        var normalized = skuCode.Trim().ToLower();
        return await _context.ProductVariants.AnyAsync(v => v.SkuCode.ToLower() == normalized, ct);
    }

    public async Task AddProductAsync(Product product, CancellationToken ct = default)
    {
        await _context.Products.AddAsync(product, ct);
    }

    public Task UpdateProductAsync(Product product, CancellationToken ct = default)
    {
        _context.Products.Update(product);
        return Task.CompletedTask;
    }

    public Task UpdateVariantAsync(ProductVariant variant, CancellationToken ct = default)
    {
        _context.ProductVariants.Update(variant);
        return Task.CompletedTask;
    }
}
