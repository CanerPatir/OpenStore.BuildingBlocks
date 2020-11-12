using MediatR;

namespace OpenStore.Domain
{
    public interface ICommand : IRequest
    {
    }
    
    public interface ICommand<out T> : IRequest<T>
    {
    }
}