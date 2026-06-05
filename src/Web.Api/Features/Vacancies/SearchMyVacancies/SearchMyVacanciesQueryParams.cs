using Domain.Enums;
using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Vacancies.SearchMyVacancies;

public record SearchMyVacanciesQueryParams : PageModel
{
    public string SearchQuery { get; init; } = string.Empty;

    public VacancyStatus? Status { get; init; }

    public bool Deleted { get; init; }
}
