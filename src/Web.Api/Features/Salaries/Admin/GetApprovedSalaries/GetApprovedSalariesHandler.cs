﻿using System.Threading;
using System.Threading.Tasks;
using Domain.ValueObjects.Pagination;
using Infrastructure.Database;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetApprovedSalaries;

public class GetApprovedSalariesHandler
    : Infrastructure.Services.Mediator.IRequestHandler<GetApprovedSalariesQuery, Pageable<UserSalaryAdminDto>>
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
        var query = new SalariesAdminQuery(_context)
            .ApplyFilters(request)
            .ApplyShowInStats(true)
            .ApplyOrder(request.OrderType)
            .ToAdminDtoQueryable();

        return await query.AsPaginatedAsync(request, cancellationToken);
    }
}