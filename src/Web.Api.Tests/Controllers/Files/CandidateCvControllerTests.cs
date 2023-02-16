using System;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Aws.S3.Simple.Models;
using Domain.Entities.Employments;
using Domain.Enums;
using Domain.Services.Organizations;
using MG.Utils.EFCore;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TechInterviewer.Controllers.Files;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using TestUtils.Mocks;
using Xunit;

namespace Web.Api.Tests.Controllers.Files;

public class CandidateCvControllerTests
{
    [Fact]
    public async Task UploadAsync_ValidData_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview).PleaseAsync(context);

        var uniqueStorageName = Guid.NewGuid() + "/cv.pdf";
        var storage = new CvStorageMock(
            request =>
            {
                Assert.Equal("cv.pdf", request.FileName);
                return FileUploadResult.Success(uniqueStorageName);
            });

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.UploadAsync(card.Id, new FileUploadRequest
        {
            File = new FileAttachmentMock("cv.pdf").Object
        });

        Assert.Equal(typeof(OkObjectResult), result.GetType());

        var resultCard = ((OkObjectResult)result).Value as CandidateCardDto;
        Assert.NotNull(resultCard);
        Assert.Single(resultCard.Files);

        Assert.Equal(uniqueStorageName, resultCard.Files.First().StorageFileName);
        Assert.Equal("cv.pdf", resultCard.Files.First().FileName);
        storage.VerifyUpload(Times.Once());
    }

    [Fact]
    public async Task UploadAsync_NotMyOrg_ForbiddenAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization2).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview).PleaseAsync(context);

        var uniqueStorageName = Guid.NewGuid() + "/cv.pdf";
        var storage = new CvStorageMock(
            request =>
            {
                Assert.Equal("cv.pdf", request.FileName);
                return FileUploadResult.Success(uniqueStorageName);
            });

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.UploadAsync(card.Id, new FileUploadRequest
        {
            File = new FileAttachmentMock("cv.pdf").Object
        });

        Assert.Equal(typeof(ForbidResult), result.GetType());

        card = await context.CandidateCards.ByIdOrFailAsync(card.Id);
        Assert.Empty(card.Files);
        storage.VerifyUpload(Times.Never());
    }

    [Fact]
    public async Task UploadAsync_ErrorFromStorage_BadRequestAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview).PleaseAsync(context);
        var storage = new CvStorageMock(
            request =>
            {
                Assert.Equal("cv.pdf", request.FileName);
                return FileUploadResult.Failure("no bucket");
            });

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.UploadAsync(card.Id, new FileUploadRequest
        {
            File = new FileAttachmentMock("cv.pdf").Object
        });

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

        card = await context.CandidateCards.ByIdOrFailAsync(card.Id);
        Assert.Empty(card.Files);
        storage.VerifyUpload(Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_ValidData_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview)
            .WithFile("cv.pdf")
            .PleaseAsync(context);

        var uniqueStorageName = card.Files[0].StorageFileName;
        var fileId = card.Files[0].Id;
        var storage = new CvStorageMock(
            deleteReturnFunc: (request) =>
            {
                Assert.Equal(uniqueStorageName, request);
                return true;
            });

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.DeleteAsync(card.Id, fileId);

        Assert.Equal(typeof(OkObjectResult), result.GetType());

        card = await context.CandidateCards.ByIdOrFailAsync(card.Id);
        Assert.Empty(card.Files);

        storage.VerifyDelete(Times.Once());
    }

    [Fact]
    public async Task DeleteAsync_NotMyOrg_ForbiddenAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization1 = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var organization2 = await new FakeOrganization()
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization2).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview)
            .WithFile("cv.pdf")
            .PleaseAsync(context);

        var uniqueStorageName = card.Files[0].StorageFileName;
        var fileId = card.Files[0].Id;
        var storage = new CvStorageMock(
            deleteReturnFunc: (request) =>
            {
                Assert.Equal(uniqueStorageName, request);
                return true;
            });

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.DeleteAsync(card.Id, fileId);

        Assert.Equal(typeof(ForbidResult), result.GetType());

        card = await context.CandidateCards.ByIdOrFailAsync(card.Id);
        Assert.Single(card.Files);
        storage.VerifyDelete(Times.Never());
    }

    [Fact]
    public async Task DeleteAsync_NoFileInTheCard_BadRequestAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate = await new FakeCandidate(organization).PleaseAsync(context);

        var card = await new FakeCandidateCard(candidate, user, EmploymentStatus.HrInterview)
            .PleaseAsync(context);

        var storage = new CvStorageMock(
            deleteReturnFunc: (request) => true);

        var target = new CandidateCvController(storage.Object, context, new FakeAuth(user));
        var result = await target.DeleteAsync(card.Id, Guid.NewGuid());

        Assert.Equal(typeof(BadRequestObjectResult), result.GetType());

        card = await context.CandidateCards.ByIdOrFailAsync(card.Id);
        Assert.Empty(card.Files);
        storage.VerifyDelete(Times.Never());
    }
}