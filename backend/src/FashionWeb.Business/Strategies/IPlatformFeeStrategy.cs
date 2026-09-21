using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Strategies;

public interface IPlatformFeeStrategy
{
    ChannelType Channel { get; }
    FeeBreakdown Calculate(decimal subtotal, decimal voucher, FeeSchedule schedule);
}
