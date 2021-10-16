using MediatR;

namespace OpenStore.Application;

public interface INotifySuccessRequest : IRequest
{
}
    
public interface INotifySuccessRequest<out TResult> : IRequest<TResult>
{
}