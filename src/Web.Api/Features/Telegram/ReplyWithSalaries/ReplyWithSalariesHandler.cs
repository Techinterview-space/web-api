using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.Enums;
using Domain.Extensions;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Salaries;
using Infrastructure.Services.Global;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot.Types.ReplyMarkups;
using Web.Api.Features.Telegram.ProcessMessage;

namespace Web.Api.Features.Telegram.ReplyWithSalaries;

public class ReplyWithSalariesHandler : IRequestHandler<ReplyWithSalariesCommand, TelegramBotReplyData>
{
    private readonly ICurrencyService _currencyService;
    private readonly IGlobal _global;
    private readonly DatabaseContext _context;

    public ReplyWithSalariesHandler(
        ICurrencyService currencyService,
        IGlobal global,
        DatabaseContext context)
    {
        _currencyService = currencyService;
        _global = global;
        _context = context;
    }

    public async Task<TelegramBotReplyData> Handle(ReplyWithSalariesCommand request, CancellationToken cancellationToken)
    {
        var salariesQuery = new SalariesForChartQuery(
            _context,
            request.Params,
            DateTimeOffset.Now.AddMonths(-12),
            DateTimeOffset.Now);

        var totalCount = await salariesQuery.ToQueryable().CountAsync(cancellationToken);
        var salaries = await salariesQuery
            .ToQueryable(CompanyType.Local)
            .Select(x => new
            {
                x.Grade,
                x.Value,
            })
            .ToListAsync(cancellationToken);

        var frontendLink = new SalariesChartPageLink(_global, request.Params);
        var professions = request.Params.GetProfessionsTitleOrNull();

        string replyText;
        if (salaries.Count > 0)
        {
            var currencies = await _currencyService.GetCurrenciesAsync(
                [Currency.USD],
                cancellationToken);

            var gradeGroups = EnumHelper
                     .Values<GradeGroup>()
                     .Where(x => x is not(GradeGroup.Undefined or GradeGroup.Trainee));

            replyText = $"Зарплаты {professions ?? "специалистов IT в Казахстане"} по грейдам:\n";

            foreach (var gradeGroup in gradeGroups)
            {
                var median = salaries
                                .Where(x => x.Grade.GetGroupNameOrNull() == gradeGroup)
                                .Select(x => x.Value)
                                .Median();

                if (median > 0)
                {
                    var resStr = $"<b>{median.ToString("N0", CultureInfo.InvariantCulture)}</b> тг.";
                    foreach (var currencyContent in currencies)
                    {
                        resStr += $" (~{(median / currencyContent.Value).ToString("N0", CultureInfo.InvariantCulture)}{currencyContent.CurrencyString})";
                    }

                    replyText += $"\n{gradeGroup.ToCustomString()}: {resStr}";
                }
            }

            replyText += $"<em>\n\nРассчитано на основе {totalCount} анкет(ы)</em>" +
                $"\n<em>Подробно на сайте <a href=\"{frontendLink}\">{request.ApplicationName}</a></em>";
        }
        else
        {
            replyText = professions != null
                ? $"Пока никто не оставил информацию о зарплатах для {professions}."
                : "Пока никто не оставлял информации о зарплатах.";

            replyText += $"\n\n<em>Посмотреть зарплаты по другим специальностям можно " +
                $"на сайте <a href=\"{frontendLink}\">{request.ApplicationName}</a></em>";
        }

        return new TelegramBotReplyData(
            replyText.Trim(),
            new InlineKeyboardMarkup(
                InlineKeyboardButton.WithUrl(
                    text: request.ApplicationName,
                    url: frontendLink.ToString())));
    }
}
