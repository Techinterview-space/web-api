using Domain.Entities.OpenAI;
using Infrastructure.Ai.ChatGpt;
using Infrastructure.Ai.Claude;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ai;

public class AiProviderFactory : IAiProviderFactory
{
    private readonly IServiceProvider _serviceScope;

    public AiProviderFactory(
        IServiceProvider serviceScope)
    {
        _serviceScope = serviceScope;
    }

    public IAiProvider GetProvider(
        AiEngine engine)
    {
        return engine switch
        {
            AiEngine.OpenAi => _serviceScope.GetRequiredService<ChatGptProvider>(),
            AiEngine.Claude => _serviceScope.GetRequiredService<ClaudeAiProvider>(),
            _ => throw new NotSupportedException($"AI engine '{engine}' is not supported.")
        };
    }
}