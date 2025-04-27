using Domain.ValueObjects;
using Infrastructure.Services.Global;

namespace Infrastructure.Salaries;

public record ChartPageLink
{
    private readonly ISalariesChartQueryParams _requestOrNull;
    private readonly string _frontendBaseUrl;
    private readonly Dictionary<string, string> _additionalQueryParams;

    private string _result;

    public ChartPageLink(
        string baseUrl,
        ISalariesChartQueryParams requestOrNull)
    {
        _requestOrNull = requestOrNull;
        _frontendBaseUrl = baseUrl;
        _additionalQueryParams = new Dictionary<string, string>();
    }

    public ChartPageLink(
        IGlobal global,
        ISalariesChartQueryParams requestOrNull)
        : this(
            global.FrontendBaseUrl + "/salaries",
            requestOrNull)
    {
    }

    public ChartPageLink AddQueryParam(string key, string value)
    {
        _additionalQueryParams[key] = value;
        return this;
    }

    public override string ToString()
    {
        if (_result != null)
        {
            return _result;
        }

        if (_requestOrNull != null || _additionalQueryParams.Count > 0)
        {
            string queryParams = null;
            if (_requestOrNull?.ProfessionsToInclude.Count > 0)
            {
                queryParams += $"?profsInclude={string.Join(",", _requestOrNull.ProfessionsToInclude)}";
            }

            if (_requestOrNull?.Grade != null)
            {
                queryParams = queryParams != null ? queryParams + "&" : "?";
                queryParams += $"grade={(int)_requestOrNull.Grade.Value}";
            }

            if (_additionalQueryParams.Count > 0)
            {
                queryParams = queryParams != null ? queryParams + "&" : "?";
                queryParams += string.Join("&", _additionalQueryParams.Select(x => $"{x.Key}={x.Value}"));
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