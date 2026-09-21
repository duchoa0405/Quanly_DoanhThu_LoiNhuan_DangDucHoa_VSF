using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record DrilldownFilter(
    DateTime FromDate,
    DateTime ToDate,
    ChannelType? Channel = null,
    int Page = 1,
    int PageSize = 20
);
