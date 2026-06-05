using System;
using Domain.Entities.Vacancies;
using Domain.Enums;

namespace Web.Api.Features.Vacancies.Dtos;

public record VacancyDto
{
    public VacancyDto()
    {
    }

    public VacancyDto(
        Vacancy vacancy)
    {
        Id = vacancy.Id;
        Title = vacancy.Title;
        HrContact = vacancy.HrContact;
        Description = vacancy.Description;
        Status = vacancy.Status;
        CompanyId = vacancy.CompanyId;
        CompanyNameText = vacancy.CompanyName;
        HideAttachedCompany = vacancy.HideAttachedCompany;
        CompanyName = vacancy.CompanyName
            ?? (vacancy.HideAttachedCompany ? null : vacancy.Company?.Name);
        CompanySlug = !vacancy.HideAttachedCompany && vacancy.Company != null && vacancy.Company.DeletedAt == null
            ? vacancy.Company.Slug
            : null;
        CompanyIsDeleted = !vacancy.HideAttachedCompany && vacancy.Company != null && vacancy.Company.DeletedAt != null;
        AuthorId = vacancy.AuthorId;
        CreatedAt = vacancy.CreatedAt;
        UpdatedAt = vacancy.UpdatedAt;
        DeletedAt = vacancy.DeletedAt;
    }

    public Guid Id { get; init; }

    public string Title { get; init; }

    public string HrContact { get; init; }

    public string Description { get; init; }

    public VacancyStatus Status { get; init; }

    public Guid? CompanyId { get; init; }

    public string CompanyName { get; init; }

    public string CompanyNameText { get; init; }

    public bool HideAttachedCompany { get; init; }

    public string CompanySlug { get; init; }

    public bool CompanyIsDeleted { get; init; }

    public long AuthorId { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset UpdatedAt { get; init; }

    public DateTimeOffset? DeletedAt { get; init; }
}
