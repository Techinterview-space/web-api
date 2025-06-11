using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Companies.GetCompany;
using Xunit;

namespace Web.Api.Tests.Features.Companies.GetCompany;

public class GetCompanyHandlerTests
{
    [Fact]
    public async Task Handle_UserInterviewer_CompanyHasNoReviews_CounterIncreased()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        Assert.Equal(0, context.CompanyReviews.Count());
        Assert.Empty(company.Reviews);
        Assert.Equal(0, company.ViewsCount);

        var user = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var target = new GetCompanyHandler(
            context,
            new FakeAuth(user));

        var result = await target.Handle(
            company.Id.ToString(),
            CancellationToken.None);

        Assert.Equal(company.Id, result.Company.Id);
        Assert.Equal(1, result.Company.ViewsCount);
        Assert.False(result.UserHasAnyReview);
    }

    [Fact]
    public async Task Handle_UserAdmin_CompanyHasNoReviews_CounterNotIncreased()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        Assert.Equal(0, context.CompanyReviews.Count());
        Assert.Empty(company.Reviews);
        Assert.Equal(0, company.ViewsCount);

        var user = await new UserFake(Role.Admin)
            .PleaseAsync(context);

        var target = new GetCompanyHandler(
            context,
            new FakeAuth(user));

        var result = await target.Handle(
            company.Id.ToString(),
            CancellationToken.None);

        Assert.Equal(company.Id, result.Company.Id);
        Assert.Equal(0, result.Company.ViewsCount);
        Assert.False(result.UserHasAnyReview);
    }

    [Fact]
    public async Task Handle_Anonymus_CompanyHasNoReviews_CounterIncreased()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        Assert.Equal(0, context.CompanyReviews.Count());
        Assert.Empty(company.Reviews);
        Assert.Equal(0, company.ViewsCount);

        var target = new GetCompanyHandler(
            context,
            new FakeAuth(null));

        var result = await target.Handle(
            company.Id.ToString(),
            CancellationToken.None);

        Assert.Equal(company.Id, result.Company.Id);
        Assert.Equal(1, result.Company.ViewsCount);
        Assert.False(result.UserHasAnyReview);
    }

    [Theory]
    [InlineData(0, true)]
    [InlineData(2, true)]
    [InlineData(4, false)]
    public async Task Handle_UserInterviewer_CompanyHasReviews_Cases_Match(
        int countOfMonthAgo,
        bool expected)
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        var user = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var review1 = new CompanyReviewFake(company, user)
            .SetApprovedAt(DateTime.UtcNow)
            .SetCreatedAt(DateTimeOffset.UtcNow.AddMonths(-countOfMonthAgo))
            .Please(context);

        company = context.Companies
            .Include(x => x.Reviews)
            .First();

        Assert.Equal(1, context.CompanyReviews.Count());
        Assert.Single(company.Reviews);
        Assert.Equal(review1.Id, company.Reviews[0].Id);
        Assert.Equal(0, company.ViewsCount);

        var target = new GetCompanyHandler(
            context,
            new FakeAuth(user));

        var result = await target.Handle(
            company.Id.ToString(),
            CancellationToken.None);

        Assert.Equal(company.Id, result.Company.Id);
        Assert.Equal(1, result.Company.ViewsCount);
        Assert.Single(result.Company.Reviews);
        Assert.Equal(review1.Id, result.Company.Reviews[0].Id);
        Assert.Equal(expected, result.UserHasAnyReview);
    }
}