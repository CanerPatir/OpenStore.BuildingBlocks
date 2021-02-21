using System;
using MediatR;
// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.CommandBus
{
    public class RequestSuccessNotification : INotification
    {
        public RequestSuccessNotification(IBaseRequest request, string currentUrl)
        {
            Request = request ?? throw new ArgumentNullException(nameof(request));
            CurrentUrl = currentUrl;
        }

        public IBaseRequest Request { get; }

        /// <summary>
        /// Null if current context is not and http context
        /// </summary>
        public string CurrentUrl { get; set; }

        public T As<T>() where T: class, IBaseRequest
        {
            return Request as T;
        }
    }
}