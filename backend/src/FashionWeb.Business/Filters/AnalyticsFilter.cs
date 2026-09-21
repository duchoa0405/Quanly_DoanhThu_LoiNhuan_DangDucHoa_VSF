using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record AnalyticsFilter(
    DateTime FromDate,
    DateTime ToDate,
    ChannelType? Channel = null
);
