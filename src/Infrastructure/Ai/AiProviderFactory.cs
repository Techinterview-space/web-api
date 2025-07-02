using Domain.Entities.OpenAI;
using Infrastructure.Ai.ChatGpt;
using Infrastructure.Ai.Claude;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Ai;

public class AiProviderFactory : IAiProviderFactory
{
    private readonly IServiceScope _serviceScope;

    public AiProviderFactory(
        IServiceScope serviceScope)
    {
        _serviceScope = serviceScope;
    }

    public IAiProvider GetProvider(
        AiEngine engine)
    {
        return engine switch
        {
            AiEngine.OpenAi => _serviceScope.ServiceProvider.GetRequiredService<ChatGptProvider>(),
            AiEngine.Claude => _serviceScope.ServiceProvider.GetRequiredService<ClaudeAiProvider>(),
            _ => throw new NotSupportedException($"AI engine '{engine}' is not supported.")
        };
    }
}