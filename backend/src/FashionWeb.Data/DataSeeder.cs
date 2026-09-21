using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace FashionWeb.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(AppDbContext context)
    {
        // 1. Seed Active Fee Schedules if table is empty
        if (!await context.FeeSchedules.AnyAsync())
        {
            var feeSchedules = new List<FeeSchedule>
            {
                // TikTok Shop: 4% Commission + 3% Payment + 3,000 VND fixed fee
                new()
                {
                    Channel = ChannelType.TIKTOK,
                    PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
                    CommissionRate = 0.0400m,
                    PaymentFeeRate = 0.0300m,
                    ServiceFeeRate = 0.0000m,
                    ServiceFeeCap = null,
                    FixedFeePerOrder = 3000m,
                    EffectiveFrom = new DateOnly(2024, 1, 1),
                    EffectiveTo = null,
                    IsActive = true
                },
                // Shopee: 4.5% Commission + 4% Payment + 2% Service fee (capped at 20,000 VND)
                new()
                {
                    Channel = ChannelType.SHOPEE,
                    PaymentMethod = PaymentMethod.MARKETPLACE_WALLET,
                    CommissionRate = 0.0450m,
                    PaymentFeeRate = 0.0400m,
                    ServiceFeeRate = 0.0200m,
                    ServiceFeeCap = 20000m,
                    FixedFeePerOrder = 0m,
                    EffectiveFrom = new DateOnly(2024, 1, 1),
                    EffectiveTo = null,
                    IsActive = true
                },
                // POS Cash: 0 VND fee
                new()
                {
                    Channel = ChannelType.POS,
                    PaymentMethod = PaymentMethod.CASH,
                    CommissionRate = 0.0000m,
                    PaymentFeeRate = 0.0000m,
                    ServiceFeeRate = 0.0000m,
                    ServiceFeeCap = null,
                    FixedFeePerOrder = 0m,
                    EffectiveFrom = new DateOnly(2024, 1, 1),
                    EffectiveTo = null,
                    IsActive = true
                },
                // POS Card / QR: 1% Payment Fee
                new()
                {
                    Channel = ChannelType.POS,
                    PaymentMethod = PaymentMethod.POS_CARD_QR,
                    CommissionRate = 0.0000m,
                    PaymentFeeRate = 0.0100m,
                    ServiceFeeRate = 0.0000m,
                    ServiceFeeCap = null,
                    FixedFeePerOrder = 0m,
                    EffectiveFrom = new DateOnly(2024, 1, 1),
                    EffectiveTo = null,
                    IsActive = true
                }
            };

            await context.FeeSchedules.AddRangeAsync(feeSchedules);
            await context.SaveChangesAsync();
        }

        // 2. Seed Baseline Products & SKU Variants if table is empty
        if (!await context.Products.AnyAsync())
        {
            var p1 = new Product
            {
                Name = "Áo Thun Cotton Basic VSF",
                Category = "Áo",
                IsActive = true
            };
            p1.Variants.AddRange(new[]
            {
                new ProductVariant
                {
                    ProductId = p1.Id,
                    SkuCode = "VSF-TSHIRT-BLK-M",
                    Color = "Đen",
                    Size = "M",
                    RetailPrice = 150000m,
                    CostPrice = 65000m,
                    IsActive = true
                },
                new ProductVariant
                {
                    ProductId = p1.Id,
                    SkuCode = "VSF-TSHIRT-BLK-L",
                    Color = "Đen",
                    Size = "L",
                    RetailPrice = 150000m,
                    CostPrice = 65000m,
                    IsActive = true
                },
                new ProductVariant
                {
                    ProductId = p1.Id,
                    SkuCode = "VSF-TSHIRT-WHT-M",
                    Color = "Trắng",
                    Size = "M",
                    RetailPrice = 150000m,
                    CostPrice = 65000m,
                    IsActive = true
                }
            });

            var p2 = new Product
            {
                Name = "Quần Jeans Slimfit VSF",
                Category = "Quần",
                IsActive = true
            };
            p2.Variants.AddRange(new[]
            {
                new ProductVariant
                {
                    ProductId = p2.Id,
                    SkuCode = "VSF-JEAN-BLU-30",
                    Color = "Xanh Denim",
                    Size = "30",
                    RetailPrice = 350000m,
                    CostPrice = 160000m,
                    IsActive = true
                },
                new ProductVariant
                {
                    ProductId = p2.Id,
                    SkuCode = "VSF-JEAN-BLU-32",
                    Color = "Xanh Denim",
                    Size = "32",
                    RetailPrice = 350000m,
                    CostPrice = 160000m,
                    IsActive = true
                }
            });

            var p3 = new Product
            {
                Name = "Áo Polo Pique Cao Cấp VSF",
                Category = "Áo",
                IsActive = true
            };
            p3.Variants.AddRange(new[]
            {
                new ProductVariant
                {
                    ProductId = p3.Id,
                    SkuCode = "VSF-POLO-NVY-L",
                    Color = "Xanh Navy",
                    Size = "L",
                    RetailPrice = 220000m,
                    CostPrice = 95000m,
                    IsActive = true
                },
                new ProductVariant
                {
                    ProductId = p3.Id,
                    SkuCode = "VSF-POLO-NVY-XL",
                    Color = "Xanh Navy",
                    Size = "XL",
                    RetailPrice = 220000m,
                    CostPrice = 95000m,
                    IsActive = true
                }
            });

            await context.Products.AddRangeAsync(p1, p2, p3);
            await context.SaveChangesAsync();
        }
    }
}
