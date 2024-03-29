namespace OpenStore.Domain.EventSourcing;

public interface ISnapshotStorageProvider<TAggregate, TSnapshot>
    where TAggregate : EventSourcedAggregateRoot
    where TSnapshot : ISnapshot
{
    long SnapshotFrequency { get; }
    Task<TSnapshot> GetSnapshotAsync(object aggregateId);
    Task SaveSnapshotAsync(TSnapshot snapshot);
}