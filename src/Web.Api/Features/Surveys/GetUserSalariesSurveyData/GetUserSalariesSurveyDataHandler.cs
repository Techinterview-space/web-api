using System.Threading;
using System.Threading.Tasks;
using Infrastructure.Authentication.Contracts;
using Infrastructure.Database;
using MediatR;
using Web.Api.Features.Surveys.Services;

namespace Web.Api.Features.Surveys.GetUserSalariesSurveyData;

public class GetUserSalariesSurveyDataHandler : IRequestHandler<GetUserSalariesSurveyDataQuery, GetUserSalariesSurveyDataResponse>
{
    private readonly IAuthorization _auth;
    private readonly DatabaseContext _context;

    public GetUserSalariesSurveyDataHandler(
        IAuthorization auth,
        DatabaseContext context)
    {
        _auth = auth;
        _context = context;
    }

    public async Task<GetUserSalariesSurveyDataResponse> Handle(
        GetUserSalariesSurveyDataQuery request,
        CancellationToken cancellationToken)
    {
        var currentUser = await _auth.CurrentUserOrNullAsync(cancellationToken);
        if (currentUser is null)
        {
            return new GetUserSalariesSurveyDataResponse(null);
        }

        var lastSurvey = await new SalariesSurveyUserService(_context).GetLastSurveyOrNullAsync(currentUser, cancellationToken);

        return new GetUserSalariesSurveyDataResponse(lastSurvey?.CreatedAt);
    }
}