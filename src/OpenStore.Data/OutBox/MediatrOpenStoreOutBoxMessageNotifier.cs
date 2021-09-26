using System.Threading.Tasks;
using MediatR;

namespace OpenStore.Data.OutBox
{
    public class MediatrOpenStoreOutBoxMessageNotifier : IOpenStoreOutBoxMessageNotifier
    {
        private readonly IMediator _mediator;

        public MediatrOpenStoreOutBoxMessageNotifier(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public Task Notify(OutBoxMessage outBoxMessage) => _mediator.Publish(outBoxMessage);
    }
}