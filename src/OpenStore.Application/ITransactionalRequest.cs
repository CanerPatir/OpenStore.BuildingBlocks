using MediatR;

namespace OpenStore.Application;

public interface ITransactionalRequest : IRequest
{
}

public interface ITransactionalRequest<out TResult> : IRequest<TResult>
{
}