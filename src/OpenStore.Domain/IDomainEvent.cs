using System;

namespace OpenStore.Domain
{
    public interface IDomainEvent
    {
        object Id { get; }
        ulong Version { get; set; }
        DateTimeOffset CommittedTimestamp { get; set; }
        string CorrelationId { get; }
    }

    public abstract class DomainEvent : IDomainEvent
    {
        protected DomainEvent(object id)
        {
            Id = id;
            CommittedTimestamp = DateTimeOffset.Now;
            CorrelationId = Guid.NewGuid().ToString();
        }

        public object Id { get; }
        public ulong Version { get; set; }
        public DateTimeOffset CommittedTimestamp { get; set; }
        public string CorrelationId { get; set; }
    }
}