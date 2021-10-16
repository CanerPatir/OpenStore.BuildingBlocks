using System.Security.Claims;
using MediatR;

// ReSharper disable MemberCanBePrivate.Global

namespace OpenStore.Infrastructure.CommandBus;

public class RequestSuccessNotification : INotification
{
    public RequestSuccessNotification(IBaseRequest request, object response, string currentUrl, string userAgent, ClaimsPrincipal claimsPrincipal)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Response = response;
        CurrentUrl = currentUrl;
        UserAgent = userAgent;
        ClaimsPrincipal = claimsPrincipal;
    }

    public IBaseRequest Request { get; }
        
    /// <summary>
    /// Null if does not exist
    /// </summary>
    public object Response { get; }

    /// <summary>
    /// Null if current context is not http context
    /// </summary>
    public string CurrentUrl { get; }

    public string UserAgent { get; }

    /// <summary>
    /// Null if current context is not http or no authentication context
    /// </summary>
    public ClaimsPrincipal ClaimsPrincipal { get; }

    public T As<T>() where T : class, IBaseRequest
    {
        return Request as T;
    }
}