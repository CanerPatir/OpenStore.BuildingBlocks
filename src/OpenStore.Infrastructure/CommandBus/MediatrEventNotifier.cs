using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using OpenStore.Application;
using OpenStore.Domain;

namespace OpenStore.Infrastructure.CommandBus
{
    internal class MediatrEventNotifier : IEventNotifier
    {
        private readonly IMediator _mediator;

        public MediatrEventNotifier(IMediator mediator)
        {
            _mediator = mediator;
        }

        public Task Notify(IDomainEvent @event)
        {
            _mediator.Publish(@event);
            return Task.CompletedTask;
        }

        public async Task NotifyMany(IEnumerable<IDomainEvent> events) => await Task.WhenAll(events.Select(Notify));
    }
}