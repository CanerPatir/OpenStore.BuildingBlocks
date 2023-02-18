using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.CommandBus;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<TransactionBehavior<TRequest, TResponse>> _logger;

    public TransactionBehavior(IServiceProvider serviceProvider,
        ILogger<TransactionBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request is not ITransactionalRequest && request is not ITransactionalRequest<TResponse>) return await next();

        var uow = _serviceProvider.GetService<IUnitOfWork>();

        if (uow == null)
        {
            _logger.LogWarning("Unit of work not found in transactional behavior");
            return await next();
        }

        await uow.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await next();
            await uow.SaveChangesAsync(cancellationToken);
            return result;
        }
        catch
        {
            //  await _uow.RollBackAsync(cancellationToken);
            throw;
        }
    }
}