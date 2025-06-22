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
using Octokit;
using Octokit.Internal;
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
    private readonly GithubClientService _githubClientService;
    private readonly IGithubGraphQLService _githubGraphQLService;
    private readonly DatabaseContext _context;
    private readonly IConfiguration _configuration;

    public ProcessGithubProfileTelegramMessageHandler(
        ILogger<ProcessGithubProfileTelegramMessageHandler> logger,
        GithubClientService githubClientService,
        IGithubGraphQLService githubGraphQLService,
        DatabaseContext context,
        IConfiguration configuration)
    {
        _logger = logger;
        _githubClientService = githubClientService;
        _githubGraphQLService = githubGraphQLService;
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
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

        // Handle inline query click logging
        var messageSentByBot =
            message.ViaBot is not null &&
            message.ViaBot.Username == telegramBotUsername;

        if (messageSentByBot)
        {
            await AddInlineQueryClickAsync(
                message,
                cancellationToken);

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

    private async Task<(GithubProfileData User, string ErrorReplyTextOrNull)> GetExtendedGitHubProfileDataAsync(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            // Try GraphQL service first for better performance
            try
            {
                var profileData = await _githubGraphQLService.GetUserProfileDataAsync(
                    username,
                    MonthsToFetchCommits,
                    cancellationToken);

                return (profileData, null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "GraphQL service failed for user {Username}, falling back to REST API",
                    username);

                // Fall back to the original REST implementation
                return await GetExtendedGitHubProfileDataUsingRestAsync(username, cancellationToken);
            }
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "An error occurred while processing GitHub profile for user: {Username}. Exception: {Exception}",
                username,
                e.Message);

            return (null, "An error occurred while processing your request. Please try again later.");
        }
    }

    private async Task<(GithubProfileDataBasedOnOctokitData User, string ErrorReplyTextOrNull)> GetExtendedGitHubProfileDataUsingRestAsync(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            var user = await _githubClientService.GetUserAsync(username, cancellationToken);

            var commitsCount = 0;
            var filesAdjusted = 0;
            var changesInFilesCount = 0;
            var additionsInFilesCount = 0;
            var deletionsInFilesCount = 0;

            var userOrganizations = await _githubClientService.GetOrganizationsAsync(username, cancellationToken);
            foreach (var userOrganization in userOrganizations)
            {
                var userOrganizationRepositories = await _githubClientService.GetOrganizationRepositoriesAsync(userOrganization.Login, cancellationToken);
                var orgRepoStats = await CalculateRepositoriesStatsAsync(
                    userOrganizationRepositories,
                    userOrganization.Login,
                    username,
                    _githubClientService,
                    cancellationToken);

                commitsCount += orgRepoStats.CommitsCount;
                filesAdjusted += orgRepoStats.FilesAdjusted;
                changesInFilesCount += orgRepoStats.ChangesInFilesCount;
                additionsInFilesCount += orgRepoStats.AdditionsInFilesCount;
                deletionsInFilesCount += orgRepoStats.DeletionsInFilesCount;
            }

            // Start all tasks concurrently to minimize API calls
            var userRepositories = await _githubClientService.GetUserRepositoriesAsync(username, cancellationToken);

            var userRepositoriesStats = await CalculateRepositoriesStatsAsync(
                userRepositories,
                username,
                username,
                _githubClientService,
                cancellationToken);

            commitsCount += userRepositoriesStats.CommitsCount;
            filesAdjusted += userRepositoriesStats.FilesAdjusted;
            changesInFilesCount += userRepositoriesStats.ChangesInFilesCount;
            additionsInFilesCount += userRepositoriesStats.AdditionsInFilesCount;
            deletionsInFilesCount += userRepositoriesStats.DeletionsInFilesCount;

            var issuesResult = await _githubClientService.SearchUserIssuesAsync(username, cancellationToken);
            var prsResult = await _githubClientService.SearchUserPullRequestsAsync(username, cancellationToken);

            // Fetch new data
            var discussionsResult = await _githubClientService.SearchUserDiscussionsAsync(username, cancellationToken);
            var codeReviewsCount = await _githubClientService.GetUserCodeReviewsCountAsync(username, MonthsToFetchCommits, cancellationToken);
            var topLanguages = await _githubClientService.GetTopLanguagesByCommitsAsync(
                userRepositories,
                username,
                MonthsToFetchCommits,
                3,
                cancellationToken);

            var userData = new GithubProfileDataBasedOnOctokitData(
                user,
                userRepositories,
                issuesResult,
                prsResult,
                commitsCount,
                filesAdjusted,
                changesInFilesCount,
                additionsInFilesCount,
                deletionsInFilesCount,
                discussionsResult.TotalCount,
                codeReviewsCount,
                topLanguages,
                MonthsToFetchCommits);

            return (userData, null);
        }
        catch (NotFoundException notFoundEx)
        {
            _logger.LogWarning(
                notFoundEx,
                "GitHub user not found: {Username}. Exception: {Exception}",
                username,
                notFoundEx.Message);

            return (null, "GitHub user not found. Please provide a valid GitHub username in the format: @@username or username");
        }
        catch (RateLimitExceededException rateLimitEx)
        {
            _logger.LogWarning(
                rateLimitEx,
                "GitHub API rate limit exceeded for user: {Username}. Exception: {Exception}",
                username,
                rateLimitEx.Message);

            return (null, "GitHub API rate limit exceeded. Please try again later.");
        }
        catch (Exception e)
        {
            _logger.LogWarning(
                e,
                "An error occurred while processing GitHub profile for user: {Username}. Exception: {Exception}",
                username,
                e.Message);

            return (null, "An error occurred while processing your request. Please try again later.");
        }
    }

    private async Task<RepositoryChangesStats> CalculateRepositoriesStatsAsync(
        IReadOnlyList<Repository> repositories,
        string repoOwner,
        string username,
        GithubClientService gitHubClient,
        CancellationToken cancellationToken)
    {
        var commitsCount = 0;
        var filesAdjusted = 0;
        var changesInFilesCount = 0;
        var additionsInFilesCount = 0;
        var deletionsInFilesCount = 0;

        foreach (var repo in repositories)
        {
            var commitsResult = await gitHubClient.GetRepositoryCommitsAsync(
                repoOwner,
                repo.Name,
                username,
                MonthsToFetchCommits,
                cancellationToken);

            commitsCount += commitsResult.Count;
            foreach (var commit in commitsResult)
            {
                var commitFiles = await gitHubClient.GetCommitAsync(
                    repoOwner,
                    repo.Name,
                    commit.Sha,
                    cancellationToken);

                filesAdjusted += commitFiles.Files?.Count ?? 0;
                if (commitFiles.Files is { Count: > 0 })
                {
                    foreach (var commitFile in commitFiles.Files)
                    {
                        changesInFilesCount += commitFile.Changes;
                        additionsInFilesCount += commitFile.Additions;
                        deletionsInFilesCount += commitFile.Deletions;
                    }
                }
            }
        }

        return new RepositoryChangesStats(
            commitsCount,
            filesAdjusted,
            changesInFilesCount,
            additionsInFilesCount,
            deletionsInFilesCount);
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

    private async Task AddInlineQueryClickAsync(
        Message message,
        CancellationToken cancellationToken)
    {
        if (message.From == null)
        {
            return;
        }

        var username = message.From.Username?.Trim() ?? message.From.Id.ToString();
        var chatId = message.Chat.Id;
        var chatName = message.Chat.Title?.Trim();

        _context.Add(
            new TelegramInlineReply(
                username,
                message.From.Id,
                chatId,
                chatName));

        await _context.TrySaveChangesAsync(cancellationToken);
    }

    private async Task<string> GetGithubProfileDataAsync(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            var existingGithubProfile = await _context.GithubProfiles
                .FirstOrDefaultAsync(x => x.Username == username, cancellationToken);

            GithubProfileData profileData = null;
            string errorReplyTextOrNull = null;

            var saveDaatabaseChanges = false;
            if (existingGithubProfile is not null)
            {
                existingGithubProfile.IncrementRequestsCount();
                profileData = existingGithubProfile.GetProfileDataIfRelevant();
                saveDaatabaseChanges = true;

                if (profileData is null)
                {
                    (profileData, errorReplyTextOrNull) = await GetExtendedGitHubProfileDataAsync(
                        username,
                        cancellationToken);

                    if (profileData is not null)
                    {
                        existingGithubProfile.SyncData(profileData);
                    }
                }
            }
            else
            {
                (profileData, errorReplyTextOrNull) = await GetExtendedGitHubProfileDataAsync(
                    username,
                    cancellationToken);

                if (profileData is not null)
                {
                    _context.Add(
                        new GithubProfile(
                            username,
                            profileData));

                    saveDaatabaseChanges = true;
                }
            }

            if (saveDaatabaseChanges)
            {
                await _context.SaveChangesAsync(cancellationToken);
            }

            return profileData?.GetTelegramFormattedText()
                   ?? errorReplyTextOrNull
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