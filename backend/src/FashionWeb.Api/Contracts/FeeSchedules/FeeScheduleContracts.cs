using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Api.Contracts.FeeSchedules;

public record CreateFeeScheduleRequest(
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    decimal CommissionRate,
    decimal PaymentFeeRate,
    decimal ServiceFeeRate,
    decimal? ServiceFeeCap,
    decimal FixedFeePerOrder,
    DateOnly EffectiveFrom
);

public record FeeScheduleResponse(
    Guid Id,
    ChannelType Channel,
    PaymentMethod PaymentMethod,
    decimal CommissionRate,
    decimal PaymentFeeRate,
    decimal ServiceFeeRate,
    decimal? ServiceFeeCap,
    decimal FixedFeePerOrder,
    DateOnly EffectiveFrom,
    DateOnly? EffectiveTo,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record FeeScheduleListResponse(
    List<FeeScheduleResponse> Schedules
);
