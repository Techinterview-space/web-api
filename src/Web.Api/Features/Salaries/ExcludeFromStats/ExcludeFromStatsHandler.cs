using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Web.Api.Features.Salaries.ExcludeFromStats;

public class ExcludeFromStatsHandler : IRequestHandler<ExcludeFromStatsCommand, Unit>
{
    private readonly DatabaseContext _context;

    public ExcludeFromStatsHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Unit> Handle(
        ExcludeFromStatsCommand request,
        CancellationToken cancellationToken)
    {
        var salary = await _context.Salaries
                         .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                     ?? throw new NotFoundException("Salary record not found");

        salary.ExcludeFromStats();
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}