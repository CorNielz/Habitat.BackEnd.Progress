namespace Habitat.BackEnd.Progress.Application.Common;

public sealed class PagedResponse<T>
{
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalItems { get; init; }
    public int TotalPages { get; init; }
    public IReadOnlyCollection<T> Items { get; init; } = Array.Empty<T>();

    public static PagedResponse<T> Create(IReadOnlyCollection<T> items, int totalItems, PaginationRequest pagination)
    {
        var pageSize = pagination.SafePageSize;
        return new PagedResponse<T>
        {
            Page = pagination.SafePage,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize),
            Items = items
        };
    }
}
