namespace OpenStore.Domain.EventSourcing;

public interface ISnapshot
{
    object AggregateId { get; }
    long Version { get; }
}