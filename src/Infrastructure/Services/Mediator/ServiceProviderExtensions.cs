using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Services.Mediator;

public static class ServiceProviderExtensions
{
    public static async Task<TResult> HandleBy<THandler, TRequest, TResult>(
        this IServiceProvider serviceProvider,
        TRequest request,
        CancellationToken cancellationToken = default)
        where THandler : Infrastructure.Services.Mediator.IRequestHandler<TRequest, TResult>
    {
        var handler = serviceProvider.GetRequiredService<THandler>();
        return await handler.Handle(request, cancellationToken);
    }
}