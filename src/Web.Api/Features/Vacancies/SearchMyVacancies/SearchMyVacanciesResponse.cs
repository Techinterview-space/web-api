using System.Collections.Generic;
using Domain.ValueObjects.Pagination;
using Web.Api.Features.Vacancies.Dtos;

namespace Web.Api.Features.Vacancies.SearchMyVacancies;

public record SearchMyVacanciesResponse : Pageable<VacancyListItemDto>
{
    public SearchMyVacanciesResponse(
        int currentPage,
        int pageSize,
        int totalItems,
        IReadOnlyCollection<VacancyListItemDto> results,
        bool hasAnyVacancy)
        : base(
            currentPage,
            pageSize,
            totalItems,
            results)
    {
        HasAnyVacancy = hasAnyVacancy;
    }

    public bool HasAnyVacancy { get; init; }
}
