using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Polly.Retry;

namespace OpenStore.Infrastructure.Messaging.Kafka;

public class KafkaConsumerHost<TMessage> : BackgroundService
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IConsumer<Ignore, TMessage> _consumer;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly ILogger<KafkaConsumerHost<TMessage>> _logger;

    public KafkaConsumerHost(
        IServiceScopeFactory serviceScopeFactory,
        IConsumer<Ignore, TMessage> consumer,
        AsyncRetryPolicy retryPolicy,
        ILogger<KafkaConsumerHost<TMessage>> logger)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _consumer = consumer;
        _logger = logger;
        _retryPolicy = retryPolicy;
    }

    protected sealed override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (true)
        {
            try
            {
                var cr = _consumer.Consume(stoppingToken);
                if (cr.Message != null)
                {
                    await ExecuteInternal(cr, stoppingToken);
                    _logger.LogInformation($"kafka message consumed '{cr.Message.Value}' at: '{cr.TopicPartitionOffset}'.");
                }
                else
                {
                    await Task.Delay(100, stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _consumer.Close();
                break;
            }
            catch (ConsumeException e)
            {
                _logger.LogError(e, $"Kafka consumer error: {e.Error.Reason}");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Kafka unexpected error");
            }
        }
    }

    private async Task ExecuteInternal(ConsumeResult<Ignore, TMessage> cr, CancellationToken stoppingToken)
    {
        _logger.LogInformation("Kafka message processing");
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var openStoreConsumer = scope.ServiceProvider.GetRequiredService<IOpenStoreConsumer<TMessage>>();
                
            var message = cr.Message.Value;
            if (_retryPolicy != null)
            {
                await _retryPolicy
                    .ExecuteAsync(() => openStoreConsumer.Consume(message, stoppingToken));
            }
            else
            {
                await openStoreConsumer.Consume(message, stoppingToken);
            }

            try
            {
                _consumer.Commit(cr);
            }
            catch (Exception e)
            {
                _logger.LogInformation(e, "Kafka message commit error.");
            }

            _logger.LogInformation("Kafka message processed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Kafka message processing error");
        }
    }

    public override void Dispose()
    {
        base.Dispose();
        _consumer.Dispose();
    }
}