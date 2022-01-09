using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;

namespace OpenStore.Infrastructure.CommandBus;

public class NotifyRequestSuccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly IServiceProvider _serviceProvider;

    public NotifyRequestSuccessBehavior(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var response = await next();

        if (request is INotifySuccessRequest or INotifySuccessRequest<TResponse>)
        {
            var httpContextAccessor = _serviceProvider.GetService<IHttpContextAccessor>();
            var currentUrl = httpContextAccessor?.HttpContext?.GetCurrentUrl();
            var userAgent = httpContextAccessor?.HttpContext?.GetUserAgent();
            var claimsPrincipal = httpContextAccessor?.HttpContext?.User;
            PublishAndForget((IBaseRequest) request, response is Unit ? null : response, currentUrl, userAgent, claimsPrincipal);
        }

        return response;
    }

    private void PublishAndForget(IBaseRequest notifySuccessRequest, object response, string currentUrl, string userAgent, ClaimsPrincipal claimsPrincipal)
    {
        Task.Run(async () =>
        {
            using var scope = _serviceProvider.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            await mediator.Publish(new RequestSuccessNotification(notifySuccessRequest, response, currentUrl, userAgent, claimsPrincipal));
        });
    }
}