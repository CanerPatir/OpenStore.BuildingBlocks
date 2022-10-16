using Confluent.Kafka;

namespace OpenStore.Infrastructure.Messaging.Kafka;

public class ProducerFactory : IProducerFactory, IDisposable
{
    private readonly ProducerConfig _producerConfig;

    public ProducerFactory(ProducerConfig producerConfig)
    {
        _producerConfig = producerConfig;
    }

    private readonly IDictionary<Type, IDisposable> _producers = new Dictionary<Type, IDisposable>();

    public IProducer<string, TMessage> CreateWithKey<TMessage>()
        where TMessage : class
    {
        var messageType = typeof(TMessage);
        if (_producers.TryGetValue(messageType, out var val))
        {
            return (IProducer<string, TMessage>)val;
        }

        var producer = new ProducerBuilder<string, TMessage>(_producerConfig)
            .SetKeySerializer(Serializers.Utf8)
            .SetValueSerializer(new DefaultMessageSerializer<TMessage>())
            .Build();

        _producers[messageType] = producer;
        return producer;
    }

    public IProducer<Null, TMessage> CreateWithNoKey<TMessage>()
        where TMessage : class
    {
        var messageType = typeof(TMessage);
        if (_producers.TryGetValue(messageType, out var val))
        {
            return (IProducer<Null, TMessage>)val;
        }

        var producer = new ProducerBuilder<Null, TMessage>(_producerConfig)
            .SetKeySerializer(Serializers.Null)
            .SetValueSerializer(new DefaultMessageSerializer<TMessage>())
            .Build();

        _producers[messageType] = producer;
        return producer;
    }

    public void Dispose()
    {
        foreach (var disposable in _producers.Values)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // ignored
            }
        }
    }
}