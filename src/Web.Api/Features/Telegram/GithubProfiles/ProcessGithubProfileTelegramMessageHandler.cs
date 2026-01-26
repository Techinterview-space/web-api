using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Github;
using Domain.Entities.Telegram;
using Infrastructure.Database;
using Infrastructure.Services.Github;
using Infrastructure.Services.Mediator;
using Infrastructure.Services.Telegram;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InlineQueryResults;

namespace Web.Api.Features.Telegram.GithubProfiles;

public class ProcessGithubProfileTelegramMessageHandler
    : IRequestHandler<ProcessTelegramMessageCommand, string>
{
    private const int MonthsToFetchCommits = 12;

    private readonly ILogger<ProcessGithubProfileTelegramMessageHandler> _logger;
    private readonly IGithubGraphQLService _githubGraphQLService;
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public ProcessGithubProfileTelegramMessageHandler(
        ILogger<ProcessGithubProfileTelegramMessageHandler> logger,
        IGithubGraphQLService githubGraphQLService,
        DatabaseContext context,
        IConfiguration configuration)
    {
        _logger = logger;
        _githubGraphQLService = githubGraphQLService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        if (request.UpdateRequest.ChosenInlineResult is not null)
        {
            _logger.LogInformation(
                "TELEGRAM_BOT. Github. Processing ChosenInlineResult " +
                "with InlineMessageId: {InlineMessageId} " +
                "from {Name}. " +
                "Id {Id}. " +
                "Query {Query}. " +
                "IsBot {IsBot}",
                request.UpdateRequest.ChosenInlineResult.InlineMessageId,
                request.UpdateRequest.ChosenInlineResult.From.Username,
                request.UpdateRequest.ChosenInlineResult.From.Id,
                request.UpdateRequest.ChosenInlineResult.Query,
                request.UpdateRequest.ChosenInlineResult.From.IsBot);

            await _context.SaveAsync(
                new TelegramInlineReply(
                    request.UpdateRequest.ChosenInlineResult.From.Id,
                    request.UpdateRequest.ChosenInlineResult.Query,
                    TelegramBotType.Salaries),
                cancellationToken);

            return null;
        }

        // Handle inline queries
        if (request.UpdateRequest.Type == UpdateType.InlineQuery &&
            request.UpdateRequest.InlineQuery != null)
        {
            await ProcessInlineQueryAsync(
                request.BotClient,
                request.UpdateRequest,
                cancellationToken);

            return null;
        }

        var message = request.UpdateRequest.Message;
        if (message is null ||
            message.From?.IsBot == true)
        {
            return string.Empty;
        }

        var telegramBotUsername = _configuration["Telegram:GithubTelegramBotName"];

        // Handle inline query click logging only if the bot was added to the chat.
        // TODO: mgorbatyuk: catch inline replies and log them
        var messageSentByBot =
            message.ViaBot is not null &&
            message.ViaBot.Username == telegramBotUsername;

        if (messageSentByBot)
        {
            await _context.SaveAsync(
                new TelegramInlineReply(
                    message.Chat.Id,
                    message.Text,
                    TelegramBotType.GithubProfile),
                cancellationToken);

            return null;
        }

        if (message.Chat.Type is not ChatType.Private)
        {
            _logger.LogDebug(
                "Received message in non-private chat: {ChatId} from user: {Username}. Skipping",
                message.Chat.Id,
                message.From?.Username ?? message.From?.Id.ToString());

            return null;
        }

        var messageText = message.Text?.Trim();
        var chat = await AddOrUpdateChatAsync(
            message,
            cancellationToken);

        var textToSend = await ProcessSimpleTextAsync(
            chat,
            message,
            messageText,
            request,
            cancellationToken);

        if (textToSend is not null ||
            messageText is null)
        {
            return textToSend;
        }

        var username = messageText.TrimStart('@');
        if (string.IsNullOrEmpty(username))
        {
            textToSend = "Please provide a valid GitHub username in the format: @username or username";

            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);

            return textToSend;
        }

        if (await _context.GithubProfileProcessingJobs.AnyAsync(x => x.Username == username, cancellationToken))
        {
            textToSend = $"Profile {username} is already in progress. Please wait until the previous request is completed or send it again later.";
            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);

            return textToSend;
        }

        var job = await _context.SaveAsync(
            new GithubProfileProcessingJob(username),
            cancellationToken);

        try
        {
            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                $"Starting to fetch GitHub profile data for <b>{username}</b>. It might take 2-3 mins, please be patient...",
                cancellationToken);

            textToSend = await GetGithubProfileDataAsync(username, cancellationToken);

            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);

            return textToSend;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while fetching GitHub profile data for user: {Username}. Exception: {Exception}",
                username,
                ex.Message);

            textToSend = "An error occurred while processing your request. Please try again later.";
            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);
        }
        finally
        {
            _context.GithubProfileProcessingJobs.Remove(job);
            await _context.SaveChangesAsync(cancellationToken);
        }

        return textToSend;
    }

    private async Task<string> ProcessSimpleTextAsync(
        GithubProfileBotChat chat,
        Message message,
        string messageText,
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        string textToSend = null;
        if (message.Chat.Type != ChatType.Private)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(messageText) ||
            messageText.Contains(' '))
        {
            textToSend = "Please provide a valid GitHub username in the format: @username or username";

            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);

            return textToSend;
        }

        if (messageText is "/start" or "/help")
        {
            textToSend =
                "Welcome to the GitHub Profile Bot! " +
                 "You can get information about a GitHub user by sending their username in the format: <code>@username</code> or <code>username</code>. " +
                 "For example, send <code>@octocat</code> to get information about the user 'octocat'. \n\n" +
                 "If you have any question, feel free to drop a message to @maximgorbatyuk";

            await request.BotClient.ReplyToWithHtmlAsync(
                message.Chat.Id,
                message.MessageId,
                textToSend,
                cancellationToken);
        }

        return textToSend;
    }

    private async Task<GithubProfileBotChat> AddOrUpdateChatAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        var chatTobeReturned = await _context.GithubProfileBotChats
            .FirstOrDefaultAsync(x => x.ChatId == message.Chat.Id, cancellationToken);

        if (chatTobeReturned is not null)
        {
            chatTobeReturned.IncrementMessagesCount();
        }
        else
        {
            var adminUsername = _configuration["Telegram:AdminUsername"];
            chatTobeReturned = new GithubProfileBotChat(
                chatId: message.Chat.Id,
                username: message.From?.Username ?? message.Chat.Id.ToString(),
                isAdmin: !string.IsNullOrEmpty(adminUsername) && message.From?.Username == adminUsername);

            _context.Add(chatTobeReturned);
        }

        _context.Add(
            new GithubProfileBotMessage(
                message.Text?.Trim(),
                chatTobeReturned));

        await _context.SaveChangesAsync(cancellationToken);
        return chatTobeReturned;
    }

    private async Task ProcessInlineQueryAsync(
        ITelegramBotClient client,
        Update updateRequest,
        CancellationToken cancellationToken)
    {
        var results = new List<InlineQueryResult>();
        var query = updateRequest.InlineQuery?.Query?.Trim() ?? string.Empty;

        // Add a general help result
        results.Add(new InlineQueryResultArticle(
            "help",
            "How to use GitHub Profile Bot",
            new InputTextMessageContent(
                "Send a GitHub username to get profile information.\n\n" +
                "Examples:\n" +
                "• @octocat\n" +
                "• octocat\n\n" +
                "The bot will provide detailed information about the user's GitHub profile, repositories, and activity.")
            {
                ParseMode = ParseMode.Html,
            }));

        if (!string.IsNullOrEmpty(query) && query.Length >= 2)
        {
            // Clean the username (remove @ if present)
            var username = query.TrimStart('@');

            if (!string.IsNullOrEmpty(username) && IsValidGitHubUsername(username))
            {
                // Fetch the actual GitHub profile data for inline query
                var profileText = await GetGithubProfileDataAsync(username, cancellationToken);

                results.Add(new InlineQueryResultArticle(
                    $"profile_{username}",
                    $"Get GitHub profile for @{username}",
                    new InputTextMessageContent(profileText)
                    {
                        ParseMode = ParseMode.Html,
                    }));
            }
        }

        try
        {
            await client.AnswerInlineQuery(
                updateRequest.InlineQuery!.Id,
                results,
                cacheTime: 15 * 60,
                cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(
                e,
                "An error occurred while answering inline query: {Message}",
                e.Message);
        }
    }

    private async Task<string> GetGithubProfileDataAsync(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingGithubProfile = await _context.GithubProfiles
                .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

            GithubProfileDataResult profileDataResult = null;
            GithubProfileData profileData;

            var saveDatabaseChanges = false;
            if (existingGithubProfile is not null)
            {
                existingGithubProfile.IncrementRequestsCount();
                profileData = existingGithubProfile.GetProfileDataIfRelevant();
                saveDatabaseChanges = true;

                if (profileData is null)
                {
                    profileDataResult = await _githubGraphQLService.GetUserProfileDataAsync(
                        username,
                        MonthsToFetchCommits,
                        cancellationToken);

                    profileData = profileDataResult.Data;

                    if (profileData is not null)
                    {
                        existingGithubProfile.SyncData(profileData);
                    }
                }
            }
            else
            {
                profileDataResult = await _githubGraphQLService.GetUserProfileDataAsync(
                    username,
                    MonthsToFetchCommits,
                    cancellationToken);

                profileData = profileDataResult.Data;

                if (profileData is not null)
                {
                    _context.Add(
                        new GithubProfile(
                            username,
                            profileData));

                    saveDatabaseChanges = true;
                }
            }

            if (saveDatabaseChanges)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (profileDataResult != null)
            {
                switch (profileDataResult.Result)
                {
                    case GithubProfileDataResultType.Success:
                        return profileDataResult.Data.GetTelegramFormattedText();

                    case GithubProfileDataResultType.NotFound:
                        return $"Github user {username} was not found";

                    case GithubProfileDataResultType.RateLimitExceeded:
                    case GithubProfileDataResultType.Failure:
                        return profileDataResult.ErrorMessage;

                    default:
                        throw new InvalidOperationException("Unexpected result type from GitHub profile data service: " + profileDataResult.Result);
                }
            }

            return profileData?.GetTelegramFormattedText()
                   ?? "An error occurred while fetching GitHub profile data. Please try again later.";
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error while fetching GitHub profile data for user: {Username}. Exception: {Exception}",
                username,
                ex.Message);

            return "An error occurred while processing your request. Please try again later.";
        }
    }

    private static bool IsValidGitHubUsername(
        string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return false;
        }

        // GitHub username validation rules
        // - May only contain alphanumeric characters or single hyphens
        // - Cannot begin or end with a hyphen
        // - Maximum is 39 characters
        if (username.Length > 39)
        {
            return false;
        }

        if (username.StartsWith('-') || username.EndsWith('-'))
        {
            return false;
        }

        return username.All(c => char.IsLetterOrDigit(c) || c == '-');
    }
}