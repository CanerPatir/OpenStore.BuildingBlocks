using OpenStore.Domain.EventSourcing;

namespace OpenStore.Data.EventSourcing.EventStore;

public interface ISession<TAggregate, TSnapshot> : IDisposable
    where TAggregate : EventSourcedAggregateRoot
    where TSnapshot : ISnapshot
{
    Task<TAggregate> LoadAsync(object id);
    void Attach(TAggregate aggregate);
    void Detach(TAggregate aggregate);
    Task SaveAsync();
    void DetachAll();
}