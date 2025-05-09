using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Correlation;

public class CorrelationIdAccessor : ICorrelationIdAccessor
{
    /// <summary>
    /// Header name for correlation id.
    /// </summary>
    public const string CorrelationIdHeaderName = "X-Correlation-Id";

    private readonly IHttpContextAccessor _contextAccessor;

    public CorrelationIdAccessor(
        IHttpContextAccessor contextAccessor)
    {
        _contextAccessor = contextAccessor;
    }

    public string GetValue()
    {
        return _contextAccessor.HttpContext?.Items.TryGetValue(CorrelationIdHeaderName, out var value) == true
            ? value?.ToString()
            : null;
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