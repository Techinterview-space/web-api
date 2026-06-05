using Domain.ValueObjects.Pagination;

namespace Web.Api.Features.Vacancies.SearchVacancies;

public record SearchVacanciesQueryParams : PageModel
{
    public string SearchQuery { get; init; } = string.Empty;
}
