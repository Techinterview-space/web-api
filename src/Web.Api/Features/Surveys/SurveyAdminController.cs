using System;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.ValueObjects.Pagination;
using Infrastructure.Services.Mediator;
using Microsoft.AspNetCore.Mvc;
using Web.Api.Features.Surveys.Admin.GetSurveyRepliesForAdmin;
using Web.Api.Features.Surveys.Dtos;
using Web.Api.Setup.Attributes;

namespace Web.Api.Features.Surveys;

[ApiController]
[Route("api/survey")]
[HasAnyRole(Role.Admin)]
public class SurveyAdminController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;

    public SurveyAdminController(
        IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    [HttpGet("admin/all")]
    public async Task<Pageable<SalariesSurveyReplyAdminDto>> GetAll(
        [FromQuery] GetSurveyRepliesForAdminQueryParams request,
        CancellationToken cancellationToken)
    {
        return await _serviceProvider.HandleBy<GetSurveyRepliesForAdminHandler, GetSurveyRepliesForAdminQueryParams, Pageable<SalariesSurveyReplyAdminDto>>(
            request,
            cancellationToken);
    }
}
