using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Github.DeleteGithubProfile;

public class DeleteGithubProfileHandler : IRequestHandler<DeleteGithubProfileCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public DeleteGithubProfileHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        DeleteGithubProfileCommand request,
        CancellationToken cancellationToken)
    {
        var profile = await _context.GithubProfiles
            .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

        if (profile == null)
        {
            throw new BadHttpRequestException($"Github profile with username '{request.Username}' not found");
        }

        _context.GithubProfiles.Remove(profile);
        await _context.TrySaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}