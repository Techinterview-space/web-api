using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Correlation;

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    /// <summary>
    /// Header name for correlation id.
    /// </summary>
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly HttpContext _httpContextOrNull;

    private string _value;

    public CorrelationIdAccessor(
        IHttpContextAccessor contextAccessor)
        : this(contextAccessor.HttpContext)
    {
    }

    public CorrelationIdAccessor(
        HttpContext httpContext)
    {
        _httpContextOrNull = httpContext;
    }

    public string GetValue()
    {
        if (_httpContextOrNull == null)
        {
            _value ??= Guid.NewGuid().ToString();
            return _value;
        }

        if (_httpContextOrNull.Items.TryGetValue(CorrelationIdHeaderName, out var valueObject))
        {
            return valueObject?.ToString();
        }

        _value ??= Guid.NewGuid().ToString();
        _httpContextOrNull.Items[CorrelationIdHeaderName] = _value;

        return _value;
    }

    public void PutCorrelationIdToHeaders(
        HttpClient client)
    {
        if (client == null)
        {
            throw new System.ArgumentNullException(nameof(client), "Please provide Http client");
        }

        var correlationId = GetValue();
        if (string.IsNullOrEmpty(correlationId) ||
            client.DefaultRequestHeaders.Contains(CorrelationIdHeaderName))
        {
            return;
        }

        client.DefaultRequestHeaders.Add(CorrelationIdHeaderName, correlationId);
    }
}