using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record TopSkuFilter(
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    ChannelType? Channel = null,
    string? SortBy = null,
    int Limit = 10
);
