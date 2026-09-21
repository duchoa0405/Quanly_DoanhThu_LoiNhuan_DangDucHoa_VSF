namespace FashionWeb.Business.Domain.Entities;

public class ProductVariant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProductId { get; set; }
    public string SkuCode { get; set; } = string.Empty;
    public string? Color { get; set; }
    public string? Size { get; set; }
    public decimal RetailPrice { get; set; }
    public decimal CostPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Product? Product { get; set; }

    public void UpdatePricing(decimal? retailPrice, decimal? costPrice)
    {
        if (retailPrice.HasValue)
        {
            if (retailPrice.Value < 0)
                throw new ArgumentException("Retail price cannot be negative.", nameof(retailPrice));
            RetailPrice = retailPrice.Value;
        }

        if (costPrice.HasValue)
        {
            if (costPrice.Value < 0)
                throw new ArgumentException("Cost price cannot be negative.", nameof(costPrice));
            CostPrice = costPrice.Value;
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }
}
