using System;
using System.Collections.Generic;
using System.Threading.Channels;

namespace OpenStore.Infrastructure.Messaging.InMemory
{
    public class ChannelFactory : IDisposable
    {
        private readonly IDictionary<Type, object> _channels = new Dictionary<Type, object>();
        private const int BoundedCapacity = 1000; // Creates back pressure to block producer when message count reached capacity 
        private readonly List<Action> _disposeActions = new List<Action>();

        public Channel<TMessage> CreateOrGet<TMessage>()
        {
            var messageType = typeof(TMessage);
            if (_channels.TryGetValue(messageType, out var channel))
            {
                return (Channel<TMessage>) channel;
            }

            var createdChannel = Channel.CreateBounded<TMessage>(new BoundedChannelOptions(BoundedCapacity)
            {
                SingleWriter = false,
                SingleReader = true
            });

            _disposeActions.Add(() => createdChannel.Writer.Complete());
            _channels[messageType] = createdChannel;

            return createdChannel;
        }

        internal bool Exists<TMessage>() => _channels.ContainsKey(typeof(TMessage));

        public void Dispose()
        {
            foreach (var disposeAction in _disposeActions)
            {
                disposeAction.Invoke();
            }
        }
    }
}