using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.CommandBus
{
    public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IUnitOfWork _uow;

        public TransactionBehavior(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            if (!(request is ITransactionalRequest) && !(request is ITransactionalRequest<TResponse>)) return await next();
            
            await _uow.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await next();
                await _uow.SaveChangesAsync(cancellationToken);
                return result;
            }
            catch
            {
                //  await _uow.RollBackAsync(cancellationToken);
                throw;
            }
        }
    }
}