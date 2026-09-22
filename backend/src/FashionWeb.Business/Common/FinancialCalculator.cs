using FashionWeb.Business.Exceptions;

namespace FashionWeb.Business.Common;

/// <summary>
/// Canonical financial and managerial profit calculations for revenue recognition,
/// shop voucher allocation, fee allocation, and contribution margin reporting.
/// </summary>
public static class FinancialCalculator
{
    public static decimal CalculateGrossRevenue(decimal subtotal, decimal shopVoucher)
    {
        if (subtotal < 0m)
            throw new ValidationException("Subtotal cannot be negative.");

        if (shopVoucher < 0m)
            throw new ValidationException("Shop voucher cannot be negative.");

        if (shopVoucher > subtotal)
            throw new ValidationException($"Shop voucher ({shopVoucher:N2}) cannot exceed subtotal ({subtotal:N2}).");

        return MoneyMath.Round(subtotal - shopVoucher);
    }

    public static decimal AllocateVoucher(decimal shopVoucher, decimal lineTotal, decimal orderSubtotal)
    {
        if (orderSubtotal <= 0m || shopVoucher <= 0m || lineTotal <= 0m)
            return 0.00m;

        var ratio = lineTotal / orderSubtotal;
        return MoneyMath.Round(shopVoucher * ratio);
    }

    public static decimal AllocateFees(decimal totalPlatformFees, decimal lineTotal, decimal orderSubtotal)
    {
        if (orderSubtotal <= 0m || totalPlatformFees <= 0m || lineTotal <= 0m)
            return 0.00m;

        var ratio = lineTotal / orderSubtotal;
        return MoneyMath.Round(totalPlatformFees * ratio);
    }

    public static decimal CalculateSkuGrossRevenue(decimal lineTotal, decimal allocatedVoucher)
    {
        return MoneyMath.Round(lineTotal - allocatedVoucher);
    }

    public static decimal CalculateSkuProfit(decimal lineTotal, decimal allocatedVoucher, decimal allocatedFees, decimal cogs)
    {
        var lineGrossRevenue = CalculateSkuGrossRevenue(lineTotal, allocatedVoucher);
        return MoneyMath.Round(lineGrossRevenue - allocatedFees - cogs);
    }

    public static decimal CalculateOrderProfit(decimal grossRevenue, decimal totalPlatformFees, decimal cogs)
    {
        var netPayout = grossRevenue - totalPlatformFees;
        return MoneyMath.Round(netPayout - cogs);
    }

    public static decimal CalculateContributionMargin(decimal contributionProfit, decimal grossRevenue)
    {
        if (grossRevenue <= 0m)
            return 0.00m;

        return Math.Round((contributionProfit / grossRevenue) * 100m, 2, MidpointRounding.AwayFromZero);
    }
}
