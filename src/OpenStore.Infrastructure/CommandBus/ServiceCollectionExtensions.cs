using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.CommandBus;

public static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddOpenStoreCommandBus(this IServiceCollection services, ServiceLifetime lifetime, params Assembly[] assemblies)
    {
        return services
                .AddMediatR(conf =>
                {
                    conf.RegisterServicesFromAssemblies(assemblies);
                    conf.Lifetime = lifetime;
                })
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(NotifyRequestSuccessBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))
                .AddTransient(typeof(IPipelineBehavior<,>), typeof(DisposeBehavior<,>))
            ;
    }
}