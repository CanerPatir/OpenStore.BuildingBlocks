using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Messaging
{
    public interface IOpenStoreConsumer<in TMessage>
    {
        public Task Consume(TMessage message, CancellationToken cancellationToken);
    }
}