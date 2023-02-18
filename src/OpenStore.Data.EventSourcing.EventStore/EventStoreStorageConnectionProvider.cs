using EventStore.ClientAPI;

namespace OpenStore.Data.EventSourcing.EventStore;

public interface IEventStoreStorageConnectionProvider
{
    Task<IEventStoreConnection> GetConnectionAsync();
    string EventStreamPrefix { get; }
    string SnapshotStreamPrefix { get; }
    long SnapshotFrequency { get; }
    int PageSize { get; }
}

public class EventStoreStorageConnectionProvider : IEventStoreStorageConnectionProvider, IDisposable
{
    private IEventStoreConnection _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly Func<IEventStoreConnection> _eventStoreConnectionFactoryMethod;

    public EventStoreStorageConnectionProvider(Func<IEventStoreConnection> eventStoreConnectionFactory)
    {
        _eventStoreConnectionFactoryMethod = eventStoreConnectionFactory;
    }

    public async Task<IEventStoreConnection> GetConnectionAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (_connection != null) return _connection;
            _connection = _eventStoreConnectionFactoryMethod.Invoke();
            await _connection.ConnectAsync();

            return _connection;
        }
        finally
        {
            _lock.Release();
        }
    }

    public string EventStreamPrefix => "Event-";
    public string SnapshotStreamPrefix => "Snapshot-";
    public long SnapshotFrequency => 2;
    public int PageSize => 200;

    private void CloseConnection()
    {
        _lock.Wait();
        try
        {
            if (_connection == null) return;
            _connection.Close();
            _connection.Dispose();
            _connection = null;
        }
        finally
        {
            _lock.Release();
        }
    }

    public void Dispose()
    {
        CloseConnection();
    }
}