using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
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

            if (request.Body.Questions != null)
            {
                // Remove all existing questions and their options
                foreach (var existingQuestion in survey.Questions.ToList())
                {
                    _context.PublicSurveyOptions.RemoveRange(existingQuestion.Options);
                }

                _context.PublicSurveyQuestions.RemoveRange(survey.Questions);
                survey.Questions.Clear();

                // Add new questions with their options via domain method
                foreach (var questionRequest in request.Body.Questions)
                {
                    var question = survey.AddQuestion(
                        questionRequest.Text,
                        questionRequest.Order,
                        questionRequest.AllowMultipleChoices);

                    foreach (var optionRequest in questionRequest.Options)
                    {
                        question.AddOption(optionRequest.Text, optionRequest.Order);
                    }
                }
            }
        }
        else
        {
            if (request.Body.Title != null ||
                request.Body.Slug != null ||
                request.Body.Questions != null)
            {
                throw new BadRequestException("Only description can be updated for published or closed surveys.");
            }
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        return new PublicSurveyDto(survey, isAuthor: true);
    }
}
