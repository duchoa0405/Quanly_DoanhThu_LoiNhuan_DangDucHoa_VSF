namespace FashionWeb.Business.Common;

public static class MoneyMath
{
    public static decimal Round(decimal amount)
    {
        return Math.Round(amount, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateGrossRevenue(decimal subtotal, decimal shopVoucher)
    {
        return FinancialCalculator.CalculateGrossRevenue(subtotal, shopVoucher);
    }

    public static decimal CalculateRateFee(decimal basisAmount, decimal rate)
    {
        return Round(basisAmount * rate);
    }
}
