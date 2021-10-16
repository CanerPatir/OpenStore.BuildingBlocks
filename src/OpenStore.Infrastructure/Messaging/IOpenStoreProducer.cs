namespace OpenStore.Infrastructure.Messaging;

public interface IOpenStoreProducer
{
    Task Produce<TMessage>(string topic, string key, TMessage message, CancellationToken cancellationToken)
        where TMessage : class;
        
    Task Produce<TMessage>(string topic, TMessage message, CancellationToken cancellationToken)
        where TMessage : class;
        
    Task ProduceMany<TMessage>(string topic, string key, IEnumerable<TMessage> messages, CancellationToken cancellationToken)
        where TMessage : class;

    Task ProduceMany<TMessage>(string topic, IEnumerable<TMessage> messages, CancellationToken cancellationToken)
        where TMessage : class;
}