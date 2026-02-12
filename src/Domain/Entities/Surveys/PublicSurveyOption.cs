using System;
using System.Collections.Generic;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Surveys;

public class PublicSurveyOption : HasDatesBase, IHasIdBase<Guid>
{
    public const int TextMaxLength = 200;

    public Guid Id { get; protected set; }

    public Guid QuestionId { get; protected set; }

    public virtual PublicSurveyQuestion Question { get; protected set; }

    public string Text { get; protected set; }

    public int Order { get; protected set; }

    public virtual List<PublicSurveyResponseOption> ResponseOptions { get; protected set; } = new ();

    public PublicSurveyOption(
        string text,
        int order,
        PublicSurveyQuestion question)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length > TextMaxLength)
        {
            throw new BadRequestException($"Option text must not exceed {TextMaxLength} characters.");
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }

        Id = Guid.NewGuid();
        Text = text;
        Order = order;
        QuestionId = question?.Id ?? throw new ArgumentNullException(nameof(question));
        Question = question;
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length > TextMaxLength)
        {
            throw new BadRequestException($"Option text must not exceed {TextMaxLength} characters.");
        }

        Text = text;
    }

    public void UpdateOrder(int order)
    {
        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }

        Order = order;
    }

    protected PublicSurveyOption()
    {
    }
}
