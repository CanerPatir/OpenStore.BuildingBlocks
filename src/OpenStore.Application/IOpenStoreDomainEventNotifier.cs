using System.Collections.Generic;
using System.Threading.Tasks;
using OpenStore.Domain;

namespace OpenStore.Application
{
    public interface IOpenStoreDomainEventNotifier
    {
        Task Notify(IDomainEvent @event);
        Task NotifyMany(IEnumerable<IDomainEvent> events);
    }
}