using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;

namespace OpenStore.Infrastructure.CommandBus
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCommandBus<TAnyHandler>(this IServiceCollection services)
        {
            return services
                    .AddTransient<IEventNotifier, MediatrEventNotifier>()
                    .AddMediatR(Assembly.GetAssembly(typeof(TAnyHandler)))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(DisposeBehavior<,>))
                ;
        }

    }
}