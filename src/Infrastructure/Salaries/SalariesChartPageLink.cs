using Infrastructure.Services.Global;

namespace Infrastructure.Salaries;

public record SalariesChartPageLink
{
    private readonly ISalariesChartQueryParams _requestOrNull;
    private readonly string _frontendBaseUrl;

    private string _result;

    public SalariesChartPageLink(
        IGlobal global,
        ISalariesChartQueryParams requestOrNull)
    {
        _requestOrNull = requestOrNull;
        _frontendBaseUrl = global.FrontendBaseUrl + "/salaries";
    }

    public override string ToString()
    {
        if (_result != null)
        {
            return _result;
        }

        if (_requestOrNull != null)
        {
            string queryParams = null;
            if (_requestOrNull.ProfessionsToInclude.Count > 0)
            {
                queryParams += $"?profsInclude={string.Join(",", _requestOrNull.ProfessionsToInclude)}";
            }

            if (_requestOrNull.Grade.HasValue)
            {
                queryParams = queryParams != null ? queryParams + "&" : "?";
                queryParams += $"grade={(int)_requestOrNull.Grade.Value}";
            }

            _result = _frontendBaseUrl + queryParams;
        }
        else
        {
            _result = _frontendBaseUrl;
        }

        return _result;
    }
}