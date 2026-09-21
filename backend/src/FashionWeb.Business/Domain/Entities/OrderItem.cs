namespace FashionWeb.Business.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid OrderId { get; set; }
    public Guid ProductVariantId { get; set; }
    public string SkuCodeSnapshot { get; set; } = string.Empty;
    public string ProductNameSnapshot { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCostSnapshot { get; set; }
    public decimal LineTotal { get; set; }
    public decimal TotalCost { get; set; }

    public Order? Order { get; set; }
    public ProductVariant? ProductVariant { get; set; }

    public void FreezeSnapshot(string skuCode, string productName, int quantity, decimal unitPrice, decimal unitCost)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));
        if (unitCost < 0)
            throw new ArgumentException("UnitCost cannot be negative.", nameof(unitCost));

        SkuCodeSnapshot = skuCode;
        ProductNameSnapshot = productName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        UnitCostSnapshot = unitCost;
        LineTotal = quantity * unitPrice;
        TotalCost = quantity * unitCost;
    }
}
