using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entities.Interviews;
using Domain.Enums;
using Domain.ValueObjects;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Web.Api.Features.Interviews;
using Web.Api.Features.Interviews.Models;
using Web.Api.Features.Labels.Models;
using Xunit;

namespace Web.Api.Tests;

public class InterviewTemplateControllerTests
{
    [Theory]
    [InlineData(Role.Admin)]
    public void AllAsync_HasRole_True(Role role)
    {
        var controller = new InterviewTemplateController(
            new Mock<IAuthorization>().Object,
            new InMemoryDatabaseContext());

        Assert.True(controller.HasRole(nameof(InterviewTemplateController.AllAsync), role));
    }

    [Theory]
    [InlineData(Role.Interviewer)]
    public void AllAsync_HasNoRole_True(Role role)
    {
        var controller = new InterviewTemplateController(
            new Mock<IAuthorization>().Object,
            new InMemoryDatabaseContext());

        Assert.False(controller.HasRole(nameof(InterviewTemplateController.AllAsync), role));
    }

    [Fact]
    public async Task AvailableForInterviewAsync_MyTemplatesAndPublicOnesAreBeingReturned_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var otherPersonService = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);
        await otherPersonService.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = "JAVA Developer private",
            Subjects = new List<InterviewTemplateSubject>()
        });
        await otherPersonService.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = "JAVA Developer public",
            Subjects = new List<InterviewTemplateSubject>(),
            IsPublic = true
        });

        var otherPersonPublicTemplates = await context.InterviewTemplates
            .Where(x => x.AuthorId == otherPerson.Id)
            .Where(x => x.IsPublic)
            .AllAsync();

        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer private",
            Subjects = new List<InterviewTemplateSubject>()
        });
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer public",
            Subjects = new List<InterviewTemplateSubject>(),
            IsPublic = true
        });

        var forSelectBoxes = (await target.AvailableForInterviewAsync()).ToArray();
        Assert.NotEmpty(forSelectBoxes);
        Assert.Equal(3, forSelectBoxes.Length);

        Assert.Single(forSelectBoxes, x => x.AuthorId != currentUser.Id && x.IsPublic);
        Assert.Equal(2, forSelectBoxes.Count(x => x.AuthorId == currentUser.Id));
    }

    [Fact]
    public async Task MyAsync_OnlyMyAreBeingReturned_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>()
        });
        var templates = await context.InterviewTemplates.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);

        var myTemplates = await target.MyTemplatesAsync();
        Assert.Single(myTemplates);
        Assert.True(myTemplates.All(x => x.AuthorId == currentUser.Id));
    }

    [Fact]
    public async Task MyAsync_IHaveNotCreatedAnyTemplate_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>()
        });

        Assert.Single(await context.InterviewTemplates.AllAsync());

        target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);
        var myTemplates = await target.MyTemplatesAsync();
        Assert.Empty(myTemplates);
        Assert.Single(await context.InterviewTemplates.AllAsync());
    }

    [Fact]
    public async Task PublicAsync_PublicTemplatesAreBeingReturned_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            IsPublic = true,
            Subjects = new List<InterviewTemplateSubject>()
        });

        Assert.Single(await context.InterviewTemplates.AllAsync());

        target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);
        var myTemplates = await target.PublicAsync();

        Assert.Equal(1, myTemplates.TotalItems);
        Assert.Single(myTemplates.Results);
    }

    [Fact]
    public async Task PublicAsync_PrivateTemplatesAreNotBeingReturned_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            IsPublic = false,
            Subjects = new List<InterviewTemplateSubject>()
        });

        Assert.Single(await context.InterviewTemplates.AllAsync());

        target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);
        var myTemplates = await target.PublicAsync();

        Assert.Equal(0, myTemplates.TotalItems);
        Assert.Empty(myTemplates.Results);
    }

    [Fact]
    public async Task CreateAsync_ValidData_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>
            {
                new ()
                {
                    Title = "ASP.NET Core",
                    Description = "Middlewares, Caching",
                }
            },
            Labels = new List<LabelDto>
            {
                new LabelDto(".net", new HexColor("#ff0000")),
                new LabelDto("java", new HexColor("#ff0000")),
            }
        });

        var templates = await context.InterviewTemplates
            .Include(x => x.Labels)
            .AllAsync();

        Assert.Single(templates);

        var template = templates[0];
        Assert.Equal(currentUser.Id, template.AuthorId);
        Assert.Equal(".NET Developer", template.Title);
        Assert.Single(template.Subjects);
        Assert.Equal(2, template.Labels.Count);
    }

    [Fact]
    public async Task CreateAsync_EmptySubjects_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>()
        });
        var templates = await context.InterviewTemplates.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);
    }

    [Fact]
    public async Task UpdateAsync_OtherPersonTriesToUpdate_ReturnForbiddenAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>
            {
                new ()
                {
                    Title = "ASP.NET Core",
                    Description = "Middlewares, Caching",
                }
            }
        });
        var templates = await context.InterviewTemplates.AllAsync();
        Assert.Single(templates);

        var template = templates[0];

        Assert.Equal(".NET Developer", template.Title);
        Assert.Single(template.Subjects);

        target = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);

        var result = await target.UpdateAsync(new InterviewTemplateUpdateRequest
        {
            Id = template.Id,
            Title = "JAVA Developer",
            Subjects = new List<InterviewTemplateSubject>()
        });
        Assert.Equal(typeof(StatusCodeResult), result.GetType());
        Assert.Equal(StatusCodes.Status403Forbidden, ((StatusCodeResult)result).StatusCode);
    }

    [Fact]
    public async Task UpdateAsync_Labels_ExceptionAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>
            {
                new ()
                {
                    Title = "ASP.NET Core",
                    Description = "Middlewares, Caching",
                }
            },
            Labels = new List<LabelDto>
            {
                new LabelDto(".net", new HexColor("#ff0000")),
                new LabelDto("java", new HexColor("#ff0000")),
            }
        });
        var templates = await context.InterviewTemplates
            .Include(x => x.Labels)
            .AllAsync();

        Assert.Single(templates);

        var template = templates[0];

        Assert.Equal(".NET Developer", template.Title);
        Assert.Single(template.Subjects);
        Assert.Equal(2, template.Labels.Count);

        await target.UpdateAsync(new InterviewTemplateUpdateRequest
        {
            Id = template.Id,
            Title = "JAVA Developer",
            Subjects = new List<InterviewTemplateSubject>(),
            Labels = new List<LabelDto>
            {
                new LabelDto(template.Labels.First()),
                new LabelDto("qa", new HexColor("#ff0000")),
                new LabelDto("ba", new HexColor("#ff0000")),
            }
        });

        template = await context.InterviewTemplates
            .Include(x => x.Labels)
            .ByIdOrFailAsync(template.Id);

        Assert.Equal(3, template.Labels.Count);
        Assert.Contains(template.Labels, x => x.Title == ".net");
        Assert.Contains(template.Labels, x => x.Title == "qa");
        Assert.Contains(template.Labels, x => x.Title == "ba");
    }

    [Fact]
    public async Task DeleteAsync_EmptySubjects_OkAsync()
    {
        await using var context = new SqliteContext();
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>()
        });
        var templates = await context.InterviewTemplates.AllAsync();
        Assert.Single(templates);
        Assert.Empty(templates[0].Subjects);

        await target.DeleteAsync(templates[0].Id);
        Assert.False(await context.InterviewTemplates.AnyAsync());
    }

    [Fact]
    public async Task DeleteAsync_OtherPersonTriesToUpdate_ReturnsForbiddenAsync()
    {
        await using var context = new SqliteContext();
        var otherPerson = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var currentUser = await new FakeUser(Role.Interviewer).PleaseAsync(context);
        var target = new InterviewTemplateController(
            new FakeAuth(currentUser),
            context);

        Assert.False(await context.InterviewTemplates.AnyAsync());
        await target.CreateAsync(new InterviewTemplateCreateRequest
        {
            Title = ".NET Developer",
            Subjects = new List<InterviewTemplateSubject>
            {
                new ()
                {
                    Title = "ASP.NET Core",
                    Description = "Middlewares, Caching",
                }
            },
            Labels = new List<LabelDto>
            {
                new LabelDto(".net", new HexColor("#ff0000")),
                new LabelDto("java", new HexColor("#ff0000")),
            }
        });
        var templates = await context.InterviewTemplates.AllAsync();
        Assert.Single(templates);

        var template = templates[0];

        Assert.Equal(".NET Developer", template.Title);
        Assert.Single(template.Subjects);

        target = new InterviewTemplateController(
            new FakeAuth(otherPerson),
            context);
        var result = await target.DeleteAsync(template.Id);
        Assert.Equal(typeof(StatusCodeResult), result.GetType());
        Assert.Equal(StatusCodes.Status403Forbidden, ((StatusCodeResult)result).StatusCode);
    }
}