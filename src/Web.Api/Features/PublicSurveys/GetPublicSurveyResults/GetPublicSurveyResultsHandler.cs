using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Enums;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.GetPublicSurveyResults;

public class GetPublicSurveyResultsHandler
    : IRequestHandler<string, PublicSurveyResultsDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetPublicSurveyResultsHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<PublicSurveyResultsDto> Handle(
        string slug,
        CancellationToken cancellationToken)
    {
        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var survey = await _context.PublicSurveys
            .AsNoTracking()
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .ThenInclude(o => o.ResponseOptions)
            .Include(s => s.Questions)
            .ThenInclude(q => q.Responses)
            .FirstOrDefaultAsync(
                s => s.Slug == slug && s.DeletedAt == null,
                cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

        if (survey.Status == PublicSurveyStatus.Draft)
        {
            throw new NotFoundException("Survey not found.");
        }

        var question = survey.Questions.FirstOrDefault()
            ?? throw new BadRequestException("Survey has no questions.");

        var hasResponded = question.Responses?.Any(r => r.UserId == user.Id) ?? false;
        if (!hasResponded)
        {
            throw new BadRequestException("You must respond to the survey to view results.");
        }

        var totalResponses = question.Responses?.Count ?? 0;

        var options = question.Options
            .OrderBy(o => o.Order)
            .Select(o =>
            {
                var responseCount = o.ResponseOptions?.Count ?? 0;
                return new PublicSurveyOptionResultDto
                {
                    Id = o.Id,
                    Text = o.Text,
                    ResponseCount = responseCount,
                    Percentage = totalResponses > 0
                        ? Math.Round((decimal)responseCount / totalResponses * 100, 1)
                        : 0,
                };
            })
            .ToList();

        return new PublicSurveyResultsDto
        {
            TotalResponses = totalResponses,
            Options = options,
        };
    }
}
