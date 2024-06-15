using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetApprovedSalaries;

public record GetApprovedSalariesQuery : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;