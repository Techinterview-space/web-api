using System.Threading;
using System.Threading.Tasks;
using Domain.Entities.Questions;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using TechInterviewer.Features.Surveys.Dtos;

namespace TechInterviewer.Features.Surveys.ReplyOnSalariesSurvey;

public class ReplyOnSalariesSurveyHandler
    : IRequestHandler<ReplyOnSalariesSurveyCommand, SalariesSurveyReplyDto>
{
    private readonly DatabaseContext _context;
    private readonly IAuthorization _authorization;

    public ReplyOnSalariesSurveyHandler(
        DatabaseContext context,
        IAuthorization authorization)
    {
        _context = context;
        _authorization = authorization;
    }

    public async Task<SalariesSurveyReplyDto> Handle(
        ReplyOnSalariesSurveyCommand request,
        CancellationToken cancellationToken)
    {
        request.IsValidOrFail();

        var question = await _context.SalariesSurveyQuestions
            .ByIdOrFailAsync(request.SalariesSurveyQuestionId, cancellationToken);

        var currentUser = await _authorization.CurrentUserOrFailAsync(cancellationToken);
        var reply = new SalariesSurveyReply(
            request.ReplyType,
            question,
            currentUser);

        _context.Add(reply);
        await _context.TrySaveChangesAsync(cancellationToken);

        return new SalariesSurveyReplyDto(reply);
    }
}