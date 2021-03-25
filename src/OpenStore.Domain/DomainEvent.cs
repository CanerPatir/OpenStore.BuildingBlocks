using System;

namespace OpenStore.Domain
{
    public abstract record DomainEvent : IDomainEvent
    {
        protected DomainEvent(object id)
        {
            Id = id;
            CommittedTimestamp = DateTime.UtcNow;
            CorrelationId = Guid.NewGuid().ToString();
        }

        public object Id { get; }
        public ulong Version { get; set; }
        public DateTime CommittedTimestamp { get; set; }
        public string CorrelationId { get; set; }
    }
}