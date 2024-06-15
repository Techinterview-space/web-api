using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetExcludedFromStatsSalaries;

public record GetExcludedFromStatsSalariesQuery
    : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;