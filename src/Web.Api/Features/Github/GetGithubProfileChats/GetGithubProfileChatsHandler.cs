using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProfileChats;

public class GetGithubProfileChatsHandler
    : IRequestHandler<GetGithubProfileChatsQueryParams, GetGithubProfileChatsResponse>
{
    private readonly DatabaseContext _context;

    public GetGithubProfileChatsHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetGithubProfileChatsResponse> Handle(
        GetGithubProfileChatsQueryParams request,
        CancellationToken cancellationToken)
    {
        var chats = await _context.GithubProfileBotChats
            .OrderByDescending(x => x.MessagesCount)
            .ThenByDescending(x => x.CreatedAt)
            .AsPaginatedAsync(
                request,
                cancellationToken);

        return new GetGithubProfileChatsResponse(
            chats.CurrentPage,
            chats.PageSize,
            chats.TotalItems,
            chats.Results
                .Select(x => new GithubProfileBotChatDto(x))
                .ToList());
    }
}