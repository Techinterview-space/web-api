using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Surveys;
using Domain.Validation;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using Infrastructure.Services.Mediator;
using Web.Api.Features.PublicSurveys.Dtos;

namespace Web.Api.Features.PublicSurveys.CreatePublicSurvey;

public class CreatePublicSurveyHandler
    : IRequestHandler<CreatePublicSurveyRequest, PublicSurveyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public CreatePublicSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<PublicSurveyDto> Handle(
        CreatePublicSurveyRequest request,
        CancellationToken cancellationToken)
    {
        request.ThrowIfInvalid();

        var user = await _authorization.GetCurrentUserOrFailAsync(cancellationToken);

        var survey = new PublicSurvey(
            request.Title,
            request.Description,
            request.Slug,
            user.Id);

        foreach (var questionRequest in request.Questions)
        {
            var question = survey.AddQuestion(
                questionRequest.Text,
                questionRequest.Order,
                questionRequest.AllowMultipleChoices);

            for (int i = 0; i < questionRequest.Options.Count; i++)
            {
                question.AddOption(questionRequest.Options[i], i);
            }
        }

        await _context.PublicSurveys.AddAsync(survey, cancellationToken);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new PublicSurveyDto(survey, isAuthor: true);
    }
}
