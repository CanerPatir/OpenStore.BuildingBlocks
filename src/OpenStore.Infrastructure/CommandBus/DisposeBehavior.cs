using MediatR;

namespace OpenStore.Infrastructure.CommandBus;

public class DisposeBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (!(request is IDisposable disposableRequest)) return await next();

        try
        {
            return await next();
        }
        finally
        {
            disposableRequest.Dispose();
        }
    }
}