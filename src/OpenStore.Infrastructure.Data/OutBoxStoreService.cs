using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data
{
    public abstract class OutBoxStoreService : IOutBoxStoreService
    {
        protected static IEnumerable<OutBoxMessage> WrapEvents(IEnumerable<IDomainEvent> events) => events.Select(e => new OutBoxMessage(e));

        public abstract Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}