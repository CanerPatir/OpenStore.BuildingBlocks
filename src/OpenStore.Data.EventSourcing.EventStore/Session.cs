using OpenStore.Domain.EventSourcing;

namespace OpenStore.Data.EventSourcing.EventStore;

public class Session<TAggregate, TSnapshot> : ISession<TAggregate, TSnapshot>
    where TAggregate : EventSourcedAggregateRoot
    where TSnapshot : ISnapshot
{
    private readonly IEventSourcingRepository<TAggregate, TSnapshot> _repository;
    private readonly IList<TAggregate> _trackedItems = new List<TAggregate>();
    private readonly SemaphoreSlim _syncLock = new(1, 1);

    public Session(IEventSourcingRepository<TAggregate, TSnapshot> repository)
    {
        _repository = repository;
    }

    public async Task<TAggregate> LoadAsync(object id)
    {
        await _syncLock.WaitAsync();

        try
        {
            var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(id));
            if (item == null)
            {
                item = await _repository.GetAsync(id);
                _trackedItems.Add(item);
            }

            return item;
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public void Attach(TAggregate aggregate)
    {
        _syncLock.Wait();

        try
        {
            var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(aggregate.Id));

            if (item == null)
            {
                _trackedItems.Add(aggregate);
            }
            else
            {
                throw new ArgumentException("Item with the same id is already tracked", nameof(aggregate));
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public void Detach(TAggregate aggregate)
    {
        _syncLock.Wait();

        try
        {
            var item = _trackedItems.FirstOrDefault(a => a.Id.Equals(aggregate.Id));

            if (item != null)
            {
                _trackedItems.Remove(aggregate);
            }
            else
            {
                throw new ArgumentException("Item with the same id is not tracked", nameof(aggregate));
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public async Task SaveAsync()
    {
        await _syncLock.WaitAsync();

        try
        {
            foreach (var trackedItem in _trackedItems)
            {
                await _repository.SaveAsync(trackedItem);
            }
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public void DetachAll()
    {
        _syncLock.Wait();

        try
        {
            _trackedItems.Clear();
        }
        finally
        {
            _syncLock.Release();
        }
    }

    public void Dispose()
    {
        _trackedItems.Clear();
    }
}