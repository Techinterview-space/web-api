using System;

namespace Web.Api.Features.Surveys.GetUserSalariesSurveyData;

public record GetUserSalariesSurveyDataResponse
{
    public GetUserSalariesSurveyDataResponse(
        DateTimeOffset? lastSurveyReplyDate)
    {
        LastSurveyReplyDate = lastSurveyReplyDate;
        HasRecentSurveyReply = lastSurveyReplyDate.HasValue;
    }

    public bool HasRecentSurveyReply { get; }

    public DateTimeOffset? LastSurveyReplyDate { get; }
}