namespace Infrastructure.Services.Correlation;

public interface ICorrelationIdAccessor
{
    /// <summary>
    /// Returns correlation id value.
    /// </summary>
    /// <returns>Correlation id value.</returns>
    string GetValue();

    /// <summary>
    /// Puts correlation id to headers.
    /// </summary>
    /// <param name="client">Http client.</param>
    void PutCorrelationIdToHeaders(
        HttpClient client);
}