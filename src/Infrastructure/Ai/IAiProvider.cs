namespace Infrastructure.Ai;

public interface IAiProvider
{
    Task<AiChatResult> AnalyzeChatAsync(
        string input,
        string systemPrompt,
        string model = null,
        string correlationId = null,
        CancellationToken cancellationToken = default);
}