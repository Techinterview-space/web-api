using System;

namespace Web.Api.Features.Admin.Vacancies.RestoreVacancy;

public record RestoreVacancyCommand
{
    public RestoreVacancyCommand(
        Guid vacancyId)
    {
        VacancyId = vacancyId;
    }

    public Guid VacancyId { get; }
}
