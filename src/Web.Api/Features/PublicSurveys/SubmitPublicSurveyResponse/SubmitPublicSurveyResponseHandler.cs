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

namespace Web.Api.Features.PublicSurveys.SubmitPublicSurveyResponse;

public class SubmitPublicSurveyResponseHandler
    : IRequestHandler<SubmitPublicSurveyResponseCommand, Nothing>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public SubmitPublicSurveyResponseHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<Nothing> Handle(
        SubmitPublicSurveyResponseCommand request,
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
                s => s.Slug == request.Slug && s.DeletedAt == null,
                cancellationToken)
            ?? throw new NotFoundException("Survey not found.");

        if (!survey.CanAcceptResponses())
        {
            throw new BadRequestException("This survey is not accepting responses.");
        }

        if (!survey.Questions.Any())
        {
            throw new BadRequestException("Survey has no questions.");
        }

        // Validate no duplicate question answers
        var answeredQuestionIds = request.Body.Answers
            .Select(a => a.QuestionId)
            .ToHashSet();

        if (answeredQuestionIds.Count != request.Body.Answers.Count)
        {
            throw new BadRequestException("Duplicate question answers are not allowed.");
        }

        // Validate answers cover all questions
        var surveyQuestionIds = survey.Questions
            .Select(q => q.Id)
            .ToHashSet();

        if (!surveyQuestionIds.SetEquals(answeredQuestionIds))
        {
            throw new BadRequestException("You must answer all questions in the survey.");
        }

        // Check if user already responded to any question
        var alreadyResponded = survey.Questions
            .Any(q => q.Responses?.Any(r => r.UserId == user.Id) ?? false);

        if (alreadyResponded)
        {
            throw new BadRequestException("You have already responded to this survey.");
        }

        // Process each answer
        foreach (var answer in request.Body.Answers)
        {
            var question = survey.Questions.First(q => q.Id == answer.QuestionId);

            if (!question.AllowMultipleChoices && answer.OptionIds.Count > 1)
            {
                throw new BadRequestException(
                    $"Question \"{question.Text}\" allows only one choice.");
            }

            var selectedOptions = question.Options
                .Where(o => answer.OptionIds.Contains(o.Id))
                .ToList();

            if (selectedOptions.Count != answer.OptionIds.Count)
            {
                throw new BadRequestException("One or more selected options are invalid.");
            }

            var response = new PublicSurveyResponse(question, user.Id);

            foreach (var option in selectedOptions)
            {
                response.AddSelectedOption(option);
            }

            await _context.PublicSurveyResponses.AddAsync(response, cancellationToken);
        }

        await _context.TrySaveChangesAsync(cancellationToken);

        return Nothing.Value;
    }
}
