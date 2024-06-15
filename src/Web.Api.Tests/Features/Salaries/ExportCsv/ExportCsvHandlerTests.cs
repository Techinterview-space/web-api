using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Salaries.ExportCsv;
using Xunit;

namespace Web.Api.Tests.Features.Salaries.ExportCsv;

public class ExportCsvHandlerTests
{
    [Fact]
    public async Task Handle_HasSalaries_UserDidntDownloadFile_Ok()
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        Assert.Equal(1, context.Salaries.Count());

        context.ChangeTracker.Clear();

        Assert.Equal(0, context.UserCsvDownloads.Count());
        var result = await new ExportCsvHandler(
                new FakeAuth(user),
                context,
                new SalaryLabelsProviderFake(context))
            .Handle(new ExportCsvQuery(), default);

        Assert.Equal(1, context.Salaries.Count());
        Assert.Equal(1, context.UserCsvDownloads.Count());

        Assert.Contains(salary1.Value.ToString(CultureInfo.InvariantCulture), result.CsvContent);

        var userDownload = context.UserCsvDownloads.First();
        Assert.Equal(user.Id, userDownload.UserId);
    }

    [Fact]
    public async Task Handle_HasSalaries_OldDownloadRecord_Ok()
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var oldDownload = await new UserCsvDownloadFake(user, DateTime.UtcNow.AddDays(-2)).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        context.ChangeTracker.Clear();

        Assert.Equal(1, context.Salaries.Count());
        Assert.Equal(1, context.UserCsvDownloads.Count());
        var result = await new ExportCsvHandler(
                new FakeAuth(user),
                context,
                new SalaryLabelsProviderFake(context))
            .Handle(new ExportCsvQuery(), default);

        Assert.Equal(1, context.Salaries.Count());
        Assert.Equal(1, context.UserCsvDownloads.Count());

        Assert.Contains(salary1.Value.ToString(CultureInfo.InvariantCulture), result.CsvContent);

        var userDownload = context.UserCsvDownloads.First();
        Assert.Equal(user.Id, userDownload.UserId);
        Assert.NotEqual(oldDownload.Id, userDownload.Id);
    }

    [Fact]
    public async Task Handle_HasSalaries_RecentDownloadRecord_Fail()
    {
        await using var context = new InMemoryDatabaseContext();

        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var oldDownload = await new UserCsvDownloadFake(user, DateTime.UtcNow.AddHours(-2)).PleaseAsync(context);

        var salary1 = await context.SaveAsync(new UserSalaryFake(
                user,
                value: 400_000,
                createdAt: DateTimeOffset.Now.AddDays(-1))
            .AsDomain());

        context.ChangeTracker.Clear();

        Assert.Equal(1, context.Salaries.Count());
        Assert.Equal(1, context.UserCsvDownloads.Count());
        await Assert.ThrowsAsync<NoPermissionsException>(() =>
            new ExportCsvHandler(
                    new FakeAuth(user),
                    context,
                    new SalaryLabelsProviderFake(context))
                .Handle(new ExportCsvQuery(), default));

        Assert.Equal(1, context.Salaries.Count());
        Assert.Equal(1, context.UserCsvDownloads.Count());

        var userDownload = context.UserCsvDownloads.First();
        Assert.Equal(user.Id, userDownload.UserId);
        Assert.Equal(oldDownload.Id, userDownload.Id);
    }
}