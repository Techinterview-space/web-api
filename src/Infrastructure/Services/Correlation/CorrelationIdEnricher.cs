using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace Infrastructure.Services.Correlation;

public class CorrelationIdEnricher : ILogEventEnricher
{
    /// <summary>
    /// Property name.
    /// </summary>
    public const string PropertyName = "CorrelationId";

    private readonly IHttpContextAccessor _contextAccessor;
    private readonly bool _destructureObjects;

    public CorrelationIdEnricher(
        bool destructureObjects = false)
        : this(new HttpContextAccessor(), destructureObjects)
    {
    }

    public CorrelationIdEnricher(
        IHttpContextAccessor contextAccessor,
        bool destructureObjects = false)
    {
        _destructureObjects = destructureObjects;
        _contextAccessor = contextAccessor;
    }

    public void Enrich(
        LogEvent logEvent,
        ILogEventPropertyFactory propertyFactory)
    {
        var correlationId = new CorrelationIdAccessor(_contextAccessor).GetValue();
        if (string.IsNullOrEmpty(correlationId))
        {
            return;
        }

        var property = propertyFactory.CreateProperty(PropertyName, correlationId, _destructureObjects);
        logEvent.AddPropertyIfAbsent(property);
    }
}