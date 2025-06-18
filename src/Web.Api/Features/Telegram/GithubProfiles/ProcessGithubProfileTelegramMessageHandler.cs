using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;
using Octokit;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Web.Api.Features.Telegram.GithubProfiles;

public class ProcessGithubProfileTelegramMessageHandler
    : IRequestHandler<ProcessTelegramMessageCommand, string>
{
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
            var githubClient = new GitHubClient(new ProductHeaderValue("techinterview.space"));
            var user = await githubClient.User.Get(username);
            textToSend = $"Hello, {user.Name}!\n\n" +
                         $"Your GitHub profile: {user.HtmlUrl}\n" +
                         $"Followers: {user.Followers}\n" +
                         $"Following: {user.Following}\n" +
                         $"Public Repos: {user.PublicRepos}\n" +
                         $"Private repos: {user.TotalPrivateRepos}\n";
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