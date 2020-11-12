using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenStore.Domain;

// ReSharper disable SuspiciousTypeConversion.Global

namespace OpenStore.Infrastructure.Data
{
    public interface IOutBoxService
    {
        Task StoreMessages(IEnumerable<IDomainEvent> events, CancellationToken cancellationToken = default);
        
        Task<bool> PushPendingMessages(int take, CancellationToken token = default);
    }
}