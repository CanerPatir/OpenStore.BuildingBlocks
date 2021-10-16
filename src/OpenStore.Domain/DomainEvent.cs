using System;

namespace OpenStore.Domain;

public abstract record DomainEvent : IDomainEvent
{
    protected DomainEvent(string id)
    {
        Id = id;
    }

    public string Id { get; }
    public long Version { get; set; } 
}