using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.GetVacancy;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.GetVacancy;

public class GetVacancyHandlerTests
{
    [Fact]
    public async Task Handle_PublicVacancy_AnonymousCanView()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new GetVacancyHandler(context, new FakeAuth(null));

        var result = await target.Handle(vacancy.Id, CancellationToken.None);

        Assert.Equal(vacancy.Id, result.Id);
    }

    [Fact]
    public async Task Handle_DraftVacancy_Anonymous_ThrowsNotFound()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);

        var target = new GetVacancyHandler(context, new FakeAuth(null));

        await Assert.ThrowsAsync<NotFoundException>(
            () => target.Handle(vacancy.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DraftVacancy_Owner_CanView()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);

        var target = new GetVacancyHandler(context, new FakeAuth(author));

        var result = await target.Handle(vacancy.Id, CancellationToken.None);

        Assert.Equal(vacancy.Id, result.Id);
    }

    [Fact]
    public async Task Handle_DraftVacancy_OtherUser_ThrowsNotFound()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);

        var target = new GetVacancyHandler(context, new FakeAuth(otherUser));

        await Assert.ThrowsAsync<NotFoundException>(
            () => target.Handle(vacancy.Id, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeletedVacancy_Admin_CanView()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        vacancy.Delete(author);
        context.SaveChanges();

        var target = new GetVacancyHandler(context, new FakeAuth(admin));

        var result = await target.Handle(vacancy.Id, CancellationToken.None);

        Assert.Equal(vacancy.Id, result.Id);
        Assert.NotNull(result.DeletedAt);
    }

    [Fact]
    public async Task Handle_NonExistent_ThrowsNotFound()
    {
        await using var context = new InMemoryDatabaseContext();

        var target = new GetVacancyHandler(context, new FakeAuth(null));

        await Assert.ThrowsAsync<NotFoundException>(
            () => target.Handle(Guid.NewGuid(), CancellationToken.None));
    }
}
