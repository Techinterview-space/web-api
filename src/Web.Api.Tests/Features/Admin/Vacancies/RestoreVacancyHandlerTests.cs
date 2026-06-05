using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Admin.Vacancies.RestoreVacancy;
using Xunit;

namespace Web.Api.Tests.Features.Admin.Vacancies;

public class RestoreVacancyHandlerTests
{
    [Fact]
    public async Task Handle_RestoresDeletedVacancy_AddsHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        vacancy.Delete(author);
        context.SaveChanges();

        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var target = new RestoreVacancyHandler(context, new FakeAuth(admin));

        await target.Handle(
            new RestoreVacancyCommand(vacancy.Id),
            CancellationToken.None);

        var restored = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Null(restored.DeletedAt);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Admin has restored the vacancy");

        Assert.Equal(admin.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_NotDeletedVacancy_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var admin = await new UserFake(Role.Admin).PleaseAsync(context);
        var target = new RestoreVacancyHandler(context, new FakeAuth(admin));

        await Assert.ThrowsAsync<BadRequestException>(
            () => target.Handle(
                new RestoreVacancyCommand(vacancy.Id),
                CancellationToken.None));
    }
}
