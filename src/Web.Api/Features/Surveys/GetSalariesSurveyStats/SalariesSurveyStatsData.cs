using System.Collections.Generic;
using Domain.Entities.Questions;

namespace Web.Api.Features.Surveys.GetSalariesSurveyStats;

public record SalariesSurveyStatsData
{
    public int CountOfRecords { get; init; }

    public List<ReplyDataItem<SurveyUsefulnessReplyType>> UsefulnessData { get; init; }

    public List<ReplyDataItem<ExpectationReplyType>> ExpectationData { get; init; }

    public record ReplyDataItem<TEnum>
        where TEnum : struct
    {
        public ReplyDataItem(
            TEnum replyType,
            SalariesSurveyStatsDataItem data)
        {
            ReplyType = replyType;
            Data = data;
        }

        public TEnum ReplyType { get; }

        public SalariesSurveyStatsDataItem Data { get; }
    }
}