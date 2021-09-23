using MediatR;

namespace OpenStore.Domain
{
    public interface IDomainEvent : INotification
    {
        string Id { get; }
        long Version { get; set; }
    }
}