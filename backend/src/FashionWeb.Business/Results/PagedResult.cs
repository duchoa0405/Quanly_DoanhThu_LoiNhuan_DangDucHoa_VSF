namespace FashionWeb.Business.Results;

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalItems,
    int TotalPages
)
{
    public PagedResult(IReadOnlyList<T> items, int totalItems, int page, int pageSize)
        : this(
            items,
            page,
            pageSize,
            totalItems,
            pageSize > 0 ? (int)Math.Ceiling(totalItems / (double)pageSize) : 0
        )
    {
    }
}
