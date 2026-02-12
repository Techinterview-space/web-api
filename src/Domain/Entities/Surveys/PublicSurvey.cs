using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Entities.Users;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;

namespace Domain.Entities.Surveys;

public class PublicSurvey : HasDatesBase, IHasIdBase<Guid>, IHasDeletedAt
{
    public const int TitleMaxLength = 500;
    public const int DescriptionMaxLength = 2000;
    public const int SlugMaxLength = 100;

    public Guid Id { get; protected set; }

    public string Title { get; protected set; }

    public string Description { get; protected set; }

    public string Slug { get; protected set; }

    public long AuthorId { get; protected set; }

    public virtual User Author { get; protected set; }

    public PublicSurveyStatus Status { get; protected set; }

    public DateTimeOffset? PublishedAt { get; protected set; }

    public DateTimeOffset? DeletedAt { get; protected set; }

    public virtual List<PublicSurveyQuestion> Questions { get; protected set; } = new ();

    public PublicSurvey(
        string title,
        string description,
        string slug,
        long authorId)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (title.Length > TitleMaxLength)
        {
            throw new BadRequestException($"Title must not exceed {TitleMaxLength} characters.");
        }

        if (description != null && description.Length > DescriptionMaxLength)
        {
            throw new BadRequestException($"Description must not exceed {DescriptionMaxLength} characters.");
        }

        if (authorId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(authorId));
        }

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        Slug = ValidateSlug(slug);
        AuthorId = authorId;
        Status = PublicSurveyStatus.Draft;
    }

    public void Publish()
    {
        NotDeletedOrFail();
        EnsureDraftOrFail();

        if (!Questions.Any())
        {
            throw new BadRequestException("Survey must have at least one question.");
        }

        if (Questions.Any(q => !q.HasValidOptions()))
        {
            throw new BadRequestException("All questions must have at least 2 options before publishing.");
        }

        Status = PublicSurveyStatus.Published;
        PublishedAt = DateTimeOffset.UtcNow;
    }

    public void Close()
    {
        NotDeletedOrFail();

        if (Status != PublicSurveyStatus.Published)
        {
            throw new BadRequestException("Only published surveys can be closed.");
        }

        Status = PublicSurveyStatus.Closed;
    }

    public void Reopen()
    {
        NotDeletedOrFail();

        if (Status != PublicSurveyStatus.Closed)
        {
            throw new BadRequestException("Only closed surveys can be reopened.");
        }

        Status = PublicSurveyStatus.Published;
    }

    public void Delete()
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Survey is already deleted.");
        }

        DeletedAt = DateTimeOffset.UtcNow;
    }

    public void Restore()
    {
        if (DeletedAt == null)
        {
            throw new BadRequestException("Survey is not deleted.");
        }

        DeletedAt = null;
    }

    public void UpdateTitle(string title)
    {
        NotDeletedOrFail();
        EnsureDraftOrFail();

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentNullException(nameof(title));
        }

        if (title.Length > TitleMaxLength)
        {
            throw new BadRequestException($"Title must not exceed {TitleMaxLength} characters.");
        }

        Title = title;
    }

    public void UpdateDescription(string description)
    {
        NotDeletedOrFail();

        if (description != null && description.Length > DescriptionMaxLength)
        {
            throw new BadRequestException($"Description must not exceed {DescriptionMaxLength} characters.");
        }

        Description = description;
    }

    public void UpdateSlug(string slug)
    {
        NotDeletedOrFail();
        EnsureDraftOrFail();
        Slug = ValidateSlug(slug);
    }

    public bool CanAcceptResponses()
    {
        return Status == PublicSurveyStatus.Published && DeletedAt == null;
    }

    public bool IsDraft()
    {
        return Status == PublicSurveyStatus.Draft;
    }

    public bool IsPublished()
    {
        return Status == PublicSurveyStatus.Published;
    }

    private void EnsureDraftOrFail()
    {
        if (Status != PublicSurveyStatus.Draft)
        {
            throw new BadRequestException("This operation is only allowed for draft surveys.");
        }
    }

    private void NotDeletedOrFail()
    {
        if (DeletedAt != null)
        {
            throw new BadRequestException("Cannot modify a deleted survey.");
        }
    }

    private static string ValidateSlug(string slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            throw new ArgumentNullException(nameof(slug));
        }

        var result = new KebabCaseSlug(slug).ToString();

        if (result.Length > SlugMaxLength)
        {
            throw new BadRequestException($"Slug must not exceed {SlugMaxLength} characters.");
        }

        return result;
    }

    protected PublicSurvey()
    {
    }
}
