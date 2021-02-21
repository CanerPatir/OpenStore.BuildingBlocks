using System;
using MediatR;

namespace OpenStore.Infrastructure.CommandBus
{
    public class RequestSuccessNotification : INotification
    {
        public RequestSuccessNotification(IBaseRequest request)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public IBaseRequest Request { get; }

        public T As<T>() where T: class, IBaseRequest
        {
            return Request as T;
        }
    }
}