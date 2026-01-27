namespace Infrastructure.Services.Mediator;

public interface IRequestHandler<TRequest, TResult>
{
    Task<TResult> Handle(
        TRequest request,
        CancellationToken cancellationToken);
}