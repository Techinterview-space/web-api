using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Infrastructure.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Dashboard.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Dashboard;

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
        var yearAgo = DateTime.UtcNow.AddYears(-1);
        var feedbackReviews = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= yearAgo)
            .Select(x => x.UsefulnessRating)
            .ToListAsync(cancellationToken);

        var telegramBotInlineUsages = await _context.TelegramInlineReplies
            .Where(x => x.CreatedAt >= yearAgo)
            .Select(x => new TelegramInlineUsageSourceItem
            {
                CreatedAt = x.CreatedAt,
                Username = x.Username,
                ChatId = x.ChatId,
                ChatName = x.ChatName
            })
            .ToListAsync(cancellationToken);

        return new AdminDashboardData(
            new AverageRatingData(feedbackReviews),
            new TelegramInlineUsagesData(telegramBotInlineUsages));
    }
}