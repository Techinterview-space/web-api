using System;
using System.ComponentModel.DataAnnotations;
using Domain.Enums;

namespace Web.Api.Features.Vacancies.UpdateVacancy;

public record UpdateVacancyBodyRequest
{
    [Required]
    public string Title { get; init; }

    public Guid? CompanyId { get; init; }

    public string CompanyName { get; init; }

    public bool HideAttachedCompany { get; init; }

    public string HrContact { get; init; }

    public string Description { get; init; }

    public VacancyStatus Status { get; init; }
}
