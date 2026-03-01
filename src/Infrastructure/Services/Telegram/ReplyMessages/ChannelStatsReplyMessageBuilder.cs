using System;
using System.Text;
using Domain.Entities.ChannelStats;
using Telegram.Bot.Types.Enums;

namespace Infrastructure.Services.Telegram.ReplyMessages;

public static class ChannelStatsReplyMessageBuilder
{
    public static TelegramBotReplyData Build(
        MonthlyStatsRun run,
        string channelName)
    {
        var sb = new StringBuilder();

        sb.AppendLine($"<b>Статистика канала «{EscapeHtml(channelName)}»</b>");
        sb.AppendLine($"за {run.Month:MMMM yyyy}");
        sb.AppendLine();

        sb.AppendLine($"Всего постов: <b>{run.PostsCountTotal}</b>");
        sb.AppendLine($"Среднее постов в день: <b>{run.AveragePostsPerDay:F1}</b>");

        if (run.MaxPostsDayUtc.HasValue)
        {
            sb.AppendLine();
            sb.AppendLine($"Самый активный день: <b>{run.MaxPostsDayUtc.Value:dd.MM.yyyy}</b> ({run.MaxPostsCount} постов)");
            sb.AppendLine($"Самый тихий день: <b>{run.MinPostsDayUtc!.Value:dd.MM.yyyy}</b> ({run.MinPostsCount} постов)");
        }

        if (run.MostLikedPostId.HasValue)
        {
            sb.AppendLine();
            sb.Append($"Самый популярный пост: <b>{run.MostLikedPostLikes}</b> лайков");
            if (!string.IsNullOrEmpty(run.MostLikedPostRef))
            {
                sb.Append($" — <a href=\"{run.MostLikedPostRef}\">ссылка</a>");
            }

            sb.AppendLine();
        }

        if (run.MostCommentedPostId.HasValue)
        {
            sb.Append($"Самый обсуждаемый пост: <b>{run.MostCommentedPostComments}</b> комментариев");
            if (!string.IsNullOrEmpty(run.MostCommentedPostRef))
            {
                sb.Append($" — <a href=\"{run.MostCommentedPostRef}\">ссылка</a>");
            }

            sb.AppendLine();
        }

        sb.AppendLine();
        sb.AppendLine($"<em>Рассчитано: {run.CalculatedAtUtc:yyyy-MM-dd HH:mm:ss} UTC</em>");

        return new TelegramBotReplyData(
            sb.ToString(),
            parseMode: ParseMode.Html);
    }

    private static string EscapeHtml(string text)
    {
        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;");
    }
}
