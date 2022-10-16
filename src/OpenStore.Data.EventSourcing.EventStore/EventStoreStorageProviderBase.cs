using EventStore.ClientAPI;
using OpenStore.Domain;
using OpenStore.Domain.EventSourcing;

namespace OpenStore.Data.EventSourcing.EventStore;

public abstract class EventStoreStorageProviderBase
{
    private readonly ISerializer _serializer;

    protected EventStoreStorageProviderBase(ISerializer serializer)
    {
        _serializer = serializer;
    }

    protected abstract string GetStreamNamePrefix();

    protected string AggregateIdToStreamName(Type t, string id)
    {
        //Ensure first character of type name is in lower case

        var prefix = GetStreamNamePrefix();
        return $"{char.ToLower(prefix[0])}{prefix.Substring(1)}{t.Name}{id}";
    }

    protected IDomainEvent DeserializeEvent(ResolvedEvent returnedEvent)
    {
        var header = _serializer.Deserialize<EventStoreMetaDataHeader>(returnedEvent.Event.Metadata);

        var returnType = Type.GetType(header.ClrType);

        return (IDomainEvent)_serializer.Deserialize(returnedEvent.Event.Data, returnType);
    }

    protected EventData SerializeEvent(IDomainEvent @event, long commitNumber)
    {
        var header = new EventStoreMetaDataHeader()
        {
            ClrType = GetClrTypeName(@event),
            CommitNumber = Convert.ToInt64(commitNumber) // todo: danger
        };

        return new EventData(Guid.NewGuid(), @event.GetType().Name, true, _serializer.Serialize(@event), _serializer.Serialize(header));
    }

    protected TSnapshot DeserializeSnapshotEvent<TSnapshot>(ResolvedEvent returnedEvent)
    {
        var header = _serializer.Deserialize<EventStoreMetaDataHeader>(returnedEvent.Event.Metadata);

        var returnType = Type.GetType(header.ClrType);

        return (TSnapshot)_serializer.Deserialize(returnedEvent.Event.Data, returnType);
    }

    protected EventData SerializeSnapshotEvent<TSnapshot>(TSnapshot snapshot, long commitNumber)
        where TSnapshot : ISnapshot
    {
        var header = new EventStoreMetaDataHeader()
        {
            ClrType = GetClrTypeName(snapshot),
            CommitNumber = Convert.ToInt64(commitNumber) // todo: danger
        };

        return new EventData(Guid.NewGuid(), snapshot.GetType().Name, true, _serializer.Serialize(snapshot), _serializer.Serialize(header));
    }

    private string GetClrTypeName(object @event)
    {
        return @event.GetType() + "," + @event.GetType().Assembly.GetName().Name;
    }
}