using System;
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

        string textToSend;

        var username = message.Text?.Trim().TrimStart('@');
        if (string.IsNullOrEmpty(username))
        {
            try
            {
                var githubClient = new GitHubClient(new ProductHeaderValue("techinterview.space"));
                var user = await githubClient.User.Get(username);
                textToSend = $"Hello, {user.Name}!\n\n" +
                             $"Your GitHub profile: {user.HtmlUrl}\n" +
                             $"Followers: {user.Followers}\n" +
                             $"Following: {user.Following}\n" +
                             $"Public Repos: {user.PublicRepos}\n" +
                             $"Private repos: {user.TotalPrivateRepos}\n";
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
}