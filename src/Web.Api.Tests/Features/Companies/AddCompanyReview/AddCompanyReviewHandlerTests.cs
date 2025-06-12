using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Companies;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using TestUtils.Services;
using Web.Api.Features.CompanyReviews.AddCompanyReview;
using Xunit;

namespace Web.Api.Tests.Features.Companies.AddCompanyReview;

public class AddCompanyReviewHandlerTests
{
    [Fact]
    public async Task Handle_UserHasNoReviews_Added()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);

        Assert.Equal(0, context.CompanyReviews.Count());

        var user = await new UserFake(Role.Interviewer)
            .PleaseAsync(context);

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.Equal(Nothing.Value, result);

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Single(reviews);

        Assert.Equal(5, reviews[0].CultureAndValues);
        Assert.Equal(4, reviews[0].CodeQuality);
        Assert.Equal(3, reviews[0].WorkLifeBalance);
        Assert.Equal(2, reviews[0].Management);
        Assert.Equal(1, reviews[0].CompensationAndBenefits);
        Assert.Equal(1, reviews[0].CareerOpportunities);
        Assert.Equal("Pros", reviews[0].Pros);
        Assert.Equal("Cons", reviews[0].Cons);
        Assert.True(reviews[0].IWorkHere);
        Assert.Equal(user.Id, reviews[0].UserId);
        Assert.Null(reviews[0].ApprovedAt);
        Assert.Null(reviews[0].OutdatedAt);

        company = context.Companies
            .Include(x => x.Reviews)
            .FirstOrDefault(x => x.Id == company.Id);

        Assert.NotNull(company);

        Assert.Single(company.Reviews);
        Assert.Equal(0, company.Rating);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(4)]
    [InlineData(0)]
    public async Task Handle_UserHasOutdatedReview_Added(
        int countOfMonthAgo)
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var company = new CompanyFake().Please(context);
        var review1 = new CompanyReviewFake(company, user)
            .SetOutdatedAt(DateTime.UtcNow.AddMonths(-countOfMonthAgo).AddDays(-1))
            .Please(context);

        Assert.Equal(1, context.CompanyReviews.Count());

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.Equal(Nothing.Value, result);

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Equal(2, reviews.Count);

        Assert.Equal(5, reviews[1].CultureAndValues);
        Assert.Equal(4, reviews[1].CodeQuality);
        Assert.Equal(3, reviews[1].WorkLifeBalance);
        Assert.Equal(2, reviews[1].Management);
        Assert.Equal(1, reviews[1].CompensationAndBenefits);
        Assert.Equal(1, reviews[1].CareerOpportunities);
        Assert.Equal("Pros", reviews[1].Pros);
        Assert.Equal("Cons", reviews[1].Cons);
        Assert.True(reviews[1].IWorkHere);
        Assert.Equal(user.Id, reviews[1].UserId);

        company = context.Companies
            .Include(x => x.Reviews)
            .FirstOrDefault(x => x.Id == company.Id);

        Assert.NotNull(company);

        Assert.Equal(2, company.Reviews.Count);
        Assert.Equal(0, company.Rating);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(4)]
    public async Task Handle_UserHasApprovedReview_Added(
        int countOfMonthAgo)
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var company = new CompanyFake().Please(context);

        var review1 = new CompanyReviewFake(company, user)
            .SetApprovedAt(DateTime.UtcNow)
            .SetCreatedAt(DateTime.UtcNow.AddMonths(-countOfMonthAgo))
            .Please(context);

        Assert.Equal(1, context.CompanyReviews.Count());

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.Equal(Nothing.Value, result);

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Equal(2, reviews.Count);

        Assert.Equal(5, reviews[1].CultureAndValues);
        Assert.Equal(4, reviews[1].CodeQuality);
        Assert.Equal(3, reviews[1].WorkLifeBalance);
        Assert.Equal(2, reviews[1].Management);
        Assert.Equal(1, reviews[1].CompensationAndBenefits);
        Assert.Equal(1, reviews[1].CareerOpportunities);
        Assert.Equal("Pros", reviews[1].Pros);
        Assert.Equal("Cons", reviews[1].Cons);
        Assert.True(reviews[1].IWorkHere);
        Assert.Equal(user.Id, reviews[1].UserId);

        company = context.Companies
            .Include(x => x.Reviews)
            .FirstOrDefault(x => x.Id == company.Id);

        Assert.NotNull(company);

        Assert.Equal(2, company.Reviews.Count);
        Assert.Equal(0, company.Rating);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    public async Task Handle_UserHasApprovedReviewRecently_Approved_Error(
        int countOfMonthAgo)
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var company = new CompanyFake().Please(context);

        var review1 = new CompanyReviewFake(company, user)
            .SetApprovedAt(DateTime.UtcNow)
            .SetCreatedAt(DateTime.UtcNow.AddMonths(-countOfMonthAgo).AddDays(1))
            .Please(context);

        Assert.Equal(1, context.CompanyReviews.Count());

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Single(reviews);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    public async Task Handle_UserHasApprovedReviewRecently_NotApproved_Error(
        int countOfMonthAgo)
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var company = new CompanyFake().Please(context);

        var review1 = new CompanyReviewFake(company, user)
            .SetCreatedAt(DateTime.UtcNow.AddMonths(-countOfMonthAgo).AddDays(1))
            .Please(context);

        Assert.Null(review1.ApprovedAt);
        Assert.Equal(1, context.CompanyReviews.Count());

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        await Assert.ThrowsAsync<BadRequestException>(() => handler.Handle(command, CancellationToken.None));

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Single(reviews);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(0)]
    public async Task Handle_UserHasApprovedReviewRecently_Outdated_Ok(
        int countOfMonthAgo)
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var company = new CompanyFake().Please(context);

        var review1 = new CompanyReviewFake(company, user)
            .SetApprovedAt(DateTime.UtcNow.AddDays(-2))
            .SetOutdatedAt(DateTime.UtcNow.AddDays(-1))
            .SetCreatedAt(DateTime.UtcNow.AddMonths(-countOfMonthAgo).AddDays(1))
            .Please(context);

        Assert.NotNull(review1.ApprovedAt);
        Assert.NotNull(review1.OutdatedAt);
        Assert.Equal(1, context.CompanyReviews.Count());

        var handler = new AddCompanyReviewHandler(
            context,
            new FakeAuth(user),
            new TelegramAdminNotificationServiceFake());

        var command = new AddCompanyReviewCommand(
            company.Id,
            new AddCompanyReviewBodyRequest
            {
                CultureAndValues = 5,
                CodeQuality = 4,
                WorkLifeBalance = 3,
                Management = 2,
                CompensationAndBenefits = 1,
                CareerOpportunities = 1,
                Pros = "Pros",
                Cons = "Cons",
                IWorkHere = true,
            });

        context.ChangeTracker.Clear();
        var result = await handler.Handle(command, CancellationToken.None);
        Assert.Equal(Nothing.Value, result);

        var reviews = context.CompanyReviews
            .Where(x => x.CompanyId == company.Id)
            .ToList();

        Assert.Equal(2, reviews.Count);

        Assert.Equal(5, reviews[1].CultureAndValues);
        Assert.Equal(4, reviews[1].CodeQuality);
        Assert.Equal(3, reviews[1].WorkLifeBalance);
        Assert.Equal(2, reviews[1].Management);
        Assert.Equal(1, reviews[1].CompensationAndBenefits);
        Assert.Equal(1, reviews[1].CareerOpportunities);
        Assert.Equal("Pros", reviews[1].Pros);
        Assert.Equal("Cons", reviews[1].Cons);
        Assert.True(reviews[1].IWorkHere);
        Assert.Equal(user.Id, reviews[1].UserId);

        company = context.Companies
            .Include(x => x.Reviews)
            .FirstOrDefault(x => x.Id == company.Id);

        Assert.NotNull(company);

        Assert.Equal(2, company.Reviews.Count);
        Assert.Equal(0, company.Rating);
    }
}