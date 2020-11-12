using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenStore.Domain.EventSourcing
{
    public interface IEventStorageProvider<in TAggregate> 
        where TAggregate: EventSourcedAggregateRoot
    {
        Task<IEnumerable<IDomainEvent>> GetEventsAsync(object aggregateId, ulong start, int count);
        Task<IDomainEvent> GetLastEventAsync(object aggregateId);
        Task SaveAsync(TAggregate aggregate);
    }
}