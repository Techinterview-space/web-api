using System;
using System.Collections.Generic;
using Domain.Entities.Users;

namespace Domain.Entities.Questions;

public class SalariesSurveyQuestion : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public string Title { get; protected set; }

    public string Description { get; protected set; }

    public long? CreatedByUserId { get; protected set; }

    public virtual User CreatedByUser { get; protected set; }

    public virtual List<SalariesSurveyReply> Replies { get; protected set; } = new ();

    public SalariesSurveyQuestion(
        string title,
        string description,
        User createdByUser)
    {
        Title = title;
        Description = description;
        CreatedByUserId = createdByUser?.Id;
    }

    // for seeding
    public SalariesSurveyQuestion(
        Guid id,
        string title,
        string description,
        DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        CreatedAt = UpdatedAt = createdAt;
    }

    protected SalariesSurveyQuestion()
    {
    }
}