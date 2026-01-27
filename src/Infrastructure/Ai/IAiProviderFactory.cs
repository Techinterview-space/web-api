using Domain.Entities.Prompts;

namespace Infrastructure.Ai;

public interface IAiProviderFactory
{
    IAiProvider GetProvider(
        AiEngine engine);
}