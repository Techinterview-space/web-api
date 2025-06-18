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
            try
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("techinterview.space"));
                var user = await githubClient.User.Get(username);
                
                // Get extended profile data
                var extendedData = await GetExtendedGitHubProfileDataAsync(githubClient, username, cancellationToken);
                
                textToSend = $"Hello, {user.Name}!\n\n" +
                             $"Your GitHub profile: {user.HtmlUrl}\n" +
                             $"Followers: {user.Followers}\n" +
                             $"Following: {user.Following}\n" +
                             $"Public Repos: {user.PublicRepos}\n" +
                             $"Private repos: {user.TotalPrivateRepos}\n" +
                             extendedData;
            }
            catch (NotFoundException notFoundEx)
            {
                _logger.LogWarning(
                    notFoundEx,
                    "GitHub user not found: {Username}. Exception: {Exception}",
                    username,
                    notFoundEx.Message);

                textToSend = "GitHub user not found. Please provide a valid GitHub username in the format: @username or username";
            }
            catch (RateLimitExceededException rateLimitEx)
            {
                _logger.LogWarning(
                    rateLimitEx,
                    "GitHub API rate limit exceeded for user: {Username}. Exception: {Exception}",
                    username,
                    rateLimitEx.Message);

                textToSend = "GitHub API rate limit exceeded. Please try again later.";
            }
            catch (Exception e)
            {
                _logger.LogWarning(
                    e,
                    "An error occurred while processing GitHub profile for user: {Username}. Exception: {Exception}",
                    username,
                    e.Message);

                textToSend = "An error occurred while processing your request. Please try again later.";
            }
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
                 "You can get information about a GitHub user by sending their username in the format: @username or username. " +
                 "For example, send @octocat to get information about the user 'octocat'. \n\n" +
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

    private async Task<string> GetExtendedGitHubProfileDataAsync(
        GitHubClient githubClient, 
        string username, 
        CancellationToken cancellationToken)
    {
        try
        {
            string issuesCount = "N/A", prsCount = "N/A", totalStars = "N/A", forksCount = "N/A";

            // Start all tasks concurrently to minimize API calls
            var reposTask = githubClient.Repository.GetAllForUser(username);
            
            var issueSearchRequest = new SearchIssuesRequest
            {
                Author = username,
                Type = IssueTypeQualifier.Issue
            };
            var issuesTask = githubClient.Search.SearchIssues(issueSearchRequest);

            var prSearchRequest = new SearchIssuesRequest
            {
                Author = username,
                Type = IssueTypeQualifier.PullRequest
            };
            var prsTask = githubClient.Search.SearchIssues(prSearchRequest);

            // Wait for all tasks to complete
            await Task.WhenAll(reposTask, issuesTask, prsTask);

            // Process repositories data
            var repos = await reposTask;
            totalStars = repos.Sum(r => r.StargazersCount).ToString();
            forksCount = repos.Count(r => r.Fork).ToString();

            // Process search results
            var issuesResult = await issuesTask;
            issuesCount = issuesResult.TotalCount.ToString();

            var prsResult = await prsTask;
            prsCount = prsResult.TotalCount.ToString();

            return $"\n\n<b>Additional Stats:</b>\n" +
                   $"Issues created: {issuesCount}\n" +
                   $"Pull requests: {prsCount}\n" +
                   $"Total stars received: {totalStars}\n" +
                   $"Repositories forked: {forksCount}\n";
        }
        catch (RateLimitExceededException)
        {
            // Re-throw rate limit exceptions to be handled by the main handler
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get extended GitHub profile data for user: {Username}", username);
            return "\n\n<i>Extended stats temporarily unavailable</i>\n";
        }
    }
}