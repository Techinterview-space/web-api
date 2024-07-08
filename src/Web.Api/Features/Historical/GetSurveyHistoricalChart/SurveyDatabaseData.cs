using System;
using Domain.Entities.Enums;
using Domain.Entities.Questions;
using Domain.Entities.Salaries;
using Domain.Enums;

namespace Web.Api.Features.Historical.GetSurveyHistoricalChart;

public record SurveyDatabaseData
{
    public SurveyUsefulnessReplyType UsefulnessReply { get; init; }

    public ExpectationReplyType ExpectationReply { get; init; }

    public UserLastSalaryData LastSalaryOrNull { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public record UserLastSalaryData
    {
        public KazakhstanCity? City { get; init; }

        public CompanyType CompanyType { get; init; }

        public DeveloperGrade? Grade { get; init; }

        public DateTimeOffset CreatedAt { get; init; }
    }
}