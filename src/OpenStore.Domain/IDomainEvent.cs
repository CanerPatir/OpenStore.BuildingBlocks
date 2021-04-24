using System;

namespace OpenStore.Domain
{
    public interface IDomainEvent
    {
        string Id { get; }
        long Version { get; set; }
        DateTime CommittedTimestamp { get; set; }
        string CorrelationId { get; }
    }
}