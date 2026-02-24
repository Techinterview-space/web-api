using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
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

        if (!survey.Questions.Any())
        {
            throw new BadRequestException("Survey has no questions.");
        }

        // User must have responded to all questions
        var hasRespondedToAll = survey.Questions
            .All(q => q.Responses?.Any(r => r.UserId == user.Id) ?? false);

        if (!hasRespondedToAll)
        {
            throw new BadRequestException("You must respond to the survey to view results.");
        }

        var totalResponses = survey.Questions
            .SelectMany(q => (IEnumerable<PublicSurveyResponse>)q.Responses ?? Enumerable.Empty<PublicSurveyResponse>())
            .Select(r => r.UserId)
            .Distinct()
            .Count();

        var questions = survey.Questions
            .OrderBy(q => q.Order)
            .Select(q =>
            {
                var questionTotalResponses = q.Responses?.Count ?? 0;

                var options = q.Options
                    .Select(o =>
                    {
                        var responseCount = o.ResponseOptions?.Count ?? 0;
                        return new PublicSurveyOptionResultDto
                        {
                            Id = o.Id,
                            Text = o.Text,
                            ResponseCount = responseCount,
                            Percentage = questionTotalResponses > 0
                                ? Math.Round((decimal)responseCount / questionTotalResponses * 100, 1)
                                : 0,
                        };
                    })
                    .OrderByDescending(o => o.ResponseCount)
                    .ToList();

                return new PublicSurveyQuestionResultDto
                {
                    Id = q.Id,
                    Text = q.Text,
                    Order = q.Order,
                    Options = options,
                };
            })
            .ToList();

        return new PublicSurveyResultsDto
        {
            TotalResponses = totalResponses,
            Questions = questions,
        };
    }
}
