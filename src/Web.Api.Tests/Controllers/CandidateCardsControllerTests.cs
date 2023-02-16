using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Database;
using Domain.Entities.Employments;
using Domain.Enums;
using Domain.Services.Labels;
using Domain.Services.Organizations;
using Domain.Services.Organizations.Requests;
using Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Controllers.Organizations;
using TestUtils.Auth;
using TestUtils.Db;
using TestUtils.Fakes;
using Xunit;

namespace Web.Api.Tests.Controllers;

public class CandidateCardsControllerTests
{
    [Fact]
    public async Task ForOrganizationAsync_OnlyNecessaryCards_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate2 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate3 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate4 = await new FakeCandidate(organization).PleaseAsync(context);

        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview).PleaseAsync(context);
        var card2 = await new FakeCandidateCard(candidate2, user, EmploymentStatus.TechnicalInterview).PleaseAsync(context);
        var card3 = await new FakeCandidateCard(candidate3, user, EmploymentStatus.PreOffered).PleaseAsync(context);
        var card4 = await new FakeCandidateCard(candidate4, user, EmploymentStatus.Hired).PleaseAsync(context);
        var card5 = await new FakeCandidateCard(candidate4, user, EmploymentStatus.TechnicalInterview).Archived().PleaseAsync(context);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var cardsResult = await target.ForOrganizationAsync(organization.Id, new CandidateCardsFilterRequest
        {
            Statuses = $"{(int)EmploymentStatus.HrInterview},{(int)EmploymentStatus.TechnicalInterview}",
        });

        Assert.Equal(typeof(OkObjectResult), cardsResult.GetType());

        var cards = (((OkObjectResult)cardsResult).Value as IEnumerable<CandidateCardDto>)?.ToArray();
        Assert.NotNull(cards);
        Assert.Equal(2, cards.Length);
        Assert.Contains(cards, x => x.Id == card1.Id);
        Assert.Contains(cards, x => x.Id == card2.Id);

        Assert.DoesNotContain(cards, x => x.Id == card3.Id);
        Assert.DoesNotContain(cards, x => x.Id == card4.Id);
        Assert.DoesNotContain(cards, x => x.Id == card5.Id);
    }

    [Fact]
    public async Task AvailableCandidatesAsync_OnlyNecessary_OkAsync()
    {
        await using var context = new InMemoryDatabaseContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate2 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate3 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate4 = await new FakeCandidate(organization).PleaseAsync(context);
        var candidate5 = await new FakeCandidate(organization).PleaseAsync(context);

        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview).PleaseAsync(context);
        var card2 = await new FakeCandidateCard(candidate2, user, EmploymentStatus.TechnicalInterview).PleaseAsync(context);
        var card3 = await new FakeCandidateCard(candidate3, user, EmploymentStatus.PreOffered).PleaseAsync(context);
        var card4 = await new FakeCandidateCard(candidate4, user, EmploymentStatus.Hired).PleaseAsync(context);
        var card5 = await new FakeCandidateCard(candidate5, user, EmploymentStatus.TechnicalInterview).Archived().PleaseAsync(context);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var cardsResult = await target.AvailableCandidatesAsync(organization.Id);

        Assert.Equal(typeof(OkObjectResult), cardsResult.GetType());

        var candidates = (((OkObjectResult)cardsResult).Value as IEnumerable<CandidateDto>)?.ToArray();
        Assert.NotNull(candidates);
        Assert.Equal(2, candidates.Length);
        Assert.Contains(candidates, x => x.Id == candidate4.Id);
        Assert.Contains(candidates, x => x.Id == candidate5.Id);

        Assert.DoesNotContain(candidates, x => x.Id == candidate1.Id);
        Assert.DoesNotContain(candidates, x => x.Id == candidate2.Id);
        Assert.DoesNotContain(candidates, x => x.Id == candidate3.Id);
    }

    [Fact]
    public async Task CreateAsync_ExistingCandidate_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var createResult = await target.CreateAsync(new EditCandidateCardRequest
        {
            OrganizationId = organization.Id,
            CandidateId = candidate1.Id,
            EmploymentStatus = EmploymentStatus.HrInterview
        });

        Assert.Equal(typeof(OkObjectResult), createResult.GetType());

        var card = ((OkObjectResult)createResult).Value as CandidateCardDto;
        Assert.NotNull(card);

        Assert.Equal(candidate1.Id, card.CandidateId);
        Assert.NotNull(card.Candidate);

        Assert.Equal(organization.Id, card.OrganizationId);
        Assert.NotNull(card.Organization);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.Equal(user.Id, card.OpenById);

        Assert.NotNull(card.Comments);
        Assert.NotNull(card.Interviews);
    }

    [Fact]
    public async Task CreateAsync_NewCandidate_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var target = new CandidateCardsController(context, new FakeAuth(user));

        var request = new EditCandidateCardRequest
        {
            OrganizationId = organization.Id,
            EmploymentStatus = EmploymentStatus.HrInterview,
            CandidateFirstName = Faker.Name.First(),
            CandidateLastName = Faker.Name.Last(),
            CandidateContacts = Faker.Phone.Number(),
            Labels = new List<LabelDto>
            {
                new LabelDto("First", HexColor.Random())
            }
        };

        var createResult = await target.CreateAsync(request);

        Assert.Equal(typeof(OkObjectResult), createResult.GetType());

        var card = ((OkObjectResult)createResult).Value as CandidateCardDto;
        Assert.NotNull(card);

        Assert.NotNull(card.Candidate);
        Assert.Equal(request.CandidateFirstName, card.Candidate.FirstName);
        Assert.Equal(request.CandidateLastName, card.Candidate.LastName);
        Assert.Equal(request.CandidateContacts, card.Candidate.Contacts);

        Assert.Equal(organization.Id, card.Candidate.OrganizationId);
        Assert.Equal(user.Id, card.Candidate.CreatedById);
        Assert.True(card.Candidate.Active);

        Assert.Equal(organization.Id, card.OrganizationId);
        Assert.NotNull(card.Organization);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.Equal(user.Id, card.OpenById);

        Assert.NotNull(card.Comments);
        Assert.NotNull(card.Interviews);
        Assert.Single(card.Labels);
    }

    [Fact]
    public async Task UpdateAsync_StatusChanged_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview).PleaseAsync(context);

        Assert.Empty(card1.Labels);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.UpdateAsync(card1.Id, new EditCandidateCardRequest
        {
            OrganizationId = organization.Id,
            EmploymentStatus = EmploymentStatus.TechnicalInterview,
            CandidateFirstName = candidate1.FirstName + "1",
            CandidateLastName = candidate1.LastName + "1",
            CandidateContacts = candidate1.Contacts,
            Labels = new List<LabelDto>
            {
                new LabelDto("First", HexColor.Random())
            }
        });

        Assert.Equal(typeof(OkObjectResult), updateResult.GetType());

        var card = ((OkObjectResult)updateResult).Value as CandidateCardDto;
        Assert.NotNull(card);

        Assert.Equal(EmploymentStatus.TechnicalInterview, card.EmploymentStatus);
        Assert.Single(card1.Labels);
    }

    [Fact]
    public async Task ArchiveAsync_WasActive_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview).PleaseAsync(context);
        Assert.True(card1.Active);
        Assert.Null(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.ArchiveAsync(card1.Id);

        Assert.Equal(typeof(OkResult), updateResult.GetType());

        var card = await context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(card1.Id);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.False(card.Active);
        Assert.NotNull(card.DeletedAt);
    }

    [Fact]
    public async Task ArchiveAsync_WasInactive_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .Archived()
            .PleaseAsync(context);

        Assert.False(card1.Active);
        Assert.NotNull(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.ArchiveAsync(card1.Id);

        Assert.Equal(typeof(BadRequestObjectResult), updateResult.GetType());

        var card = await context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(card1.Id);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.False(card.Active);
        Assert.NotNull(card.DeletedAt);
    }

    [Fact]
    public async Task RestoreAsync_WasInactive_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .Archived()
            .PleaseAsync(context);
        Assert.False(card1.Active);
        Assert.NotNull(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.RestoreAsync(card1.Id);

        Assert.Equal(typeof(OkResult), updateResult.GetType());

        var card = await context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(card1.Id);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.True(card.Active);
        Assert.Null(card.DeletedAt);
    }

    [Fact]
    public async Task RestoreAsync_WasActive_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .PleaseAsync(context);

        Assert.True(card1.Active);
        Assert.Null(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.RestoreAsync(card1.Id);

        Assert.Equal(typeof(BadRequestObjectResult), updateResult.GetType());

        var card = await context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(card1.Id);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.True(card.Active);
        Assert.Null(card.DeletedAt);
    }

    [Fact]
    public async Task RemoveAsync_WasActive_BadResultAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview).PleaseAsync(context);
        Assert.True(card1.Active);
        Assert.Null(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.RemoveAsync(card1.Id);

        Assert.Equal(typeof(BadRequestObjectResult), updateResult.GetType());

        var card = await context.CandidateCards
            .Include(x => x.Candidate)
            .Include(x => x.Comments)
            .Include(x => x.Organization)
            .ByIdOrFailAsync(card1.Id);

        Assert.Equal(EmploymentStatus.HrInterview, card.EmploymentStatus);
        Assert.True(card.Active);
        Assert.Null(card.DeletedAt);
    }

    [Fact]
    public async Task RemoveAsync_WasInactive_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .Archived()
            .PleaseAsync(context);

        Assert.False(card1.Active);
        Assert.NotNull(card1.DeletedAt);

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.RemoveAsync(card1.Id);

        Assert.Equal(typeof(OkResult), updateResult.GetType());
        Assert.False(await context.CandidateCards.AnyAsync());
        Assert.False(await context.CandidateInterviews.AnyAsync());
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());
    }

    [Fact]
    public async Task AddCommentAsync_Active_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .PleaseAsync(context);

        Assert.True(card1.Active);
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.AddCommentAsync(card1.Id, new AddCommentRequest
        {
            Comment = "Nice!",
        });

        Assert.Equal(typeof(OkResult), updateResult.GetType());
        Assert.True(await context.Set<CandidateCardComment>().AnyAsync());

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);

        Assert.Single(card1.Comments);
        Assert.Equal("Nice!", card1.Comments.First().Comment);
        Assert.Equal(user.Id, card1.Comments.First().AuthorId);
    }

    [Fact]
    public async Task AddCommentAsync_Inactive_BadRequestAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .Archived()
            .PleaseAsync(context);

        Assert.False(card1.Active);
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.AddCommentAsync(card1.Id, new AddCommentRequest
        {
            Comment = "Nice!",
        });

        Assert.Equal(typeof(BadRequestObjectResult), updateResult.GetType());
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);

        Assert.Empty(card1.Comments);
    }

    [Fact]
    public async Task DeleteCommentAsync_Active_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .PleaseAsync(context);

        Assert.True(card1.Active);
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.AddCommentAsync(card1.Id, new AddCommentRequest
        {
            Comment = "Nice!",
        });

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);

        Assert.Single(card1.Comments);
        var comment = card1.Comments.First();

        var deleteResult = await target.DeleteCommentAsync(card1.Id, comment.Id);
        Assert.Equal(typeof(OkResult), deleteResult.GetType());
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);
        Assert.Empty(card1.Comments);
    }

    [Fact]
    public async Task DeleteCommentAsync_Inactive_OkAsync()
    {
        await using var context = new SqliteContext();
        var user = await new FakeUser(Role.Interviewer).PleaseAsync(context);

        var organization = await new FakeOrganization(user)
            .WithUser(user)
            .PleaseAsync(context);

        var candidate1 = await new FakeCandidate(organization).PleaseAsync(context);
        var card1 = await new FakeCandidateCard(candidate1, user, EmploymentStatus.HrInterview)
            .PleaseAsync(context);

        Assert.True(card1.Active);
        Assert.False(await context.Set<CandidateCardComment>().AnyAsync());

        var target = new CandidateCardsController(context, new FakeAuth(user));
        var updateResult = await target.AddCommentAsync(card1.Id, new AddCommentRequest
        {
            Comment = "Nice!",
        });

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);

        card1.Archive();
        await context.SaveChangesAsync();
        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);

        Assert.Single(card1.Comments);
        Assert.False(card1.Active);
        var comment = card1.Comments.First();

        var deleteResult = await target.DeleteCommentAsync(card1.Id, comment.Id);
        Assert.Equal(typeof(BadRequestObjectResult), deleteResult.GetType());
        Assert.True(await context.Set<CandidateCardComment>().AnyAsync());

        card1 = await context.CandidateCards
            .Include(x => x.Comments)
            .ByIdOrFailAsync(card1.Id);
        Assert.Single(card1.Comments);
    }
}