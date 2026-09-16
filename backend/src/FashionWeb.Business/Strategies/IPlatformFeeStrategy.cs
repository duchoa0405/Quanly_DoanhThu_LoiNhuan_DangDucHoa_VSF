using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public interface IPlatformFeeStrategy
{
    string ChannelCode { get; }
    FeeBreakdown CalculateFees(decimal subtotal, decimal shopVoucher);
}
