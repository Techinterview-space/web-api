using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Admin.DashboardModels;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Admin;

[ApiController]
[Route("api/admin/dashboard")]
[HasAnyRole(Role.Admin)]
public class AdminDashboardController : ControllerBase
{
    private readonly DatabaseContext _context;

    public AdminDashboardController(
        DatabaseContext context)
    {
        _context = context;
    }

    [HttpGet("")]
    public async Task<AdminDashboardData> GetDashboard(
        CancellationToken cancellationToken)
    {
        var yearAgo = DateTimeOffset.UtcNow.AddYears(-1);
        var tenDaysAgoEdge = DateTimeOffset.UtcNow.AddDays(-10);

        var feedbackReviews = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= yearAgo)
            .Select(x => x.UsefulnessRating)
            .ToListAsync(cancellationToken);

        var salariesCount = await _context.Salaries
            .Where(x =>
                x.CreatedAt >= yearAgo &&
                x.UseInStats)
            .CountAsync(cancellationToken);

        var reviewsCount = await _context.CompanyReviews
            .Where(x =>
                x.CreatedAt >= yearAgo &&
                x.ApprovedAt != null &&
                x.OutdatedAt == null)
            .CountAsync(cancellationToken);

        var usersWithUnsubscribeMeFromAllCount = await _context.Users
            .Where(x => x.UnsubscribeMeFromAll)
            .CountAsync(cancellationToken);

        var userEmailsForLastTenDays = await _context.UserEmails
            .Where(x => x.CreatedAt >= tenDaysAgoEdge)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var likesForLastTenDays = await _context.CompanyReviewVotes
            .Where(x => x.CreatedAt >= tenDaysAgoEdge)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var reviewsForLastTenDays = await _context.CompanyReviews
            .Where(x => x.CreatedAt >= tenDaysAgoEdge)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var messagesToGithubProfileBot = await _context.GithubProfileBotMessages
            .Include(x => x.GithubProfileBotChat)
            .Where(x =>
                x.CreatedAt >= tenDaysAgoEdge &&
                !x.GithubProfileBotChat.IsAdmin)
            .Select(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new AdminDashboardData(
            averageRatingData: new AverageRatingData(feedbackReviews),
            totalSalaries: salariesCount,
            totalCompanyReviews: reviewsCount,
            usersWithUnsubscribeMeFromAllCount: usersWithUnsubscribeMeFromAllCount,
            userEmailsSourceData: userEmailsForLastTenDays,
            reviewLikesForLastDays: likesForLastTenDays,
            reviewsForLastDays: reviewsForLastTenDays,
            salariesBotMessages: new List<DateTimeOffset>(0),
            githubProfileBotMessages: messagesToGithubProfileBot);
    }
}