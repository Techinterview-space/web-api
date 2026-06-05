using System;
using Domain.Enums;

namespace Web.Api.Features.Admin.Vacancies.ChangeVacancyStatus;

public record ChangeVacancyStatusCommand
{
    public ChangeVacancyStatusCommand(
        Guid vacancyId,
        VacancyStatus status)
    {
        VacancyId = vacancyId;
        Status = status;
    }

    public Guid VacancyId { get; }

    public VacancyStatus Status { get; }
}
