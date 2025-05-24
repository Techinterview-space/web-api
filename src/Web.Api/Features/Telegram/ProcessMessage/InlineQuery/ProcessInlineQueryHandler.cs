using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Telegram;
using Infrastructure.Currencies.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Global;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types.InlineQueryResults;
using Web.Api.Features.Telegram.GetTelegramBotUsages;
using Web.Api.Features.Telegram.ProcessMessage.UserCommands;
using Web.Api.Features.Telegram.ReplyWithSalaries;

namespace Web.Api.Features.Telegram.ProcessMessage.InlineQuery;

public class ProcessInlineQueryHandler : IRequestHandler<ProcessInlineQueryCommand, Unit>
{
    public const string ApplicationName = "techinterview.space/salaries";
    private const string CacheKey = "TelegramBotService_ReplyData";

    private const int CachingMinutes = 20;

    private readonly ILogger<ProcessInlineQueryHandler> _logger;
    private readonly IMediator _mediator;
    private readonly DatabaseContext _context;
    private readonly ICurrencyService _currencyService;
    private readonly IMemoryCache _cache;
    private readonly IGlobal _global;

    public ProcessInlineQueryHandler(
        ILogger<ProcessInlineQueryHandler> logger,
        IMediator mediator,
        ICurrencyService currencyService,
        DatabaseContext context,
        IMemoryCache cache,
        IGlobal global)
    {
        _logger = logger;
        _mediator = mediator;
        _currencyService = currencyService;
        _context = context;
        _cache = cache;
        _global = global;
    }

    public async Task<Unit> Handle(ProcessInlineQueryCommand request, CancellationToken cancellationToken)
    {
        var results = new List<InlineQueryResult>();

        var counter = 0;

        var updateRequest = request.UpdateRequest;
        var parametersForAllSalaries = new TelegramBotUserCommandParameters();
        var replyDataForAllSalaries = await _cache.GetOrCreateAsync(
            CacheKey + "_" + parametersForAllSalaries.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                return await _mediator.Send(
                                new ReplyWithSalariesCommand(
                                        parametersForAllSalaries,
                                        ApplicationName), cancellationToken);
            });

        results.Add(
            new InlineQueryResultArticle(
                counter.ToString(),
                "Вся статистика без фильтра по специальности",
                new InputTextMessageContent(replyDataForAllSalaries.ReplyText)
                {
                    ParseMode = replyDataForAllSalaries.ParseMode,
                }));

        counter++;

        if (updateRequest.InlineQuery?.Query != null &&
            updateRequest.InlineQuery.Query.Length > 1)
        {
            var requestedProfession = updateRequest.InlineQuery.Query.ToLowerInvariant();
            if (ProductManagersTelegramBotUserCommandParameters.ShouldIncludeGroup(requestedProfession))
            {
                results.Add(
                    await GetProfessionsGroupInlineResultAsync(
                        counter,
                        new ProductManagersTelegramBotUserCommandParameters(request.AllProfessions),
                        "Все продакты (Product managers)",
                        cancellationToken));

                counter++;
            }

            if (QaAndTestersTelegramBotUserCommandParameters.ShouldIncludeGroup(requestedProfession))
            {
                results.Add(
                    await GetProfessionsGroupInlineResultAsync(
                        counter,
                        new QaAndTestersTelegramBotUserCommandParameters(request.AllProfessions),
                        "Тестировщики, QA и автоматизаторы",
                        cancellationToken));

                counter++;
            }

            foreach (var profession in request.AllProfessions)
            {
                if (!profession.Title.Contains(requestedProfession, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var parameters = new TelegramBotUserCommandParameters(profession);

                var replyData = await _cache.GetOrCreateAsync(
                    CacheKey + "_" + parameters.GetKeyPostfix(),
                    async entry =>
                    {
                        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                        return await _mediator.Send(
                            new ReplyWithSalariesCommand(
                                    parameters,
                                    ApplicationName), cancellationToken);
                    });

                results.Add(new InlineQueryResultArticle(
                    counter.ToString(),
                    profession.Title,
                    new InputTextMessageContent(replyData.ReplyText)
                    {
                        ParseMode = replyData.ParseMode,
                    }));

                counter++;
            }
        }

        try
        {
            await request.BotClient.AnswerInlineQueryAsync(
                updateRequest.InlineQuery!.Id,
                results,
                cancellationToken: cancellationToken);

            var userName = updateRequest.InlineQuery.From.Username
                           ?? $"{updateRequest.InlineQuery.From.FirstName} {updateRequest.InlineQuery.From.LastName}".Trim();
            await _mediator.Send(
                new GetOrCreateTelegramBotUsageCommand(
                    userName,
                    null,
                    updateRequest.InlineQuery.Query,
                    TelegramBotUsageType.InlineQuery), cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred while answering inline query: {Message}",
                e.Message);
        }

        return Unit.Value;
    }

    private async Task<InlineQueryResultArticle> GetProfessionsGroupInlineResultAsync(
        int counter,
        TelegramBotUserCommandParameters professionGroupParams,
        string title,
        CancellationToken cancellationToken)
    {
        var professionsGroupReplyData = await _cache.GetOrCreateAsync(
            CacheKey + "_" + professionGroupParams.GetKeyPostfix(),
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(CachingMinutes);
                return await _mediator.Send(
                                new ReplyWithSalariesCommand(
                                        professionGroupParams,
                                        ApplicationName), cancellationToken);
            });

        return new InlineQueryResultArticle(
            counter.ToString(),
            title,
            new InputTextMessageContent(professionsGroupReplyData.ReplyText)
            {
                ParseMode = professionsGroupReplyData.ParseMode,
            });
    }
}
