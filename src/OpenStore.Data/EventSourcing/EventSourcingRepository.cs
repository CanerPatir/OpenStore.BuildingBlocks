using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Application;
using OpenStore.Domain;
using OpenStore.Domain.EventSourcing;
using OpenStore.Domain.EventSourcing.Exception;

// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Data.EventSourcing;

public class EventSourcingRepository<TAggregate, TSnapshot> : IEventSourcingRepository<TAggregate, TSnapshot>
    where TAggregate : EventSourcedAggregateRoot, ISavingChanges, new()
    where TSnapshot : ISnapshot
{
    private readonly IEventStorageProvider<TAggregate> _eventStorageProvider;
    private readonly ISnapshotStorageProvider<TAggregate, TSnapshot> _snapshotStorageProvider;
    // private readonly IOpenStoreDomainEventNotifier _domainEventNotifier;

    public EventSourcingRepository(
        IEventStorageProvider<TAggregate> eventStorageProvider,
        // IOpenStoreDomainEventNotifier domainEventNotifier,
        ISnapshotStorageProvider<TAggregate, TSnapshot> snapshotStorageProvider)
    {
        _eventStorageProvider = eventStorageProvider;
        _snapshotStorageProvider = snapshotStorageProvider;
        // _domainEventNotifier = domainEventNotifier;
    }

    public async Task<TAggregate> GetAsync(object id, CancellationToken cancellationToken = default)
    {
        var item = default(TAggregate);
        var isSnapshottable = typeof(ISnapshottable<TSnapshot>).IsAssignableFrom(typeof(TAggregate));

        var snapshot = default(TSnapshot);

        if (isSnapshottable && _snapshotStorageProvider != null)
        {
            snapshot = await _snapshotStorageProvider.GetSnapshotAsync(id);
        }

        if (snapshot != null)
        {
            item = CreateNewInstance();

            if (!(item is ISnapshottable<TSnapshot> snapshottableItem))
            {
                throw new NullReferenceException(nameof(snapshottableItem));
            }

            item.HydrateFromSnapshot(snapshot);
            snapshottableItem.ApplySnapshot(snapshot);

            var events = await _eventStorageProvider.GetEventsAsync(id, snapshot.Version + 1, int.MaxValue);
            item.Load(events);
        }
        else
        {
            var events = (await _eventStorageProvider.GetEventsAsync(id, 0, int.MaxValue)).ToList();

            if (events.Any())
            {
                item = CreateNewInstance();
                item.Load(events);
            }
        }

        return item;
    }

    public async Task SaveAsync(TAggregate aggregateRoot, CancellationToken cancellationToken = default)
    {
        if (!aggregateRoot.HasUncommittedChanges())
        {
            return;
        }

        await CommitChanges(aggregateRoot);
    }

    public Task Delete(TAggregate aggregateRoot, CancellationToken token = default) => throw new NotSupportedException();

    private async Task CommitChanges(TAggregate aggregate)
    {
        var expectedVersion = aggregate.LastCommittedVersion;

        var item = await _eventStorageProvider.GetLastEventAsync(aggregate.Id);

        if (item != null && expectedVersion == (int) StreamState.NoStream)
        {
            throw new AggregateCreationException($"Aggregate {item.Id} can't be created as it already exists with version {item.Version + 1}");
        }
        else if (item != null && item.Version + 1 != expectedVersion)
        {
            throw new ConcurrencyException($"Aggregate {item.Id} has been modified externally and has an updated state. Can't commit changes.");
        }

        var changesToCommit = aggregate
            .GetUncommittedChanges()
            .ToList();

        //perform pre commit actions
        // foreach (var e in changesToCommit)
        // {
        //     e.CommittedTimestamp = DateTime.UtcNow;
        // }

        //CommitAsync events to storage provider
        await _eventStorageProvider.SaveAsync(aggregate);

        // //Publish to event publisher asynchronously
        // if (_domainEventNotifier != null)
        // {
        //     foreach (var e in changesToCommit)
        //     {
        //         await _domainEventNotifier.Notify(e);
        //     }
        // }

        //If the Aggregate implements snapshottable
        if (aggregate is ISnapshottable<TSnapshot> snapshottable &&
            _snapshotStorageProvider != null)
        {
            //Every N events we save a snapshot
            if (aggregate.Version >= _snapshotStorageProvider.SnapshotFrequency &&
                (
                    changesToCommit.Count >= _snapshotStorageProvider.SnapshotFrequency ||
                    aggregate.Version % _snapshotStorageProvider.SnapshotFrequency < changesToCommit.Count ||
                    aggregate.Version % _snapshotStorageProvider.SnapshotFrequency == 0
                )
            )
            {
                var snapshot = snapshottable.TakeSnapshot();
                await _snapshotStorageProvider.SaveSnapshotAsync(snapshot);
            }
        }

        aggregate.Commit();
    }

    private static TAggregate CreateNewInstance()
    {
        // todo: activate over reflection to avoid forcing strict empty constructor declaration
        return new TAggregate();
    }
}