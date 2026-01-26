using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.Github.Dtos;

namespace Web.Api.Features.Github.GetGithubProcessingJobs;

public class GetGithubProcessingJobsHandler : IRequestHandler<Nothing, GetGithubProcessingJobsResponse>
{
    private readonly DatabaseContext _context;

    public GetGithubProcessingJobsHandler(DatabaseContext context)
    {
        _context = context;
    }

    public async Task<GetGithubProcessingJobsResponse> Handle(
        Nothing request,
        CancellationToken cancellationToken)
    {
        var jobs = await _context.GithubProfileProcessingJobs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        return new GetGithubProcessingJobsResponse(
            jobs.Select(x => new GithubProfileProcessingJobDto(x)).ToList());
    }
}