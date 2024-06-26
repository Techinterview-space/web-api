﻿using Domain.ValueObjects.Pagination;
using MediatR;
using Web.Api.Features.Salaries.Models;

namespace Web.Api.Features.Salaries.Admin.GetSourcedSalaries;

public record GetSourcedSalariesQuery : GetAllSalariesQueryParams, IRequest<Pageable<UserSalaryAdminDto>>;