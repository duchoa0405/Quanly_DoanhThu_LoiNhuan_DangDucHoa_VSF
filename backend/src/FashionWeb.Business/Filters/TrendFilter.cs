using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record TrendFilter(
    DateTime FromDate,
    DateTime ToDate,
    ChannelType? Channel = null
);
