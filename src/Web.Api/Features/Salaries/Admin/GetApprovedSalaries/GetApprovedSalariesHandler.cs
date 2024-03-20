using System.Threading;
using System.Threading.Tasks;
using Domain.Database;
using Domain.ValueObjects.Pagination;
using MediatR;
using TechInterviewer.Controllers.Salaries;

namespace TechInterviewer.Features.Salaries.Admin.GetApprovedSalaries;

public class GetApprovedSalariesHandler
    : IRequestHandler<GetApprovedSalariesQuery, Pageable<UserSalaryAdminDto>>
{
    private readonly DatabaseContext _context;

    public GetApprovedSalariesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<UserSalaryAdminDto>> Handle(
        GetApprovedSalariesQuery request,
        CancellationToken cancellationToken)
    {
        var query = new AdminSalariesQuery(_context).ToQueryable(request, true);
        return await query.AsPaginatedAsync(request, cancellationToken);
    }
}