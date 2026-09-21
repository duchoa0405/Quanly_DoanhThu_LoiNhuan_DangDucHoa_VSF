using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record TopSkuFilter(
    DateTime FromDate,
    DateTime ToDate,
    ChannelType? Channel = null,
    int Limit = 5
);
