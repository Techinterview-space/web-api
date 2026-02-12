using System;
using Domain.Entities.Surveys;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Fakes;
using Xunit;

namespace Domain.Tests.Entities.Surveys;

public class PublicSurveyTests
{
    [Fact]
    public void Constructor_ValidParams_CreatesDraftSurvey()
    {
        var survey = new PublicSurveyFake(authorId: 1, title: "My Survey", slug: "my-survey");

        Assert.Equal("My Survey", survey.Title);
        Assert.Equal("my-survey", survey.Slug);
        Assert.Equal(1, survey.AuthorId);
        Assert.Equal(PublicSurveyStatus.Draft, survey.Status);
        Assert.Null(survey.PublishedAt);
        Assert.Null(survey.DeletedAt);
        Assert.True(survey.IsDraft());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptyTitle_Throws(string title)
    {
        Assert.Throws<ArgumentNullException>(() =>
            new PublicSurvey(title, "desc", "valid-slug", 1));
    }

    [Fact]
    public void Constructor_TitleTooLong_Throws()
    {
        var longTitle = new string('a', 501);

        Assert.Throws<BadRequestException>(() =>
            new PublicSurveyFake(title: longTitle));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_NullOrEmptySlug_Throws(string slug)
    {
        Assert.ThrowsAny<Exception>(() =>
            new PublicSurvey("Valid Title", "desc", slug, 1));
    }

    [Fact]
    public void Constructor_ZeroAuthorId_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new PublicSurveyFake(authorId: 0));
    }

    [Fact]
    public void Publish_DraftWithValidQuestions_BecomesPublished()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        survey.Questions.Add(question);
        question.AddOption("Option A", 0);
        question.AddOption("Option B", 1);

        survey.Publish();

        Assert.Equal(PublicSurveyStatus.Published, survey.Status);
        Assert.NotNull(survey.PublishedAt);
        Assert.True(survey.IsPublished());
        Assert.True(survey.CanAcceptResponses());
    }

    [Fact]
    public void Publish_DraftWithNoQuestions_Throws()
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<BadRequestException>(() => survey.Publish());
    }

    [Fact]
    public void Publish_DraftWithQuestionLessThanTwoOptions_Throws()
    {
        var survey = new PublicSurveyFake();
        var question = new PublicSurveyQuestionFake(survey);
        survey.Questions.Add(question);
        question.AddOption("Only one", 0);

        Assert.Throws<BadRequestException>(() => survey.Publish());
    }

    [Fact]
    public void Publish_AlreadyPublished_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        Assert.Throws<BadRequestException>(() => survey.Publish());
    }

    [Fact]
    public void Close_Published_BecomesClosed()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        survey.Close();

        Assert.Equal(PublicSurveyStatus.Closed, survey.Status);
        Assert.False(survey.CanAcceptResponses());
    }

    [Fact]
    public void Close_Draft_Throws()
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<BadRequestException>(() => survey.Close());
    }

    [Fact]
    public void Reopen_Closed_BecomesPublished()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Closed);

        survey.Reopen();

        Assert.Equal(PublicSurveyStatus.Published, survey.Status);
    }

    [Fact]
    public void Reopen_Published_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        Assert.Throws<BadRequestException>(() => survey.Reopen());
    }

    [Fact]
    public void Reopen_Draft_Throws()
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<BadRequestException>(() => survey.Reopen());
    }

    [Fact]
    public void Delete_NotDeleted_SetsDeletedAt()
    {
        var survey = new PublicSurveyFake();

        survey.Delete();

        Assert.NotNull(survey.DeletedAt);
    }

    [Fact]
    public void Delete_AlreadyDeleted_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.Throws<BadRequestException>(() => survey.Delete());
    }

    [Fact]
    public void Restore_Deleted_ClearsDeletedAt()
    {
        var survey = new PublicSurveyFake()
            .SetDeletedAt(DateTimeOffset.UtcNow);

        survey.Restore();

        Assert.Null(survey.DeletedAt);
    }

    [Fact]
    public void Restore_NotDeleted_Throws()
    {
        var survey = new PublicSurveyFake();

        Assert.Throws<BadRequestException>(() => survey.Restore());
    }

    [Fact]
    public void UpdateTitle_Draft_UpdatesTitle()
    {
        var survey = new PublicSurveyFake();

        survey.UpdateTitle("New Title");

        Assert.Equal("New Title", survey.Title);
    }

    [Fact]
    public void UpdateTitle_Published_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        Assert.Throws<BadRequestException>(() => survey.UpdateTitle("New Title"));
    }

    [Fact]
    public void UpdateTitle_Deleted_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.Throws<BadRequestException>(() => survey.UpdateTitle("New Title"));
    }

    [Fact]
    public void UpdateDescription_Draft_UpdatesDescription()
    {
        var survey = new PublicSurveyFake();

        survey.UpdateDescription("New description");

        Assert.Equal("New description", survey.Description);
    }

    [Fact]
    public void UpdateDescription_Published_UpdatesDescription()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        survey.UpdateDescription("Updated description");

        Assert.Equal("Updated description", survey.Description);
    }

    [Fact]
    public void UpdateDescription_Deleted_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.Throws<BadRequestException>(() => survey.UpdateDescription("New desc"));
    }

    [Fact]
    public void UpdateSlug_Draft_UpdatesSlug()
    {
        var survey = new PublicSurveyFake();

        survey.UpdateSlug("new-slug");

        Assert.Equal("new-slug", survey.Slug);
    }

    [Fact]
    public void UpdateSlug_Published_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        Assert.Throws<BadRequestException>(() => survey.UpdateSlug("new-slug"));
    }

    [Fact]
    public void CanAcceptResponses_PublishedAndNotDeleted_True()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published);

        Assert.True(survey.CanAcceptResponses());
    }

    [Fact]
    public void CanAcceptResponses_PublishedButDeleted_False()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published)
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.False(survey.CanAcceptResponses());
    }

    [Fact]
    public void CanAcceptResponses_Draft_False()
    {
        var survey = new PublicSurveyFake();

        Assert.False(survey.CanAcceptResponses());
    }

    [Fact]
    public void Publish_Deleted_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.Throws<BadRequestException>(() => survey.Publish());
    }

    [Fact]
    public void Close_Deleted_Throws()
    {
        var survey = new PublicSurveyFake()
            .SetStatus(PublicSurveyStatus.Published)
            .SetDeletedAt(DateTimeOffset.UtcNow);

        Assert.Throws<BadRequestException>(() => survey.Close());
    }
}
