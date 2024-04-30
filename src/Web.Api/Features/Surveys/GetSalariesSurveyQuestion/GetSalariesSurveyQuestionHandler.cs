using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Validation.Exceptions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;

public class GetSalariesSurveyQuestionHandler
    : IRequestHandler<GetSalariesSurveyQuestionQuery, GetSalariesSurveyQuestionResponse>
{
    public const int RecentRepliesDays = 92;

    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public GetSalariesSurveyQuestionHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<GetSalariesSurveyQuestionResponse> Handle(
        GetSalariesSurveyQuestionQuery request,
        CancellationToken cancellationToken)
    {
        var salariesSurveyQuestion = await _context.SalariesSurveyQuestions
            .Select(x => new SalariesSurveyQuestionDto
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (salariesSurveyQuestion == null)
        {
            throw new NotFoundException("Salaries survey question not found");
        }

        var requiresReply = false;
        var currentUser = await _authorization.CurrentUserOrNullAsync(cancellationToken);
        if (currentUser is not null)
        {
            var createdAtEdge = DateTime.UtcNow.AddDays(-92);
            var hasReplyByCurrentUser = await _context.SalariesSurveyReplies
                .Where(x =>
                    x.SalariesSurveyQuestionId == salariesSurveyQuestion.Id &&
                    x.CreatedByUserId == currentUser.Id)
                .Where(x => x.CreatedAt >= createdAtEdge)
                .AnyAsync(cancellationToken);

            requiresReply = !hasReplyByCurrentUser;
        }

        return new GetSalariesSurveyQuestionResponse(salariesSurveyQuestion, requiresReply);
    }
}