namespace FashionWeb.Api.Contracts.Common;

public record PaginationResponse<T>(
    List<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
);
