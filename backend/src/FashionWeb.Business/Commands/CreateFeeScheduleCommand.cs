using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Commands;

public record CreateFeeScheduleCommand(
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    decimal CommissionRate,
    decimal PaymentFeeRate,
    decimal ServiceFeeRate,
    decimal? ServiceFeeCap,
    decimal FixedFeePerOrder,
    DateOnly EffectiveFrom,
    string ActorIdentity
);
