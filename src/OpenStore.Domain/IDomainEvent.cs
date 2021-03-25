using System;

namespace OpenStore.Domain
{
    public interface IDomainEvent
    {
        object Id { get; }
        ulong Version { get; }
        DateTime CommittedTimestamp { get; }
        string CorrelationId { get; }
    }
}