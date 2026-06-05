using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.SearchMyVacancies;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.SearchMyVacancies;

public class SearchMyVacanciesHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsOnlyCurrentUsersVacancies()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var otherAuthor = await new UserFake(Role.Interviewer).PleaseAsync(context);

        new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        new VacancyFake(company, author, VacancyStatus.Draft).Please(context);
        new VacancyFake(company, otherAuthor, VacancyStatus.Public).Please(context);

        var target = new SearchMyVacanciesHandler(context, new FakeAuth(author));

        var result = await target.Handle(
            new SearchMyVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
        Assert.All(result.Results, x => Assert.Equal(author.Id, x.AuthorId));
        Assert.True(result.HasAnyVacancy);
    }

    [Fact]
    public async Task Handle_IncludesDeletedByDefault()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        var deleted = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        deleted.Delete(author);
        context.SaveChanges();

        var target = new SearchMyVacanciesHandler(context, new FakeAuth(author));

        var result = await target.Handle(
            new SearchMyVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal(2, result.TotalItems);
    }

    [Fact]
    public async Task Handle_DeletedFilter_ReturnsOnlyDeleted()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        var deleted = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        deleted.Delete(author);
        context.SaveChanges();

        var target = new SearchMyVacanciesHandler(context, new FakeAuth(author));

        var result = await target.Handle(
            new SearchMyVacanciesQueryParams { Page = 1, PageSize = 20, Deleted = true },
            CancellationToken.None);

        var dto = Assert.Single(result.Results);
        Assert.Equal(deleted.Id, dto.Id);
    }

    [Fact]
    public async Task Handle_StatusFilter_ExcludesDeleted()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var draft = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);
        var deletedDraft = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);
        deletedDraft.Delete(author);
        context.SaveChanges();

        var target = new SearchMyVacanciesHandler(context, new FakeAuth(author));

        var result = await target.Handle(
            new SearchMyVacanciesQueryParams { Page = 1, PageSize = 20, Status = VacancyStatus.Draft },
            CancellationToken.None);

        var dto = Assert.Single(result.Results);
        Assert.Equal(draft.Id, dto.Id);
    }

    [Fact]
    public async Task Handle_NoVacancies_HasAnyVacancyFalse()
    {
        await using var context = new InMemoryDatabaseContext();
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new SearchMyVacanciesHandler(context, new FakeAuth(author));

        var result = await target.Handle(
            new SearchMyVacanciesQueryParams { Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Empty(result.Results);
        Assert.False(result.HasAnyVacancy);
    }
}
