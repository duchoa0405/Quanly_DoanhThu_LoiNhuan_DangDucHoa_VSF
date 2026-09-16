namespace FashionWeb.Business.Strategies;

public class FeeStrategyFactory
{
    private readonly IEnumerable<IPlatformFeeStrategy> _strategies;

    public FeeStrategyFactory(IEnumerable<IPlatformFeeStrategy> strategies)
    {
        _strategies = strategies;
    }

    public IPlatformFeeStrategy GetStrategy(string channelCode)
    {
        var strategy = _strategies.FirstOrDefault(s => s.ChannelCode.Equals(channelCode, StringComparison.OrdinalIgnoreCase));
        if (strategy == null)
            throw new NotSupportedException($"Kênh bán hàng '{channelCode}' chưa được hỗ trợ tính phí.");
        return strategy;
    }
}
