using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.SearchVacancies;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.SearchVacancies;

public class SearchVacanciesHandlerTests
{
    [Fact]
    public async Task Handle_OnlyPublicNonDeleted_AreReturned()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var publicVacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        new VacancyFake(company, author, VacancyStatus.Draft).Please(context);
        new VacancyFake(company, author, VacancyStatus.Closed).Please(context);

        var deleted = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        deleted.Delete(author);
        context.SaveChanges();

        var target = new SearchVacanciesHandler(context);

        var result = await target.Handle(
            new SearchVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal(1, result.TotalItems);
        Assert.Single(result.Results);
        Assert.Equal(publicVacancy.Id, result.Results.First().Id);
    }

    [Fact]
    public async Task Handle_Search_MatchesTitleAndDescription()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var byTitle = new VacancyFake(
            company,
            author,
            VacancyStatus.Public,
            title: "Senior Backend Engineer",
            description: "Java shop").Please(context);

        var byDescription = new VacancyFake(
            company,
            author,
            VacancyStatus.Public,
            title: "Frontend role",
            description: "We use React and dotnet backend").Please(context);

        var target = new SearchVacanciesHandler(context);

        var titleMatch = await target.Handle(
            new SearchVacanciesQueryParams { Page = 1, PageSize = 20, SearchQuery = "engineer" },
            CancellationToken.None);

        Assert.Single(titleMatch.Results);
        Assert.Equal(byTitle.Id, titleMatch.Results.First().Id);

        var descriptionMatch = await target.Handle(
            new SearchVacanciesQueryParams { Page = 1, PageSize = 20, SearchQuery = "react" },
            CancellationToken.None);

        Assert.Single(descriptionMatch.Results);
        Assert.Equal(byDescription.Id, descriptionMatch.Results.First().Id);
    }

    [Fact]
    public async Task Handle_OrdersByCreatedAtDescending()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var older = new VacancyFake(company, author, VacancyStatus.Public)
            .SetCreatedAt(DateTimeOffset.UtcNow.AddDays(-5))
            .Please(context);
        var newer = new VacancyFake(company, author, VacancyStatus.Public)
            .SetCreatedAt(DateTimeOffset.UtcNow.AddDays(-1))
            .Please(context);

        var target = new SearchVacanciesHandler(context);

        var result = await target.Handle(
            new SearchVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal(2, result.Results.Count);
        Assert.Equal(newer.Id, result.Results.First().Id);
        Assert.Equal(older.Id, result.Results.Last().Id);
    }

    [Fact]
    public async Task Handle_DeletedCompany_VacancyStillListed_WithoutSlug()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        company.Delete();
        context.SaveChanges();

        var target = new SearchVacanciesHandler(context);

        var result = await target.Handle(
            new SearchVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        var dto = Assert.Single(result.Results);
        Assert.Equal(vacancy.Id, dto.Id);
        Assert.True(dto.CompanyIsDeleted);
        Assert.Null(dto.CompanySlug);
        Assert.Equal(company.Name, dto.CompanyName);
    }

    [Fact]
    public async Task Handle_PageBelowOne_ClampedToFirstPage()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new SearchVacanciesHandler(context);

        var result = await target.Handle(
            new SearchVacanciesQueryParams { Page = 0, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal(1, result.CurrentPage);
        Assert.Single(result.Results);
        Assert.Equal(vacancy.Id, result.Results.First().Id);
    }
}
