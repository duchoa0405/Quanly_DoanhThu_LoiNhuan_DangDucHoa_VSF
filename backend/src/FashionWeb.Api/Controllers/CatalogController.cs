using FashionWeb.Api.Authorization;
using FashionWeb.Api.Contracts.Catalog;
using FashionWeb.Api.Mappings;
using FashionWeb.Business.Commands;
using FashionWeb.Business.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FashionWeb.Api.Controllers;

[Route("api/v1/catalog")]
public class CatalogController : BaseApiController
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("variants/selectable")]
    [Authorize(Policy = Policies.RequireSalesOps)]
    public async Task<ActionResult<SelectableVariantListResponse>> GetSelectableVariants([FromQuery] string? search, CancellationToken ct)
    {
        var results = await _catalogService.GetSelectableVariantsAsync(search, ct);
        var dtos = results.Select(CatalogContractMapper.MapToSelectableVariantResponse).ToList();

        return Ok(new SelectableVariantListResponse(dtos));
    }

    [HttpGet("products")]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<PagedProductListResponse>> ListProducts(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var paged = await _catalogService.ListProductsAsync(search, page, pageSize, ct);
        var productDtos = paged.Items.Select(CatalogContractMapper.MapToProductResponse).ToList();

        return Ok(new PagedProductListResponse(
            Items: productDtos,
            Page: paged.Page,
            PageSize: paged.PageSize,
            TotalItems: paged.TotalItems,
            TotalPages: paged.TotalPages
        ));
    }

    [HttpGet("products/{id:guid}")]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<ProductResponse>> GetProductById(Guid id, CancellationToken ct)
    {
        var product = await _catalogService.GetProductByIdAsync(id, ct);
        if (product == null)
            return NotFound(new ProblemDetails { Title = "Product Not Found", Detail = $"Product with ID '{id}' was not found.", Status = 404 });

        return Ok(CatalogContractMapper.MapToProductResponse(product));
    }

    [HttpPost("products")]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<ProductResponse>> CreateProduct([FromBody] CreateProductRequest request, CancellationToken ct)
    {
        var cmd = new CreateProductCommand(
            Name: request.Name,
            Category: request.Category,
            Variants: request.Variants.Select(v => new CreateVariantItem(
                SkuCode: v.SkuCode,
                Color: v.Color,
                Size: v.Size,
                RetailPrice: v.RetailPrice,
                CostPrice: v.CostPrice
            )).ToList(),
            ActorId: GetCurrentUserIdentity()
        );

        var created = await _catalogService.CreateProductAsync(cmd, ct);
        return CreatedAtAction(nameof(GetProductById), new { id = created.Id }, CatalogContractMapper.MapToProductResponse(created));
    }

    [HttpPatch("products/{id:guid}")]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<ProductResponse>> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request, CancellationToken ct)
    {
        var cmd = new UpdateProductCommand(
            Id: id,
            Name: request.Name,
            Category: request.Category,
            IsActive: request.IsActive,
            ActorId: GetCurrentUserIdentity()
        );

        var updated = await _catalogService.UpdateProductAsync(cmd, ct);
        return Ok(CatalogContractMapper.MapToProductResponse(updated));
    }

    [HttpPatch("variants/{id:guid}")]
    [Authorize(Policy = Policies.RequireFinanceManager)]
    public async Task<ActionResult<ProductVariantResponse>> UpdateVariant(Guid id, [FromBody] UpdateVariantRequest request, CancellationToken ct)
    {
        var cmd = new UpdateVariantCommand(
            VariantId: id,
            RetailPrice: request.RetailPrice,
            CostPrice: request.CostPrice,
            IsActive: request.IsActive,
            ActorId: GetCurrentUserIdentity()
        );

        var updated = await _catalogService.UpdateVariantAsync(cmd, ct);
        return Ok(CatalogContractMapper.MapToVariantResponse(updated));
    }
}
