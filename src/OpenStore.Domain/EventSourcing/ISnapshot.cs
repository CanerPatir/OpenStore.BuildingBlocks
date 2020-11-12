namespace OpenStore.Domain.EventSourcing
{
    public interface ISnapshot
    {
        object AggregateId { get; }
        ulong Version { get; }
    }
}