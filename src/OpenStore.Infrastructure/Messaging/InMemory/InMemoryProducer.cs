using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Messaging.InMemory
{
    public class InMemoryProducer : IOpenStoreProducer
    {
        private readonly ChannelFactory _channelFactory;

        public InMemoryProducer(ChannelFactory channelFactory)
        {
            _channelFactory = channelFactory;
        }
        
        public Task Produce<TMessage>(string topic, string key, TMessage message, CancellationToken cancellationToken) 
            where TMessage : class => Produce(topic, message, cancellationToken);

        public async Task Produce<TMessage>(string topic, TMessage message, CancellationToken cancellationToken)
            where TMessage : class
        {
            if (!_channelFactory.Exists<TMessage>())
            {
                throw new Exception($"Consumer not exists for this message. {typeof(TMessage).FullName}");
            }
            
            var writer = _channelFactory.CreateOrGet<TMessage>().Writer;
                
            await writer.WriteAsync(message, cancellationToken);
        }

        public Task ProduceMany<TMessage>(string topic, string key, IEnumerable<TMessage> messages, CancellationToken cancellationToken)
            where TMessage : class => ProduceMany(topic, messages, cancellationToken);

        public async Task ProduceMany<TMessage>(string topic, IEnumerable<TMessage> messages, CancellationToken cancellationToken) where TMessage : class
        {
            var tasks = messages.Select(m => Produce(topic, m, cancellationToken));
            await Task.WhenAll(tasks);
        }
    }
}