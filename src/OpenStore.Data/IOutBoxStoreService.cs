using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Domain;

namespace OpenStore.Data
{
    public interface IOutBoxStoreService
    {
        Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
    }
}