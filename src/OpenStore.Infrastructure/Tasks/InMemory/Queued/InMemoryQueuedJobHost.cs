using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Tasks.InMemory.Queued;

public class InMemoryQueuedJobHost : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<InMemoryQueuedJobHost> _logger;
    private readonly ChannelReader<Func<IServiceProvider, CancellationToken, Task>> _reader;

    public InMemoryQueuedJobHost(
        IServiceProvider serviceProvider,
        ChannelReader<Func<IServiceProvider, CancellationToken, Task>> reader)
    {
        _serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        _logger = serviceProvider.GetRequiredService<ILogger<InMemoryQueuedJobHost>>();
        _reader = reader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _reader.WaitToReadAsync(stoppingToken))
        {
            while (_reader.TryRead(out var workItem))
            {
                await ExecuteInternal(workItem, stoppingToken);
            }
        }
    }

    private async Task ExecuteInternal(Func<IServiceProvider, CancellationToken, Task> workItem, CancellationToken cancellation)
    {
        try
        {
            _logger.LogInformation($"Starting to proceed queued job: {nameof(workItem)}.");

            await TaskHelper.RunBgLong(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                await workItem(scope.ServiceProvider, cancellation);
                _logger.LogInformation($"Queued job done {nameof(workItem)}.");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Demystify(), $"Error occurred executing queued job: {nameof(workItem)}.");
        }
    }
}