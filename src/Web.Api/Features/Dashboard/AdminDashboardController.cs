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
        var feedbackReviews = await _context.SalariesSurveyReplies
            .Select(x => x.UsefulnessRating)
            .ToListAsync(cancellationToken);

        var telegramBotInlineUsages = await _context.TelegramInlineReplies
            .Select(x => new TelegramInlineUsageItem
            {
                CreatedAt = x.CreatedAt,
                Username = x.Username
            })
            .ToListAsync(cancellationToken);

        return new AdminDashboardData(
            new AverageRatingData(feedbackReviews),
            new TelegramInlineUsagesData(telegramBotInlineUsages));
    }
}