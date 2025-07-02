using Domain.Entities.OpenAI;

namespace Infrastructure.Ai;

public interface IAiProviderFactory
{
    IAiProvider GetProvider(
        AiEngine engine);
}