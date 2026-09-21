using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record AnalyticsFilter(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    ChannelType? Channel = null
);
