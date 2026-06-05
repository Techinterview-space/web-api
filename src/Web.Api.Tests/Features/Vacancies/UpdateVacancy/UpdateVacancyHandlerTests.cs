using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.UpdateVacancy;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.UpdateVacancy;

public class UpdateVacancyHandlerTests
{
    [Fact]
    public async Task Handle_Owner_UpdatesFields_AddsModifiedHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public, title: "Old title").Please(context);

        var target = new UpdateVacancyHandler(context, new FakeAuth(author));

        var command = new UpdateVacancyCommand(
            vacancy.Id,
            new UpdateVacancyBodyRequest
            {
                Title = "New title",
                CompanyId = company.Id,
                HrContact = "hr@example.com",
                Description = "Updated description",
                Status = VacancyStatus.Public,
            });

        await target.Handle(command, CancellationToken.None);

        var updated = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Equal("New title", updated.Title);

        var history = context.VacancyHistoryRecords
            .Where(x => x.VacancyId == vacancy.Id)
            .Select(x => x.Message)
            .ToList();

        Assert.Contains("Was modified", history);
        Assert.DoesNotContain(history, x => x.StartsWith("Status changed"));

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Was modified");

        Assert.Equal(author.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_Owner_ChangesStatus_AddsStatusHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Draft, title: "Title", description: "Desc").Please(context);

        var target = new UpdateVacancyHandler(context, new FakeAuth(author));

        var command = new UpdateVacancyCommand(
            vacancy.Id,
            new UpdateVacancyBodyRequest
            {
                Title = "Title",
                CompanyId = company.Id,
                HrContact = null,
                Description = "Desc",
                Status = VacancyStatus.Public,
            });

        await target.Handle(command, CancellationToken.None);

        var updated = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Equal(VacancyStatus.Public, updated.Status);

        var history = context.VacancyHistoryRecords
            .Where(x => x.VacancyId == vacancy.Id)
            .Select(x => x.Message)
            .ToList();

        Assert.Contains("Status changed Draft -> Public", history);
        Assert.DoesNotContain("Was modified", history);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Status changed Draft -> Public");

        Assert.Equal(author.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_Owner_SetsFreeTextNameAndHidesCompany_AddsModifiedHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public, title: "Title", description: "Desc").Please(context);

        var target = new UpdateVacancyHandler(context, new FakeAuth(author));

        var command = new UpdateVacancyCommand(
            vacancy.Id,
            new UpdateVacancyBodyRequest
            {
                Title = "Title",
                CompanyId = company.Id,
                CompanyName = "NDA",
                HideAttachedCompany = true,
                Description = "Desc",
                Status = VacancyStatus.Public,
            });

        await target.Handle(command, CancellationToken.None);

        var updated = context.Vacancies.First(x => x.Id == vacancy.Id);
        Assert.Equal("NDA", updated.CompanyName);
        Assert.True(updated.HideAttachedCompany);
        Assert.Equal(company.Id, updated.CompanyId);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == vacancy.Id &&
                x.Message == "Was modified");

        Assert.Equal(author.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_NonOwner_ThrowsNoPermissions()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var otherUser = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);

        var target = new UpdateVacancyHandler(context, new FakeAuth(otherUser));

        var command = new UpdateVacancyCommand(
            vacancy.Id,
            new UpdateVacancyBodyRequest
            {
                Title = "Hacked",
                CompanyId = company.Id,
                Status = VacancyStatus.Public,
            });

        await Assert.ThrowsAsync<NoPermissionsException>(
            () => target.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeletedVacancy_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var author = await new UserFake(Role.Interviewer).PleaseAsync(context);
        var vacancy = new VacancyFake(company, author, VacancyStatus.Public).Please(context);
        vacancy.Delete(author);
        context.SaveChanges();

        var target = new UpdateVacancyHandler(context, new FakeAuth(author));

        var command = new UpdateVacancyCommand(
            vacancy.Id,
            new UpdateVacancyBodyRequest
            {
                Title = "New title",
                CompanyId = company.Id,
                Status = VacancyStatus.Public,
            });

        await Assert.ThrowsAsync<BadRequestException>(
            () => target.Handle(command, CancellationToken.None));
    }
}
