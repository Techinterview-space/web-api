using System;

namespace Web.Api.Features.Vacancies.UpdateVacancy;

public record UpdateVacancyCommand
{
    public UpdateVacancyCommand(
        Guid vacancyId,
        UpdateVacancyBodyRequest body)
    {
        VacancyId = vacancyId;
        Body = body;
    }

    public Guid VacancyId { get; }

    public UpdateVacancyBodyRequest Body { get; }
}
