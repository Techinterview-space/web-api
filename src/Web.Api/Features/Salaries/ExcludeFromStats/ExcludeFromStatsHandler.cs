using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.ExcludeFromStats;

public class ExcludeFromStatsHandler : Infrastructure.Services.Mediator.IRequestHandler<ExcludeFromStatsCommand, Nothing>
{
    private readonly DatabaseContext _context;

    public ExcludeFromStatsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Nothing> Handle(
        ExcludeFromStatsCommand request,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Salary record not found");

        salary.ExcludeFromStats();
        await _context.SaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}