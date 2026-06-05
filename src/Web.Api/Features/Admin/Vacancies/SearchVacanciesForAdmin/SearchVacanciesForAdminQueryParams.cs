using Domain.Enums;
using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Admin.Vacancies.SearchVacanciesForAdmin;

public record SearchVacanciesForAdminQueryParams : PageModel
{
    public string SearchQuery { get; init; } = string.Empty;

    public VacancyStatus? Status { get; init; }

    public bool Deleted { get; init; }
}
