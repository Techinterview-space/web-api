using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.DeleteVacancy;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.DeleteVacancy;

public class DeleteVacancyHandlerTests
{
    [Fact]
    public async Task Handle_Owner_SoftDeletes_AddsHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new DeleteVacancyHandler(context, new FakeAuth(author));

        await target.Handle(new DeleteVacancyCommand(vacancy.Id), CancellationToken.None);

        var deleted = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.NotNull(deleted.DeletedAt);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Was deleted");

        Assert.Equal(author.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_Admin_CanDelete()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new DeleteVacancyHandler(context, new FakeAuth(admin));

        await target.Handle(new DeleteVacancyCommand(vacancy.Id), CancellationToken.None);

        var deleted = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.NotNull(deleted.DeletedAt);
    }

    [Fact]
    public async Task Handle_OtherUser_ThrowsNoPermissions()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new DeleteVacancyHandler(context, new FakeAuth(otherUser));

        await Assert.ThrowsAsync<NoPermissionsException>(
            () => target.Handle(new DeleteVacancyCommand(vacancy.Id), CancellationToken.None));
    }
}
