using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Surveys;

public class PublicSurveyResponse : HasDatesBase, IHasIdBase<Guid>
{
    public Guid Id { get; protected set; }

    public Guid QuestionId { get; protected set; }

    public virtual PublicSurveyQuestion Question { get; protected set; }

    public long UserId { get; protected set; }

    public virtual List<PublicSurveyResponseOption> SelectedOptions { get; protected set; } = new ();

    public PublicSurveyResponse(
        PublicSurveyQuestion question,
        long userId)
    {
        if (question == null)
        {
            throw new ArgumentNullException(nameof(question));
        }

        if (userId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(userId));
        }

        Id = Guid.NewGuid();
        QuestionId = question.Id;
        Question = question;
        UserId = userId;
    }

    public void AddSelectedOption(PublicSurveyOption option)
    {
        if (option == null)
        {
            throw new ArgumentNullException(nameof(option));
        }

        if (option.QuestionId != QuestionId)
        {
            throw new BadRequestException("Option does not belong to the same question as this response.");
        }

        if (SelectedOptions.Any(x => x.OptionId == option.Id))
        {
            throw new BadRequestException("This option has already been selected.");
        }

        SelectedOptions.Add(new PublicSurveyResponseOption(Id, option.Id));
    }

    protected PublicSurveyResponse()
    {
    }
}
