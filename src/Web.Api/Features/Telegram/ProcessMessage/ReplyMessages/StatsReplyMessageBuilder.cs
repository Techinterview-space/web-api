using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram.ProcessMessage.ReplyMessages;

public class StatsReplyMessageBuilder
{
    private readonly DatabaseContext _context;

    public StatsReplyMessageBuilder(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<TelegramBotReplyData> BuildAsync(
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var dayAgo = now.AddDays(-1);
        var allSalariesCount = await _context.Salaries
            .CountAsync(cancellationToken);

        var addedRelevantSalariesCount = await _context.Salaries
            .Where(x => x.UseInStats)
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var addedIrrelevantSalariesCount = await _context.Salaries
            .Where(x => !x.UseInStats)
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var surveyPassedCount = await _context.SalariesSurveyReplies
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var newUsersCount = await _context.Users
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var newCompanyReviewsCount = await _context.CompanyReviews
            .Where(x => x.CreatedAt >= dayAgo)
            .CountAsync(cancellationToken);

        var messageToSend = $@"
Всего анкет: {allSalariesCount}

За последние сутки:
- Новых пользователей: <b>+{newUsersCount}</b>
- Добавлено релевантных анкет: <b>+{addedRelevantSalariesCount}</b>
- Добавлено нерелевантных анкет: <b>+{addedIrrelevantSalariesCount}</b>
- Прошли опрос: <b>+{surveyPassedCount}</b>
- Отзывов о компаниях: <b>+{newCompanyReviewsCount}</b>

<em>Даты выборки: [{dayAgo:yyyy-MM-dd HH:mm:ss} - {now:yyyy-MM-dd HH:mm:ss}]</em>";

        return new TelegramBotReplyData(
            messageToSend,
            parseMode: ParseMode.Html);
    }
}