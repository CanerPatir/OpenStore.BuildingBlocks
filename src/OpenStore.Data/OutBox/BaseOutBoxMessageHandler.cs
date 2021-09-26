using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Data.OutBox
{
    public abstract class BaseOutBoxMessageHandler : MediatR.INotificationHandler<OutBoxMessage>
    {
        public abstract Task Handle(OutBoxMessage outBoxMessage, CancellationToken cancellationToken);
        
        protected virtual T RecreateMessage<T>(OutBoxMessage outBoxMessage) where T : class =>
            JsonSerializer.Deserialize(outBoxMessage.Payload,
                System.Type.GetType(outBoxMessage.Type) ?? throw new InvalidOperationException("Message 'Type' should not be null")) as T;
    }
}