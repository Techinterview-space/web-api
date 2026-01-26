using System.Collections.Generic;

namespace Domain.ValueObjects.Pagination;

// TODO Maxim: rename to Paginated
public record Pageable<T> : PaginatedListBase
{
    public IReadOnlyCollection<T> Results { get; protected set; }

    public Pageable(
        int currentPage,
        int pageSize,
        int totalItems,
        IReadOnlyCollection<T> results)
    {
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalItems = totalItems;
        Results = results;
    }
}