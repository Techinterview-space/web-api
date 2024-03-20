using Infrastructure.Salaries;
using Telegram.Bot.Types.ReplyMarkups;

namespace TechInterviewer.Features.Telegram.ProcessMessage;

public record TelegramBotStartCommandReplyData : TelegramBotReplyData
{
    public TelegramBotStartCommandReplyData(
        SalariesChartPageLink frontendLink)
        : base(
            PrepareText(frontendLink),
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    text: ProcessTelegramMessageHandler.ApplicationName,
                    url: frontendLink.ToString())))
    {
    }

    private static string PrepareText(
        SalariesChartPageLink frontendLink)
    {
        return $@"Привет!

Я - бот-ассистент проекта по сбору статистики по зарплатам в IT в Казахстане <a href=""{frontendLink}"">{ProcessTelegramMessageHandler.ApplicationName}</a>.
Я могу подсказать в чате, какие зарплаты у разных специалистов по грейдам.
Попробуй тегнуть меня в груповом чате и ввести ""frontend"" и выбери вариант в выпадающем меню - в итоге в чат будет отправлена статистика по зарплатам фронтенд-разработчиков.

<em>Вопросы и предложения: @maximgorbatyuk</em>
<em>Github проекта: <a href=""https://github.com/Techinterview-space"">репозитории</a></em>
<em>Поддержать копейкой: <a href=""https://boosty.to/ake111aa"">boosty.to</a></em>";
    }
}