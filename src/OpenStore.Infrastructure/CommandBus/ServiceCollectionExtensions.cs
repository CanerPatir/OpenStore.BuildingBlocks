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
            return services.AddCommandBus(Assembly.GetAssembly(typeof(TAnyHandler)));
        }

        public static IServiceCollection AddCommandBus(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services
                    .AddTransient<IDomainEventNotifier, MediatrDomainEventNotifier>()
                    .AddMediatR(assemblies)
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(NotifyRequestSuccessBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(DisposeBehavior<,>))
                ;
        }
    }
}