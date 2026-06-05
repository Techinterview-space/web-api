using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Admin.Vacancies.ChangeVacancyStatus;
using Xunit;

namespace Web.Api.Tests.Features.Admin.Vacancies;

public class ChangeVacancyStatusHandlerTests
{
    [Fact]
    public async Task Handle_MoveToClosed_AddsHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var target = new ChangeVacancyStatusHandler(context, new FakeAuth(admin));

        await target.Handle(
            new ChangeVacancyStatusCommand(vacancy.Id, VacancyStatus.Closed),
            CancellationToken.None);

        var updated = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Equal(VacancyStatus.Closed, updated.Status);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Admin has moved the vacancy to status Closed");

        Assert.Equal(admin.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_MoveToDraft_AddsHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var target = new ChangeVacancyStatusHandler(context, new FakeAuth(admin));

        await target.Handle(
            new ChangeVacancyStatusCommand(vacancy.Id, VacancyStatus.Draft),
            CancellationToken.None);

        var updated = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Equal(VacancyStatus.Draft, updated.Status);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Admin has moved the vacancy to status Draft");

        Assert.Equal(admin.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_MoveToPublic_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Draft).Please(context);

        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var target = new ChangeVacancyStatusHandler(context, new FakeAuth(admin));

        await Assert.ThrowsAsync<BadRequestException>(
            () => target.Handle(
                new ChangeVacancyStatusCommand(vacancy.Id, VacancyStatus.Public),
                CancellationToken.None));
    }
}
