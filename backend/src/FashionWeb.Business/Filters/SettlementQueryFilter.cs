using FashionWeb.Business.Domain.Enums;

namespace FashionWeb.Business.Filters;

public record SettlementQueryFilter(
    ChannelType? Channel = null,
    ReconciliationStatus? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    string? Search = null,
    int Page = 1,
    int PageSize = 20
);
