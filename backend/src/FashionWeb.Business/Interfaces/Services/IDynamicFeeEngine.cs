using FashionWeb.Business.Domain.Entities;
using FashionWeb.Business.Domain.Enums;
using FashionWeb.Business.Domain.ValueObjects;

namespace FashionWeb.Business.Interfaces.Services;

public interface IDynamicFeeEngine
{
    Task<FeeBreakdown> CalculateFeePreviewAsync(ChannelType channel, PaymentMethod method, decimal subtotal, decimal voucher, CancellationToken ct = default);
    Task<OrderFeeSnapshot> CalculateAndFreezeFeeAsync(Order order, CancellationToken ct = default);
}
