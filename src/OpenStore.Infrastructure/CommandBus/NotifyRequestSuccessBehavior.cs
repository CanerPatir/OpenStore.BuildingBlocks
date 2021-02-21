using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;

namespace OpenStore.Infrastructure.CommandBus
{
    public class NotifyRequestSuccessBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public NotifyRequestSuccessBehavior(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var response = await next();

            if (request is INotifySuccessRequest || request is INotifySuccessRequest<TResponse>)
            {
                PublishAndForget((IBaseRequest) request);
            }

            return response;
        }

        private void PublishAndForget(IBaseRequest notifySuccessRequest)
        {
            Task.Run(async () =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
                await mediator.Publish(new RequestSuccessNotification(notifySuccessRequest));
            });
        }
    }
}