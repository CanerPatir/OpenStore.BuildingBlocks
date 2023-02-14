using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace OpenStore.Data.OutBox;

public class OutBoxPollHost : IHostedService, IDisposable
{
    private readonly int _fetchSize;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private Timer _timer;
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly TimeSpan _oneMinuteInterval = TimeSpan.FromMinutes(1);

    public OutBoxPollHost(int fetchSize, IServiceScopeFactory serviceScopeFactory)
    {
        _fetchSize = fetchSize;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer
        (
            PushMessages,
            null,
            TimeSpan.Zero,
            _oneMinuteInterval
        );
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    private async void PushMessages(object state)
    {
        await _semaphore.WaitAsync();
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var outBox = scope.ServiceProvider.GetRequiredService<IOutBoxService>();
            await outBox.NotifyPendingMessages(_fetchSize);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _semaphore?.Dispose();
    }
}