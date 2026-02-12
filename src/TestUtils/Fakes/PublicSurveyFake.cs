using System;
using Domain.Entities.Surveys;
using Domain.Enums;

namespace TestUtils.Fakes;

public class PublicSurveyFake : PublicSurvey
{
    public PublicSurveyFake(
        long authorId = 1,
        string title = null,
        string slug = null)
        : base(
            title ?? "Test Survey " + Guid.NewGuid().ToString("N")[..8],
            "Test survey description",
            slug ?? "test-survey-" + Guid.NewGuid().ToString("N")[..8],
            authorId)
    {
    }

    public PublicSurveyFake SetStatus(PublicSurveyStatus status)
    {
        Status = status;
        return this;
    }

    public PublicSurveyFake SetPublishedAt(DateTimeOffset publishedAt)
    {
        PublishedAt = publishedAt;
        return this;
    }

    public PublicSurveyFake SetDeletedAt(DateTimeOffset deletedAt)
    {
        DeletedAt = deletedAt;
        return this;
    }
}
