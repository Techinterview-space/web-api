using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Microsoft.Extensions.Logging;
using Octokit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram.GithubProfiles;

public class ProcessGithubProfileTelegramMessageHandler
    : IRequestHandler<ProcessTelegramMessageCommand, string>
{
    private readonly ILogger<ProcessGithubProfileTelegramMessageHandler> _logger;

    public ProcessGithubProfileTelegramMessageHandler(
        ILogger<ProcessGithubProfileTelegramMessageHandler> logger)
    {
        _logger = logger;
    }

    public async Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        var message = request.UpdateRequest.Message;
        if (message is null)
        {
            return string.Empty;
        }

        var messageText = message.Text?.Trim();
        var textToSend = await ProcessSimpleTextAsync(
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
        if (!string.IsNullOrEmpty(username))
        {
            var (userData, errorReplyTextOrNull) = await GetExtendedGitHubProfileDataAsync(
                username,
                cancellationToken);

            textToSend = userData?.GetTelegramFormattedText() ?? errorReplyTextOrNull;
        }
        else
        {
            textToSend = "Please provide a valid GitHub username in the format: @username or username";
        }

        await request.BotClient.SendMessage(
            message.Chat.Id,
            textToSend,
            parseMode: ParseMode.Html,
            replyParameters: new ReplyParameters
            {
                MessageId = message.MessageId,
            },
            cancellationToken: cancellationToken);

        return textToSend;
    }

    private async Task<string> ProcessSimpleTextAsync(
        Message message,
        string messageText,
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        string textToSend = null;
        if (string.IsNullOrWhiteSpace(messageText))
        {
            textToSend = "Please provide a valid GitHub username in the format: @username or username";

            await request.BotClient.SendMessage(
                message.Chat.Id,
                textToSend,
                parseMode: ParseMode.Html,
                replyParameters: new ReplyParameters
                {
                    MessageId = message.MessageId,
                },
                cancellationToken: cancellationToken);

            return textToSend;
        }

        if (messageText is "/start" or "/help")
        {
            textToSend =
                "Welcome to the GitHub Profile Bot! " +
                 "You can get information about a GitHub user by sending their username in the format: <pre>@username</pre> or <pre>username</pre>. " +
                 "For example, send <pre>@octocat</pre> to get information about the user 'octocat'. \n\n" +
                 "If you have any question, feel free to drop a message to @maximgorbatyuk";

            await request.BotClient.SendMessage(
                message.Chat.Id,
                textToSend,
                parseMode: ParseMode.Html,
                replyParameters: new ReplyParameters
                {
                    MessageId = message.MessageId,
                },
                cancellationToken: cancellationToken);
        }

        return textToSend;
    }

    private async Task<(GithubProfileDataBasedOnOctokitData User, string ErrorReplyTextOrNull)> GetExtendedGitHubProfileDataAsync(
        string username,
        CancellationToken cancellationToken)
    {
        try
        {
            var githubClient = new GitHubClient(new ProductHeaderValue("techinterview.space"));
            var user = await githubClient.User.Get(username);

            // Start all tasks concurrently to minimize API calls
            var repos = await githubClient.Repository.GetAllForUser(username);
            var commitsCount = 0;
            var filesAdjusted = 0;
            var changesInFilesCount = 0;
            var additionsInFilesCount = 0;
            var deletionsInFilesCount = 0;

            foreach (var repo in repos)
            {
                var commitsResult = await githubClient.Repository.Commit.GetAll(
                    username,
                    repo.Name,
                    new CommitRequest
                    {
                        Author = username,
                        Since = DateTimeOffset.UtcNow.AddMonths(-6),
                    });

                commitsCount += commitsResult.Count;
                foreach (var commit in commitsResult)
                {
                    filesAdjusted += commit.Files?.Count ?? 0;
                    if (commit.Files != null && commit.Files.Count > 0)
                    {
                        foreach (var commitFile in commit.Files)
                        {
                            changesInFilesCount += commitFile.Changes;
                            additionsInFilesCount += commitFile.Additions;
                            deletionsInFilesCount += commitFile.Deletions;
                        }
                    }
                }
            }

            var issuesResult = await githubClient.Search.SearchIssues(
                new SearchIssuesRequest
                {
                    Author = username,
                    Type = IssueTypeQualifier.Issue,
                });

            var prsResult = await githubClient.Search.SearchIssues(
                new SearchIssuesRequest
                {
                    Author = username,
                    Type = IssueTypeQualifier.PullRequest
                });

            var userData = new GithubProfileDataBasedOnOctokitData(
                user,
                repos,
                issuesResult,
                prsResult,
                commitsCount,
                filesAdjusted,
                changesInFilesCount,
                additionsInFilesCount,
                deletionsInFilesCount);

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
}