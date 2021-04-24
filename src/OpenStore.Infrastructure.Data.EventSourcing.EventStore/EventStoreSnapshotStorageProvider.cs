using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using OpenStore.Domain.EventSourcing;

namespace OpenStore.Infrastructure.Data.EventSourcing.EventStore
{
    public class EventStoreSnapshotStorageProvider<TAggregate, TSnapshot> : EventStoreStorageProviderBase, ISnapshotStorageProvider<TAggregate, TSnapshot>
        where TAggregate : EventSourcedAggregateRoot 
        where TSnapshot : ISnapshot
    {
        private readonly IEventStoreStorageConnectionProvider _eventStoreStorageConnectionProvider;

        public EventStoreSnapshotStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer) : base(serializer)
        {
            _eventStoreStorageConnectionProvider = eventStoreStorageConnectionProvider;
        }

        private Task<IEventStoreConnection> GetEventStoreConnectionAsync() => _eventStoreStorageConnectionProvider.GetConnectionAsync();

        protected override string GetStreamNamePrefix() => _eventStoreStorageConnectionProvider.SnapshotStreamPrefix;

        public long SnapshotFrequency => _eventStoreStorageConnectionProvider.SnapshotFrequency;

        public async Task<TSnapshot> GetSnapshotAsync(object aggregateId)
        {
            TSnapshot snapshot = default;
            var connection = await GetEventStoreConnectionAsync();

            var streamEvents = await connection.ReadStreamEventsBackwardAsync($"{AggregateIdToStreamName(typeof(TAggregate), aggregateId.ToString())}", StreamPosition.End, 1, false);

            if (!streamEvents.Events.Any()) return default(TSnapshot);
            var result = streamEvents.Events.FirstOrDefault();
            snapshot = DeserializeSnapshotEvent<TSnapshot>(result);

            return snapshot;
        }

        public async Task SaveSnapshotAsync(TSnapshot snapshot)
        {
            var connection = await GetEventStoreConnectionAsync();
            var snapshotEvent = SerializeSnapshotEvent(snapshot, snapshot.Version);

            await connection.AppendToStreamAsync($"{AggregateIdToStreamName(typeof(TAggregate), snapshot.AggregateId.ToString())}", ExpectedVersion.Any, snapshotEvent);
        }
    }
}