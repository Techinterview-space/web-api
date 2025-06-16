using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Users.SearchUsersForAdmin;

public record SearchUsersForAdminQueryParams : PageModel
{
    public string Email { get; init; } = string.Empty;

    public bool? UnsubscribeMeFromAll { get; init; }

    public bool HasEmailFilter()
        => !string.IsNullOrWhiteSpace(Email);

    public bool HasUnsubscribeFilter()
        => UnsubscribeMeFromAll.HasValue;
}