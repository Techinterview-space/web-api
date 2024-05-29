using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Enums;
using Domain.Entities.Interviews;
using Domain.Entities.Labels;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.PDF.Interviews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TechInterviewer.Features.Interviews;
using TechInterviewer.Features.Interviews.Models;
using TechInterviewer.Features.Labels.Models;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests;

public class InterviewsControllerTests
{
    [Theory]
    [InlineData(Role.Admin)]
    public void AllAsync_HasRole_True(Role role)
    {
        var controller = new InterviewsController(
            new Mock<IAuthorization>().Object,
            new InMemoryDatabaseContext(),
            new Mock<IInterviewPdfService>().Object);

        Assert.True(controller.HasRole(nameof(InterviewsController.AllAsync), role));
    }

    [Theory]
    [InlineData(Role.Interviewer)]
    public void AllAsync_HasNoRole_True(Role role)
    {
        var controller = new InterviewsController(
            new Mock<IAuthorization>().Object,
            new InMemoryDatabaseContext(),
            new Mock<IInterviewPdfService>().Object);

        Assert.False(controller.HasRole(nameof(InterviewsController.AllAsync), role));
    }

    [Fact]
    public async Task MyAsync_OnlyMyAreBeingReturned_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = null,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>()
            },
            default);

        var templates = await context.Interviews.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);

        var myTemplates = await target.MyInterviewsAsync();
        Assert.Single(myTemplates);
        Assert.True(myTemplates.All(x => x.InterviewerId == currentUser.Id));
    }

    [Fact]
    public async Task MyAsync_IHaveNotCreatedAnyTemplate_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(otherPerson),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = null,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>()
            },
            default);

        Assert.Single(await context.Interviews.AllAsync());

        target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);
        var myTemplates = await target.MyInterviewsAsync();
        Assert.Empty(myTemplates);
        Assert.Single(await context.Interviews.AllAsync());
    }

    [Fact]
    public async Task CreateAsync_ValidData_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.Labels)
            .AllAsync();

        Assert.Single(templates);

        var template = templates[0];
        Assert.Equal(currentUser.Id, template.InterviewerId);
        Assert.Equal("Maxim Gorbatyuk", template.CandidateName);
        Assert.Equal("Good at all", template.OverallOpinion);
        Assert.Equal(DeveloperGrade.Middle, template.CandidateGrade);
        Assert.Single(template.Subjects);
        Assert.Equal(2, template.Labels.Count);

        foreach (var label in template.Labels)
        {
            Assert.True(label.Id != default);
            Assert.Equal(currentUser.Id, label.CreatedById);
            Assert.Equal("#ff0000", label.HexColor.ToString());
        }
    }

    [Fact]
    public async Task CreateAsync_EmptySubjects_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>()
            },
            default);

        var templates = await context.Interviews.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);
        Assert.Empty(templates[0].Labels);
    }

    [Fact]
    public async Task UpdateAsync_OtherPersonTriesToUpdate_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews.AllAsync();
        Assert.Single(templates);

        var template = templates[0];

        target = new InterviewsController(
            new FakeAuth(otherPerson),
            context,
            new Mock<IInterviewPdfService>().Object);
        await Assert.ThrowsAsync<NoPermissionsException>(() => target.UpdateAsync(
            new InterviewUpdateRequest
            {
                Id = template.Id,
                CandidateName = "Maxim Mitkin",
                CandidateGrade = DeveloperGrade.Lead,
                OverallOpinion = "Bad at all",
                Subjects = new List<InterviewSubject>()
            },
            default));
    }

    [Fact]
    public async Task UpdateAsync_Mix_OkAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.Labels)
            .AllAsync();

        Assert.Single(templates);

        var template = templates[0];

        context.ChangeTracker.Clear();
        await target.UpdateAsync(
            new InterviewUpdateRequest
            {
                Id = template.Id,
                CandidateName = "Maxim Mitkin",
                CandidateGrade = DeveloperGrade.Lead,
                OverallOpinion = "Bad at all",
                Subjects = new List<InterviewSubject>(),
                Labels = new List<LabelDto>
                {
                    new LabelDto(template.Labels.First()),
                    new LabelDto("qa", new HexColor("#ff0000")),
                    new LabelDto("ba", new HexColor("#ff0000")),
                    new LabelDto("react", new HexColor("#ff0000")),
                }
            },
            default);

        templates = await context.Interviews
            .Include(x => x.Labels)
            .AllAsync();

        Assert.Equal(4, templates[0].Labels.Count);

        Assert.Contains(templates[0].Labels, x => x.Title == ".net");
        Assert.Contains(templates[0].Labels, x => x.Title == "qa");
        Assert.Contains(templates[0].Labels, x => x.Title == "ba");
        Assert.Contains(templates[0].Labels, x => x.Title == "react");

        Assert.Equal(5, await context.Set<UserLabel>().CountAsync());
    }

    [Fact]
    public async Task DeleteAsync_EmptySubjects_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>()
            },
            default);

        var templates = await context.Interviews.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);

        await target.DeleteAsync(templates[0].Id, default);
        Assert.False(await context.Interviews.AnyAsync());
    }

    [Fact]
    public async Task DeleteAsync_OtherPersonTriesToUpdate_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                }
            },
            default);

        var templates = await context.Interviews.AllAsync();
        Assert.Single(templates);

        var template = templates[0];

        target = new InterviewsController(
            new FakeAuth(otherPerson),
            context,
            new Mock<IInterviewPdfService>().Object);
        await Assert.ThrowsAsync<NoPermissionsException>(() => target.DeleteAsync(template.Id, default));
    }

    [Fact]
    public async Task ShareWithLink_InterviewWithShareToken_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        var template = templates[0];

        await context.ShareLinks.AddAsync(new ShareLink(template));
        await context.TrySaveChangesAsync();

        var shareLinks = await context.ShareLinks
            .AllAsync();
        Assert.Single(shareLinks);

        await target.GetShareToken(template.Id, default);

        templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();

        Assert.NotNull(templates[0].ShareLink);
        Assert.Equal(templates[0].Id, templates[0].ShareLink.InterviewId);
        Assert.Equal(shareLinks[0].ShareToken, templates[0].ShareLink.ShareToken);
    }

    [Fact]
    public async Task ShareWithLink_InterviewWithoutShareToken_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        var template = templates[0];

        await target.GetShareToken(template.Id, default);

        templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();

        Assert.NotNull(templates[0].ShareLink);
        Assert.Equal(templates[0].Id, templates[0].ShareLink.InterviewId);
        Assert.NotNull(templates[0].ShareLink.ShareToken);
    }

    [Fact]
    public async Task GetInterviewByShareToken_ValidShareToken_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var interview = await context.Interviews
            .Include(x => x.ShareLink)
            .FirstOrDefaultAsync();

        Assert.NotNull(interview);
        await context.ShareLinks.AddAsync(new ShareLink(interview));
        await context.TrySaveChangesAsync();

        var shareLinks = await context.ShareLinks.AllAsync();
        Assert.Single(shareLinks);

        var result = await target.GetInterviewByShareToken(
                interview.Id,
                shareLinks[0].ShareToken.GetValueOrDefault(),
                default) as OkObjectResult;

        Assert.NotNull(result);
        var testInterview = result.Value as InterviewDto;

        Assert.NotNull(testInterview);
        Assert.Equal(currentUser.Id, testInterview.InterviewerId);
        Assert.Equal("Maxim Gorbatyuk", testInterview.CandidateName);
        Assert.Equal("Good at all", testInterview.OverallOpinion);
        Assert.Equal(DeveloperGrade.Middle, testInterview.CandidateGrade);
        Assert.Single(testInterview.Subjects);

        Assert.Equal(2, testInterview.Labels.Count);

        foreach (var label in testInterview.Labels)
        {
            Assert.True(label.Id != default);
            Assert.Equal(currentUser.Id, label.CreatedById);
            Assert.Equal("#ff0000", label.HexColor.ToString());
        }
    }

    [Fact]
    public async Task GetInterviewByShareToken_NotValidShareToken_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var interview = await context.Interviews.FirstOrDefaultAsync();
        Assert.NotNull(interview);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            target.GetInterviewByShareToken(
                interview.Id,
                Guid.NewGuid(),
                default));
    }

    [Fact]
    public async Task RevokeShareLink_InterviewWithShareToken_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        var template = templates[0];

        await context.ShareLinks.AddAsync(new ShareLink(template));
        await context.TrySaveChangesAsync();

        var shareLinks = await context.ShareLinks
            .AllAsync();
        Assert.Single(shareLinks);

        await target.RevokeShareLink(template.Id, default);

        templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        template = templates[0];

        Assert.NotNull(template.ShareLink);
        Assert.Equal(template.Id, template.ShareLink.InterviewId);
        Assert.NotNull(template.ShareLink.ShareToken);
        Assert.NotEqual(shareLinks[0].ShareToken, template.ShareLink.ShareToken);
    }

    [Fact]
    public async Task RevokeShareLink_InterviewWithOutShareToken_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewsController(
            new FakeAuth(currentUser),
            context,
            new Mock<IInterviewPdfService>().Object);

        Assert.False(await context.Interviews.AnyAsync());
        await target.Create(
            new InterviewCreateRequest
            {
                CandidateName = "Maxim Gorbatyuk",
                CandidateGrade = DeveloperGrade.Middle,
                OverallOpinion = "Good at all",
                Subjects = new List<InterviewSubject>
                {
                    new ()
                    {
                        Title = "ASP.NET Core",
                        Grade = DeveloperGrade.Middle,
                        Comments = "Middlewares, Caching"
                    }
                },
                Labels = new List<LabelDto>
                {
                    new LabelDto(".net", new HexColor("#ff0000")),
                    new LabelDto("java", new HexColor("#ff0000")),
                }
            },
            default);

        var templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        var template = templates[0];

        await target.RevokeShareLink(template.Id, default);

        templates = await context.Interviews
            .Include(x => x.ShareLink)
            .AllAsync();
        template = templates[0];

        Assert.NotNull(template.ShareLink);
        Assert.Equal(template.Id, template.ShareLink.InterviewId);
        Assert.NotNull(template.ShareLink.ShareToken);
    }
}

