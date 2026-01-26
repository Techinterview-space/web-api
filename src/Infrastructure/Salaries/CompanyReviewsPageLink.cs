using Infrastructure.Services.Global;

namespace Infrastructure.Salaries;

public class CompanyReviewsPageLink
{
    private readonly string _frontendBaseUrl;
    private readonly Dictionary<string, string> _additionalQueryParams;

    private string _result;

    public CompanyReviewsPageLink(
        IGlobal global)
    {
        _frontendBaseUrl = global.FrontendBaseUrl + "/companies/recent-reviews";
        _additionalQueryParams = new Dictionary<string, string>();
    }

    public CompanyReviewsPageLink AddQueryParam(
        string key, string value)
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

        if (_additionalQueryParams.Count > 0)
        {
            string queryParams = null;
            if (_additionalQueryParams.Count > 0)
            {
                queryParams = "?";
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