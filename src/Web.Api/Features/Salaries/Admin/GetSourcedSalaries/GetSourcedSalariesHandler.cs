using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Salaries;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetSourcedSalaries;

public class GetSourcedSalariesHandler
    : IRequestHandler<GetSourcedSalariesQuery, Pageable<UserSalaryAdminDto>>
{
    private readonly DatabaseContext _context;

    public GetSourcedSalariesHandler(
        DatabaseContext context)
    {
        _context = context;
    }

    public async Task<Pageable<UserSalaryAdminDto>> Handle(
        GetSourcedSalariesQuery request,
        CancellationToken cancellationToken)
    {
        var query = new SalariesAdminQuery(_context)
            .ApplyFilters(request)
            .WithSource(SalarySourceType.KolesaDevelopersCsv2022)
            .ApplyShowInStats(true)
            .ApplyOrder(request.OrderType)
            .ToAdminDtoQueryable();

        return await query.AsPaginatedAsync(request, cancellationToken);
    }
}