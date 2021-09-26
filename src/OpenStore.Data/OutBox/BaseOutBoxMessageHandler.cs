using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Data.OutBox
{
    public abstract class BaseOutBoxMessageHandler : MediatR.INotificationHandler<OutBoxMessageBatch>
    {
        public abstract Task Handle(OutBoxMessageBatch messageBatch, CancellationToken cancellationToken);
        
        protected virtual T RecreateMessage<T>(OutBoxMessage outBoxMessage) where T : class =>
            JsonSerializer.Deserialize(outBoxMessage.Payload,
                System.Type.GetType(outBoxMessage.Type) ?? throw new InvalidOperationException("Message 'Type' should not be null")) as T;
    }
}