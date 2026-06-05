using System;

namespace Web.Api.Features.Vacancies.DeleteVacancy;

public record DeleteVacancyCommand
{
    public DeleteVacancyCommand(
        Guid vacancyId)
    {
        VacancyId = vacancyId;
    }

    public Guid VacancyId { get; }
}
