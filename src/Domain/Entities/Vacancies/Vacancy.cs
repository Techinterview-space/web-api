using System;
using System.Collections.Generic;
using Domain.Entities.Companies;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Vacancies;

public class Vacancy : HasDatesBase, IHasIdBase<Guid>, IHasDeletedAt
{
    public const int TitleMaxLength = 200;

    public const int CompanyNameMaxLength = 200;

    public Vacancy(
        string title,
        Company company,
        User author,
        string hrContact,
        string description,
        VacancyStatus status,
        string companyName,
        bool hideAttachedCompany)
    {
        if (author == null)
        {
            throw new ArgumentNullException(nameof(author));
        }

        var normalizedCompanyName = NormalizeCompanyName(companyName);
        ValidateCompanyAssignment(company, normalizedCompanyName, hideAttachedCompany);

        Title = ValidateTitle(title);
        SetCompany(company);
        CompanyName = normalizedCompanyName;
        HideAttachedCompany = hideAttachedCompany;
        Author = author;
        AuthorId = author.Id;
        HrContact = hrContact?.Trim();
        Description = description;
        Status = ValidateStatus(status);

        History = new List<VacancyHistory>
        {
            new VacancyHistory(this, "Vacancy was created", author.Email),
        };
    }

    protected Vacancy()
    {
    }

    public Guid Id { get; protected set; }

    public string Title { get; protected set; }

    public Guid? CompanyId { get; protected set; }

    public virtual Company Company { get; protected set; }

    public string CompanyName { get; protected set; }

    public bool HideAttachedCompany { get; protected set; }

    public long AuthorId { get; protected set; }

    public virtual User Author { get; protected set; }

    public string HrContact { get; protected set; }

    public string Description { get; protected set; }

    public VacancyStatus Status { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<VacancyHistory> History { get; protected set; }

    public bool IsPubliclyVisible()
    {
        return Status == VacancyStatus.Public &&
               DeletedAt == null;
    }

    public bool IsOwnedBy(User user)
    {
        return user != null &&
               AuthorId == user.Id;
    }

    public bool CanBeViewedBy(User userOrNull)
    {
        if (IsPubliclyVisible())
        {
            return true;
        }

        return userOrNull != null &&
               (AuthorId == userOrNull.Id || userOrNull.Has(Role.Admin));
    }

    public void CouldBeEditedByOrFail(User user)
    {
        if (!IsOwnedBy(user))
        {
            throw new NoPermissionsException("You are not allowed to edit this vacancy.");
        }
    }

    public void Update(
        string title,
        Company company,
        string hrContact,
        string description,
        VacancyStatus status,
        string companyName,
        bool hideAttachedCompany,
        User actor)
    {
        NotDeletedOrFail();

        var newTitle = ValidateTitle(title);
        var newStatus = ValidateStatus(status);
        var trimmedHrContact = hrContact?.Trim();
        var normalizedCompanyName = NormalizeCompanyName(companyName);

        ValidateCompanyAssignment(company, normalizedCompanyName, hideAttachedCompany);

        var fieldsChanged =
            Title != newTitle ||
            CompanyId != company?.Id ||
            (CompanyName ?? string.Empty) != (normalizedCompanyName ?? string.Empty) ||
            HideAttachedCompany != hideAttachedCompany ||
            (HrContact ?? string.Empty) != (trimmedHrContact ?? string.Empty) ||
            (Description ?? string.Empty) != (description ?? string.Empty);

        var oldStatus = Status;
        var statusChanged = oldStatus != newStatus;

        Title = newTitle;
        SetCompany(company);
        CompanyName = normalizedCompanyName;
        HideAttachedCompany = hideAttachedCompany;
        HrContact = trimmedHrContact;
        Description = description;
        Status = newStatus;

        if (statusChanged)
        {
            AddHistory($"Status changed {oldStatus} -> {newStatus}", actor);
        }

        if (fieldsChanged)
        {
            AddHistory("Was modified", actor);
        }
    }

    public void ChangeStatusByAdmin(
        VacancyStatus newStatus,
        User actor)
    {
        NotDeletedOrFail();

        if (newStatus != VacancyStatus.Draft && newStatus != VacancyStatus.Closed)
        {
            throw new BadRequestException("Admin can only move a vacancy to Draft or Closed status.");
        }

        if (Status == newStatus)
        {
            return;
        }

        Status = newStatus;
        AddHistory($"Admin has moved the vacancy to status {newStatus}", actor);
    }

    public void Delete(
        User actor)
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Vacancy is already deleted.");
        }

        DeletedAt = DateTimeOffset.UtcNow;
        AddHistory("Was deleted", actor);
    }

    public void Restore(
        User actor)
    {
        if (DeletedAt == null)
        {
            throw new BadRequestException("Vacancy is not deleted.");
        }

        DeletedAt = null;
        AddHistory("Admin has restored the vacancy", actor);
    }

    public void NotDeletedOrFail()
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Cannot modify a deleted vacancy.");
        }
    }

    private static string ValidateTitle(
        string title)
    {
        title = title?.Trim();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new BadRequestException("Vacancy title cannot be empty.");
        }

        if (title.Length > TitleMaxLength)
        {
            throw new BadRequestException($"Vacancy title cannot be longer than {TitleMaxLength} characters.");
        }

        return title;
    }

    private static VacancyStatus ValidateStatus(
        VacancyStatus status)
    {
        if (status == VacancyStatus.Undefined)
        {
            throw new BadRequestException("Vacancy status is invalid.");
        }

        return status;
    }

    private static string NormalizeCompanyName(
        string companyName)
    {
        var trimmed = companyName?.Trim();
        return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
    }

    private static void ValidateCompanyAssignment(
        Company company,
        string normalizedCompanyName,
        bool hideAttachedCompany)
    {
        if (company == null && normalizedCompanyName == null)
        {
            throw new BadRequestException("Vacancy must be linked to a company or have a company name.");
        }

        if (hideAttachedCompany && company == null)
        {
            throw new BadRequestException("Cannot hide the attached company when no company is linked.");
        }

        if (normalizedCompanyName != null && normalizedCompanyName.Length > CompanyNameMaxLength)
        {
            throw new BadRequestException($"Company name cannot be longer than {CompanyNameMaxLength} characters.");
        }
    }

    private void SetCompany(
        Company company)
    {
        Company = company;
        CompanyId = company?.Id;
    }

    private void AddHistory(
        string message,
        User actor)
    {
        History ??= new List<VacancyHistory>();
        History.Add(new VacancyHistory(this, message, actor?.Email));
    }
}
