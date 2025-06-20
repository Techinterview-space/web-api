using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Github.DeleteGithubProcessingJob;

public class DeleteGithubProcessingJobHandler : IRequestHandler<DeleteGithubProcessingJobCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public DeleteGithubProcessingJobHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        DeleteGithubProcessingJobCommand request,
        CancellationToken cancellationToken)
    {
        var job = await _context.GithubProfileProcessingJobs
            .FirstOrDefaultAsync(x => x.Username == request.Username, cancellationToken);

        if (job == null)
        {
            throw new BadHttpRequestException($"Github processing job with username '{request.Username}' not found");
        }

        _context.GithubProfileProcessingJobs.Remove(job);
        await _context.TrySaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}