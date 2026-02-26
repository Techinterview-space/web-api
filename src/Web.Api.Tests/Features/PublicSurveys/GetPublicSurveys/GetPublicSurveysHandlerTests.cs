using System;
using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.PublicSurveys.GetPublicSurveys;
using Xunit;

namespace Web.Api.Tests.Features.PublicSurveys.GetPublicSurveys;

public class GetPublicSurveysHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsPublishedAndClosedSurveys()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var published = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Published)
            .SetPublishedAt(DateTimeOffset.UtcNow.AddDays(-1));

        var closed = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Closed)
            .SetPublishedAt(DateTimeOffset.UtcNow.AddDays(-2));

        context.PublicSurveys.Add(published);
        context.PublicSurveys.Add(closed);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveysHandler(context)
            .Handle(new GetPublicSurveysQuery { Page = 1, PageSize = 20 }, default);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_ExcludesDraftSurveys()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var draft = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Draft);

        var published = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Published)
            .SetPublishedAt(DateTimeOffset.UtcNow);

        context.PublicSurveys.Add(draft);
        context.PublicSurveys.Add(published);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveysHandler(context)
            .Handle(new GetPublicSurveysQuery { Page = 1, PageSize = 20 }, default);

        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task Handle_ExcludesDeletedSurveys()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var active = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Published)
            .SetPublishedAt(DateTimeOffset.UtcNow);

        var deleted = new PublicSurveyFake(user.Id)
            .SetStatus(PublicSurveyStatus.Published)
            .SetPublishedAt(DateTimeOffset.UtcNow)
            .SetDeletedAt(DateTimeOffset.UtcNow);

        context.PublicSurveys.Add(active);
        context.PublicSurveys.Add(deleted);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveysHandler(context)
            .Handle(new GetPublicSurveysQuery { Page = 1, PageSize = 20 }, default);

        Assert.Equal(1, result.TotalItems);
    }

    [Fact]
    public async Task Handle_PaginationWorks()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        for (var i = 0; i < 5; i++)
        {
            var survey = new PublicSurveyFake(user.Id)
                .SetStatus(PublicSurveyStatus.Published)
                .SetPublishedAt(DateTimeOffset.UtcNow.AddDays(-i));

            context.PublicSurveys.Add(survey);
        }

        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();

        var result = await new GetPublicSurveysHandler(context)
            .Handle(new GetPublicSurveysQuery { Page = 1, PageSize = 2 }, default);

        Assert.Equal(5, result.TotalItems);
        Assert.Equal(2, result.Results.Count);
    }
}
