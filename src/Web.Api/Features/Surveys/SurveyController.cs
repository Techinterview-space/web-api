using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TechInterviewer.Features.Surveys.GetSalariesSurveyQuestion;
using TechInterviewer.Setup.Attributes;

namespace TechInterviewer.Features.Surveys;

[HasAnyRole]
[ApiController]
[Route("api/survey")]
public class SurveyController
{
    private readonly IMediator _mediator;

    public SurveyController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("salaries-survey-question")]
    public async Task<GetSalariesSurveyQuestionResponse> GetSalariesSurveyQuestion(
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetSalariesSurveyQuestionQuery(),
            cancellationToken);

        return result;
    }
}