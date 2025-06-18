using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Services.Mediator;

namespace Web.Api.Features.Telegram.GithubProfiles;

public class ProcessGithubProfileTelegramMessageHandler
    : IRequestHandler<ProcessTelegramMessageCommand, string>
{
    public Task<string> Handle(
        ProcessTelegramMessageCommand request,
        CancellationToken cancellationToken)
    {
        throw new System.NotImplementedException();
    }
}