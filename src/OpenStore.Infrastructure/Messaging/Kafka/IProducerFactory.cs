using Confluent.Kafka;

namespace OpenStore.Infrastructure.Messaging.Kafka
{
    public interface IProducerFactory
    {
        public IProducer<string, TMessage> CreateWithKey<TMessage>()
            where TMessage : class;
        
        public IProducer<Null, TMessage> CreateWithNoKey<TMessage>()       
            where TMessage : class;
    }
}