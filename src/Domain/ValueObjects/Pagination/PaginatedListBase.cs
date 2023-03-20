namespace Domain.ValueObjects.Pagination;

public abstract record PaginatedListBase
{
    public int CurrentPage { get; protected set; }

    public int PageSize { get; protected set; }

    public int TotalItems { get; protected set; }
}