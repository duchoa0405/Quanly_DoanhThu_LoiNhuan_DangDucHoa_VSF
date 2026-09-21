using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Strategies;

public class FeeStrategyFactory
{
    private readonly IEnumerable<IPlatformFeeStrategy> _strategies;

    public FeeStrategyFactory(IEnumerable<IPlatformFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPlatformFeeStrategy GetStrategy(ChannelType channel)
    {
        var strategy = _strategies.FirstOrDefault(s => s.Channel == channel);
        if (strategy == null)
            throw new NotSupportedException($"Platform fee strategy for channel '{channel}' is not supported.");
        return strategy;
    }
}
