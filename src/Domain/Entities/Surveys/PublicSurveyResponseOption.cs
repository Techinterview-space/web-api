using System;

namespace Domain.Entities.Surveys;

public class PublicSurveyResponseOption
{
    public Guid ResponseId { get; protected set; }

    public virtual PublicSurveyResponse Response { get; protected set; }

    public Guid OptionId { get; protected set; }

    public virtual PublicSurveyOption Option { get; protected set; }

    public PublicSurveyResponseOption(
        Guid responseId,
        Guid optionId)
    {
        ResponseId = responseId;
        OptionId = optionId;
    }

    protected PublicSurveyResponseOption()
    {
    }
}
