using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record DiscrepancyQueryFilter(
    ChannelType? Channel = null,
    DiscrepancyType? DiscrepancyType = null,
    bool? IsResolved = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);
