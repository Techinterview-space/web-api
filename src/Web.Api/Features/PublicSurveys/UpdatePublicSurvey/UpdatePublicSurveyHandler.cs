using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Microsoft.EntityFrameworkCore;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.UpdatePublicSurvey;

public class UpdatePublicSurveyHandler
    : IRequestHandler<UpdatePublicSurveyCommand, PublicSurveyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public UpdatePublicSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<PublicSurveyDto> Handle(
        UpdatePublicSurveyCommand request,
        CancellationToken cancellationToken)
    {
        request.Body.ThrowIfInvalid();

        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var survey = await _context.PublicSurveys
            .Include(s => s.Questions)
            .ThenInclude(q => q.Options)
            .Include(s => s.Questions)
            .ThenInclude(q => q.Responses)
            .FirstOrDefaultAsync(
                s => s.Id == request.SurveyId && s.AuthorId == user.Id,
                cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

        if (request.Body.Description != null)
        {
            survey.UpdateDescription(request.Body.Description);
        }

        if (survey.IsDraft())
        {
            if (request.Body.Title != null)
            {
                survey.UpdateTitle(request.Body.Title);
            }

            if (request.Body.Slug != null)
            {
                survey.UpdateSlug(request.Body.Slug);
            }

            var question = survey.Questions.FirstOrDefault();
            if (question != null)
            {
                if (request.Body.Question != null)
                {
                    question.UpdateText(request.Body.Question);
                }

                if (request.Body.AllowMultipleChoices.HasValue)
                {
                    question.SetAllowMultipleChoices(request.Body.AllowMultipleChoices.Value);
                }

                if (request.Body.Options != null)
                {
                    _context.PublicSurveyOptions.RemoveRange(question.Options);
                    question.Options.Clear();
                    for (int i = 0; i < request.Body.Options.Count; i++)
                    {
                        var optionRequest = request.Body.Options[i];
                        question.AddOption(optionRequest.Text, optionRequest.Order);
                    }
                }
            }
        }
        else
        {
            if (request.Body.Title != null ||
                request.Body.Slug != null ||
                request.Body.Question != null ||
                request.Body.AllowMultipleChoices.HasValue ||
                request.Body.Options != null)
            {
                throw new BadRequestException("Only description can be updated for published or closed surveys.");
            }
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        return new PublicSurveyDto(survey, isAuthor: true);
    }
}
