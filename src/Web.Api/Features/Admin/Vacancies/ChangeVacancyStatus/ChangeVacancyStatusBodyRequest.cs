using Domain.Enums;

namespace Web.Api.Features.Admin.Vacancies.ChangeVacancyStatus;

public record ChangeVacancyStatusBodyRequest
{
    public VacancyStatus Status { get; init; }
}
