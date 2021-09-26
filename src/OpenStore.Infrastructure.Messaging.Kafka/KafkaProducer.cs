using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OpenStore.Infrastructure.Messaging.Kafka
{
    public class KafkaProducer : IOpenStoreProducer
    {
        private readonly IProducerFactory _producerFactory;
        private readonly ILogger<KafkaProducer> _logger;

        public KafkaProducer(
            IProducerFactory producerFactory,
            ILogger<KafkaProducer> logger)
        {
            _producerFactory = producerFactory;
            _logger = logger;
        }

        public async Task Produce<TMessage>(string topic, string key, TMessage message, CancellationToken cancellationToken) 
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithKey<TMessage>();

            try
            {
                var dr = await producer.ProduceAsync(topic, new Message<string, TMessage>
                {
                    Key = key,
                    Value = message,
                    Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                }, cancellationToken);

                _logger.LogInformation($"Delivered with key {key}, to '{dr.TopicPartitionOffset}' {{}}", dr);
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError(e, $"Delivery failed: {e.Error.Reason}");
                throw;
            }
        }

        public async Task Produce<TMessage>(string topic, TMessage message, CancellationToken cancellationToken) 
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithNoKey<TMessage>();

            try
            {
                var dr = await producer.ProduceAsync(topic, new Message<Null, TMessage>
                {
                    Value = message,
                    Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                }, cancellationToken);

                _logger.LogInformation($"Delivered without key to '{dr.TopicPartitionOffset}' {{}}", dr);
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError(e, $"Delivery failed: {e.Error.Reason}");
                throw;
            }
        }

        public Task ProduceMany<TMessage>(string topic, string key, IEnumerable<TMessage> messages, CancellationToken cancellationToken) 
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithKey<TMessage>();

            using (_logger.BeginScope($"Kafka batch product w/ key {Guid.NewGuid()}"))
            {
                void Handler(DeliveryReport<string, TMessage> r)
                {
                    if (r.Error.IsError)
                    {
                        _logger.LogInformation($"Bulk delivery item delivered with key {key} to '{r.TopicPartitionOffset}' {{}}", r);
                    }
                    else
                    {
                        _logger.LogError($"Batch delivery item failed: {r.Error.Reason}");
                    }
                }

                foreach (var message in messages)
                {
                    producer.Produce(topic, new Message<string, TMessage>()
                    {
                        Key = key,
                        Value = message,
                        Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                    }, Handler);
                }

                producer.Flush(cancellationToken);
            }

            return Task.CompletedTask;
        }

        public Task ProduceMany<TMessage>(string topic, IEnumerable<TMessage> messages, CancellationToken cancellationToken) 
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithNoKey<TMessage>();

            using (_logger.BeginScope($"Kafka batch product wo/ key {Guid.NewGuid()}"))
            {
                void Handler(DeliveryReport<Null, TMessage> r)
                {
                    if (r.Error.IsError)
                    {
                        _logger.LogInformation($"Bulk delivery item delivered without key to '{r.TopicPartitionOffset}' {{}}", r);
                    }
                    else
                    {
                        _logger.LogError($"Batch delivery item failed: {r.Error.Reason}");
                    }
                }

                foreach (var message in messages)
                {
                    producer.Produce(topic, new Message<Null, TMessage>()
                    {
                        Value = message,
                        Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                    }, Handler);
                }

                producer.Flush(cancellationToken);
            }

            return Task.CompletedTask;
        }
    }
}