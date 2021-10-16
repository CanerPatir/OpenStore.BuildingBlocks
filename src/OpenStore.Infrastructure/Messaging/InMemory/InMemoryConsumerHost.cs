using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace OpenStore.Infrastructure.Messaging.InMemory;

public class InMemoryConsumerHost<TMessage> : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ChannelReader<TMessage> _reader;
    private readonly ILogger<InMemoryConsumerHost<TMessage>> _logger;

    public InMemoryConsumerHost(
        ChannelReader<TMessage> reader, 
        IServiceScopeFactory serviceScopeFactory,
        AsyncRetryPolicy retryPolicy, 
        ILogger<InMemoryConsumerHost<TMessage>> logger)
    {
        _reader = reader;
        _serviceScopeFactory = serviceScopeFactory;
        _retryPolicy = retryPolicy;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (await _reader.WaitToReadAsync(stoppingToken))
        {
            while (_reader.TryRead(out var message))
            {
                await ExecuteInternal(message, stoppingToken);
            }
        }

        await _reader.Completion;
    }
        
    private async Task ExecuteInternal(TMessage message, CancellationToken stoppingToken)
    {
        _logger.LogInformation("InMem message processing");
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var openStoreConsumer = scope.ServiceProvider.GetRequiredService<IOpenStoreConsumer<TMessage>>();

            if (_retryPolicy != null)
            {
                await _retryPolicy
                    .ExecuteAsync(() => openStoreConsumer.Consume(message, stoppingToken));
            }
            else
            {
                await openStoreConsumer.Consume(message, stoppingToken);
            }

            _logger.LogInformation("InMem message processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InMem message processing error");
        }
    }

}