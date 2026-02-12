using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Validation.Exceptions;

namespace Domain.Entities.Surveys;

public class PublicSurveyQuestion : HasDatesBase, IHasIdBase<Guid>
{
    public const int TextMaxLength = 500;
    public const int MinOptions = 2;
    public const int MaxOptions = 10;

    public Guid Id { get; protected set; }

    public Guid PublicSurveyId { get; protected set; }

    public virtual PublicSurvey PublicSurvey { get; protected set; }

    public string Text { get; protected set; }

    public int Order { get; protected set; }

    public bool AllowMultipleChoices { get; protected set; }

    public virtual List<PublicSurveyOption> Options { get; protected set; } = new ();

    public virtual List<PublicSurveyResponse> Responses { get; protected set; } = new ();

    public PublicSurveyQuestion(
        string text,
        int order,
        PublicSurvey survey,
        bool allowMultipleChoices = false)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length > TextMaxLength)
        {
            throw new BadRequestException($"Question text must not exceed {TextMaxLength} characters.");
        }

        if (order < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(order));
        }

        Id = Guid.NewGuid();
        Text = text;
        Order = order;
        AllowMultipleChoices = allowMultipleChoices;
        PublicSurveyId = survey?.Id ?? throw new ArgumentNullException(nameof(survey));
        PublicSurvey = survey;
    }

    public PublicSurveyOption AddOption(string text, int order)
    {
        if (Options.Count >= MaxOptions)
        {
            throw new BadRequestException($"A question cannot have more than {MaxOptions} options.");
        }

        var option = new PublicSurveyOption(text, order, this);
        Options.Add(option);
        return option;
    }

    public void RemoveOption(Guid optionId)
    {
        var option = Options.FirstOrDefault(x => x.Id == optionId);
        if (option == null)
        {
            throw new NotFoundException($"Option {optionId} not found.");
        }

        if (Options.Count <= MinOptions)
        {
            throw new BadRequestException($"A question must have at least {MinOptions} options.");
        }

        Options.Remove(option);
    }

    public bool HasValidOptions()
    {
        return Options.Count >= MinOptions;
    }

    public void UpdateText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentNullException(nameof(text));
        }

        if (text.Length > TextMaxLength)
        {
            throw new BadRequestException($"Question text must not exceed {TextMaxLength} characters.");
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

    public void SetAllowMultipleChoices(bool allow)
    {
        AllowMultipleChoices = allow;
    }

    protected PublicSurveyQuestion()
    {
    }
}
