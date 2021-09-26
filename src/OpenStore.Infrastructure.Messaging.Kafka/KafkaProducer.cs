using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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

        public async Task ProduceMany<TMessage>(string topic, string key, IEnumerable<TMessage> messages, CancellationToken cancellationToken)
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithKey<TMessage>();

            using (_logger.BeginScope($"Kafka batch product w/ key {Guid.NewGuid()}"))
            {
                foreach (var message in messages)
                {
                    var deliveryResult = await producer.ProduceAsync(topic, new Message<string, TMessage>()
                    {
                        Key = key,
                        Value = message,
                        Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                    }, cancellationToken);

                    if (deliveryResult.Status == PersistenceStatus.NotPersisted)
                    {
                        throw new Exception($"Kafka message not persisted {message}");
                    }
                }
            }
        }

        public async Task ProduceMany<TMessage>(string topic, IEnumerable<TMessage> messages, CancellationToken cancellationToken)
            where TMessage : class
        {
            var producer = _producerFactory.CreateWithNoKey<TMessage>();

            using (_logger.BeginScope($"Kafka batch product wo/ key {Guid.NewGuid()}"))
            {
                foreach (var message in messages)
                {
                    var deliveryResult = await producer.ProduceAsync(topic, new Message<Null, TMessage>()
                    {
                        Value = message,
                        Timestamp = new Timestamp(DateTime.UtcNow, TimestampType.LogAppendTime)
                    }, cancellationToken);

                    if (deliveryResult.Status == PersistenceStatus.NotPersisted)
                    {
                        throw new Exception($"Kafka message not persisted {message}");
                    }
                }

                producer.Flush(cancellationToken);
            }
        }
    }
}