using Infrastructure.Salaries;

namespace Web.Api.Features.Telegram.ProcessMessage;

public record TelegramBotStartCommandReplyData : TelegramBotReplyData
{
    public TelegramBotStartCommandReplyData(
        SalariesChartPageLink frontendLink)
        : base(
            PrepareText(frontendLink))
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
<em>Посмотреть код: <a href=""https://github.com/Techinterview-space"">Github</a></em>
<em>Поддержать денежкой: <a href=""https://boosty.to/ake111aa"">boosty.to</a></em>

<em>Любая помощь приветствуется!</em>";
    }
}