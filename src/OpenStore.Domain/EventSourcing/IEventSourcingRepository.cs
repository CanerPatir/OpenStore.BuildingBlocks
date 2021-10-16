namespace OpenStore.Domain.EventSourcing;

public interface IEventSourcingRepository<TAggregate, TSnapshot> : IRepository<TAggregate>
    where TAggregate : EventSourcedAggregateRoot
    where TSnapshot : ISnapshot
{
}