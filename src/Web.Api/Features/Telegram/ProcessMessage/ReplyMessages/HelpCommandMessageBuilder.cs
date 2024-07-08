using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram.ProcessMessage.ReplyMessages;

public record HelpCommandMessageBuilder
{
    private readonly IGlobal _globalSettings;

    public HelpCommandMessageBuilder(
        IGlobal globalSettings)
    {
        _globalSettings = globalSettings;
    }

    public TelegramBotReplyData Build()
    {
        var salariesLink = new ChartPageLink(_globalSettings, null);
        var surveyLink = $"{_globalSettings.FrontendBaseUrl}/salaries/survey";
        var historicalDataLink = $"{_globalSettings.FrontendBaseUrl}/salaries/historical-data";
        var botInstructionLink = $"{_globalSettings.FrontendBaseUrl}/about-telegram-bot";
        var aboutProjectLink = $"{_globalSettings.FrontendBaseUrl}/about-us";
        var userAgreementLink = $"{_globalSettings.FrontendBaseUrl}/agreements/privacy-policy";

        var replyText = $@"Вас приветствует бот @techinterview_salaries_bot!

Я - часть проекта techinterview.space и нужен для того, чтобы вы могли быстро поделиться информацией в чатах, касающейся зарплат в IT в Казахстане.

Я могу подсказать не только зарплаты по всем специальностям, но и по указанным вами. На сайте есть <a href=""{botInstructionLink}"">инструкция</a> с гифками, как это сделать, но если кратко, то нужно сделать следующее:

1. тегнуть <b>@techinterview_salaries_bot</b> в поле сообщения,
2а. появится всплывающая инлайн кнопка <em>""Все специальности""</em>, которую можно тапнуть,
2б. или ввести название специальности, например <em>""frontend""</em>, и выбрать из предложенных вариантов.

В результате бот напишет сообщение, где будут представлены медианные зарплаты по грейдам.

<em>Статистика:</em>
- <em><a href=""{salariesLink}"">Статистика зарплат</a></em>
- <em><a href=""{historicalDataLink}"">Исторические данные</a></em>
- <em><a href=""{surveyLink}"">Обратная связь по проекту</a></em>

<em>Дополнительно:</em>
- <em><a href=""{aboutProjectLink}"">О проекте</a></em>
- <em><a href=""{botInstructionLink}"">Инструкция по боту</a></em>
- <em><a href=""{userAgreementLink}"">Пользовательское соглашение</a></em>
";

        return new TelegramBotReplyData(
            replyText.Trim(),
            null,
            ParseMode.Html);
    }
}