using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EventStore.ClientAPI;
using OpenStore.Domain;
using OpenStore.Domain.EventSourcing;

namespace OpenStore.Infrastructure.Data.EventSourcing.EventStore
{
    public class EventStoreEventStorageProvider<TAggregate> : EventStoreStorageProviderBase, IEventStorageProvider<TAggregate>
        where TAggregate : EventSourcedAggregateRoot
    {
        private readonly IEventStoreStorageConnectionProvider _eventStoreStorageConnectionProvider;

        public EventStoreEventStorageProvider(IEventStoreStorageConnectionProvider eventStoreStorageConnectionProvider, ISerializer serializer) : base(serializer)
        {
            _eventStoreStorageConnectionProvider = eventStoreStorageConnectionProvider;
        }

        private Task<IEventStoreConnection> GetEventStoreConnectionAsync() => _eventStoreStorageConnectionProvider.GetConnectionAsync();

        protected override string GetStreamNamePrefix() => _eventStoreStorageConnectionProvider.EventStreamPrefix;

        public async Task<IEnumerable<IDomainEvent>> GetEventsAsync(object aggregateId, ulong start, int count)
        {
            var connection = await GetEventStoreConnectionAsync();
            var events = await ReadEvents(typeof(TAggregate), connection, aggregateId, start, count);

            return events;
        }

        public async Task<IDomainEvent> GetLastEventAsync(object aggregateId)
        {
            var connection = await GetEventStoreConnectionAsync();
            var results = await connection.ReadStreamEventsBackwardAsync($"{AggregateIdToStreamName(typeof(TAggregate), aggregateId.ToString())}", StreamPosition.End, 1, false);

            if (results.Status == SliceReadStatus.Success && results.Events.Any())
            {
                return DeserializeEvent(results.Events.First());
            }

            return null;
        }

        public async Task SaveAsync(TAggregate aggregate)
        {
            var connection = await GetEventStoreConnectionAsync();
            var events = aggregate.GetUncommittedChanges();

            if (events.Any())
            {
                var lastVersion = aggregate.LastCommittedVersion;
                var lstEventData = events.Select(@event => SerializeEvent(@event, aggregate.LastCommittedVersion + 1)).ToList();

                await connection.AppendToStreamAsync($"{AggregateIdToStreamName(aggregate.GetType(), aggregate.Id)}",
                    (lastVersion < (ulong)StreamState.HasStream ? (long)ExpectedVersion.NoStream : (long)lastVersion), lstEventData);
            }
        }

        private async Task<IEnumerable<IDomainEvent>> ReadEvents(Type aggregateType, IEventStoreConnection connection, object aggregateId, ulong start, int count)
        {
            var streamEvents = new List<ResolvedEvent>();
            StreamEventsSlice currentSlice;
            long nextSliceStart = start == 0 ? StreamPosition.Start : (long)start;

            //Read the stream using pagesize which was set before.
            //We only need to read the full page ahead if expected results are larger than the page size
            do
            {
                var nextReadCount = count - streamEvents.Count;

                if (nextReadCount > _eventStoreStorageConnectionProvider.PageSize)
                {
                    nextReadCount = _eventStoreStorageConnectionProvider.PageSize;
                }

                currentSlice = await connection.ReadStreamEventsForwardAsync($"{AggregateIdToStreamName(aggregateType, aggregateId.ToString())}", nextSliceStart, nextReadCount, false);

                nextSliceStart = currentSlice.NextEventNumber;

                streamEvents.AddRange(currentSlice.Events);
            } while (!currentSlice.IsEndOfStream);

            //Deserialize and add to events list

            return streamEvents.Select(DeserializeEvent).ToList();
        }
    }
}