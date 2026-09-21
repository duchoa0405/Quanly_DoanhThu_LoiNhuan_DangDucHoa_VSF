using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record OrderQueryFilter(
    ChannelType? Channel = null,
    OrderStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);
