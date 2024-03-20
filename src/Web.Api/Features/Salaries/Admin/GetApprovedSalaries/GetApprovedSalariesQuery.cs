using Domain.ValueObjects.Pagination;
using MediatR;
using TechInterviewer.Controllers.Salaries;

namespace TechInterviewer.Features.Salaries.Admin.GetApprovedSalaries;

public record GetApprovedSalariesQuery : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;