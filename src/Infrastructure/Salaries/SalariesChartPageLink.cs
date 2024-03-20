using Infrastructure.Services.Global;

namespace Infrastructure.Salaries;

public record SalariesChartPageLink
{
    private readonly ISalariesChartQueryParams _request;
    private readonly string _frontendBaseUrl;

    private string _result;

    public SalariesChartPageLink(
        IGlobal global,
        ISalariesChartQueryParams request)
    {
        _request = request;
        _frontendBaseUrl = global.FrontendBaseUrl + "/salaries";
    }

    public override string ToString()
    {
        if (_result != null)
        {
            return _result;
        }

        string queryParams = null;
        if (_request.ProfessionsToInclude.Count > 0)
        {
            queryParams += $"?profsInclude={string.Join(",", _request.ProfessionsToInclude)}";
        }

        if (_request.Grade.HasValue)
        {
            queryParams = queryParams != null ? queryParams + "&" : "?";
            queryParams += $"grade={(int)_request.Grade.Value}";
        }

        _result = _frontendBaseUrl + queryParams;
        return _result;
    }
}