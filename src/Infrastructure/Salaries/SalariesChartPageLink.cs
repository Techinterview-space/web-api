using Domain.ValueObjects;
using Infrastructure.Services.Global;

namespace Infrastructure.Salaries;

public record SalariesChartPageLink
{
    private readonly SalariesChartQueryParamsBase _requestOrNull;
    private readonly string _frontendBaseUrl;
    private readonly Dictionary<string, string> _additionalQueryParams;

    private string _result;

    public SalariesChartPageLink(
        string baseUrl,
        SalariesChartQueryParamsBase requestOrNull)
    {
        _requestOrNull = requestOrNull;
        _frontendBaseUrl = baseUrl;
        _additionalQueryParams = new Dictionary<string, string>();
    }

    public SalariesChartPageLink(
        IGlobal global,
        SalariesChartQueryParamsBase requestOrNull)
        : this(
            global.FrontendBaseUrl + "/salaries",
            requestOrNull)
    {
    }

    public SalariesChartPageLink AddQueryParam(string key, string value)
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
            if (_requestOrNull?.SelectedProfessionIds.Count > 0)
            {
                queryParams += $"?profsInclude={string.Join(",", _requestOrNull.SelectedProfessionIds)}";
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