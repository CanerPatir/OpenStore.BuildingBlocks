using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.Data
{
    public abstract class OutBoxStoreService : IOutBoxStoreService
    {
        protected static IEnumerable<OutBoxMessage> WrapEvents(IEnumerable<IDomainEvent> events)
        {
            var correlationId = Guid.NewGuid().ToString();
            return events.Select(e => new OutBoxMessage(e, correlationId));
        }

        public abstract Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}