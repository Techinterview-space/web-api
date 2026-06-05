using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Vacancies.CreateVacancy;
using Xunit;

namespace Web.Api.Tests.Features.Vacancies.CreateVacancy;

public class CreateVacancyHandlerTests
{
    [Fact]
    public async Task Handle_ValidRequest_CreatesVacancyWithHistory()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = company.Id,
            HrContact = "hr@example.com",
            Description = "A great role",
            Status = VacancyStatus.Draft,
        };

        var result = await target.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Backend Developer", result.Title);
        Assert.Equal(VacancyStatus.Draft, result.Status);
        Assert.Equal(user.Id, result.AuthorId);

        var record = context.VacancyHistoryRecords
            .Single(x =>
                x.VacancyId == result.Id &&
                x.Message == "Vacancy was created");

        Assert.Equal(user.Email, record.CreatedBy);
    }

    [Fact]
    public async Task Handle_CompanyNotFound_ThrowsNotFound()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = Guid.NewGuid(),
            Status = VacancyStatus.Draft,
        };

        await Assert.ThrowsAsync<NotFoundException>(
            () => target.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DeletedCompanyAllowed_VacancyCreated()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        company.Delete();
        context.SaveChanges();

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = company.Id,
            Status = VacancyStatus.Draft,
        };

        var result = await target.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(company.Id, result.CompanyId);
    }

    [Fact]
    public async Task Handle_NoCompanyAndNoName_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = null,
            Status = VacancyStatus.Draft,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => target.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_NoCompanyButFreeTextName_CreatesVacancy()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = null,
            CompanyName = "NDA",
            Status = VacancyStatus.Draft,
        };

        var result = await target.Handle(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.CompanyId);
        Assert.Equal("NDA", result.CompanyName);
        Assert.Equal("NDA", result.CompanyNameText);
        Assert.Null(result.CompanySlug);
        Assert.False(result.HideAttachedCompany);
    }

    [Fact]
    public async Task Handle_HideAttachedCompanyWithFreeText_MasksCompanyInDto()
    {
        await using var context = new InMemoryDatabaseContext();
        var company = new CompanyFake().Please(context);
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = company.Id,
            CompanyName = "NDA",
            HideAttachedCompany = true,
            Status = VacancyStatus.Draft,
        };

        var result = await target.Handle(request, CancellationToken.None);

        Assert.Equal(company.Id, result.CompanyId);
        Assert.True(result.HideAttachedCompany);
        Assert.Equal("NDA", result.CompanyName);
        Assert.Equal("NDA", result.CompanyNameText);
        Assert.Null(result.CompanySlug);
        Assert.False(result.CompanyIsDeleted);
    }

    [Fact]
    public async Task Handle_HideAttachedCompanyWithoutCompany_ThrowsBadRequest()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new UserFake(Role.Interviewer).PleaseAsync(context);

        var target = new CreateVacancyHandler(context, new FakeAuth(user));

        var request = new CreateVacancyBodyRequest
        {
            Title = "Backend Developer",
            CompanyId = null,
            CompanyName = "NDA",
            HideAttachedCompany = true,
            Status = VacancyStatus.Draft,
        };

        await Assert.ThrowsAsync<BadRequestException>(
            () => target.Handle(request, CancellationToken.None));
    }
}
