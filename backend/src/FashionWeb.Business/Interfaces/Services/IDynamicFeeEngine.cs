using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Interfaces.Services;

public interface IDynamicFeeEngine
{
    FeeBreakdown CalculateFee(string channelCode, decimal subtotal, decimal shopVoucher);
}
