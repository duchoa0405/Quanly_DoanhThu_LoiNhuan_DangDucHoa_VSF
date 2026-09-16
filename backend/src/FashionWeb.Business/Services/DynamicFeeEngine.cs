using FashionWeb.Business.Domain.ValueObjects;
using FashionWeb.Business.Interfaces.Services;
using FashionWeb.Business.Strategies;

namespace FashionWeb.Business.Services;

public class DynamicFeeEngine : IDynamicFeeEngine
{
    private readonly FeeStrategyFactory _strategyFactory;

    public DynamicFeeEngine(FeeStrategyFactory strategyFactory)
    {
        _strategyFactory = strategyFactory;
    }

    public FeeBreakdown CalculateFee(string channelCode, decimal subtotal, decimal shopVoucher)
    {
        var strategy = _strategyFactory.GetStrategy(channelCode);
        return strategy.CalculateFees(subtotal, shopVoucher);
    }
}
