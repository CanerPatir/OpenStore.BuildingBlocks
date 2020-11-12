using System.Collections.Generic;
using System.Threading.Tasks;
using OpenStore.Domain;

namespace OpenStore.Application
{
    public interface IEventNotifier
    {
        Task Notify(IDomainEvent @event);
        Task NotifyMany(IEnumerable<IDomainEvent> events);
    }
}