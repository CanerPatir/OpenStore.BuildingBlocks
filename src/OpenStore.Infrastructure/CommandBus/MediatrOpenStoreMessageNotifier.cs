using System.Threading.Tasks;
using MediatR;
using OpenStore.Application;

namespace OpenStore.Infrastructure.CommandBus
{
    public class MediatrOpenStoreMessageNotifier : IOpenStoreMessageNotifier
    {
        private readonly IMediator _mediator;

        public MediatrOpenStoreMessageNotifier(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public Task Notify(MessageEnvelop message) => _mediator.Publish(message);
    }
}