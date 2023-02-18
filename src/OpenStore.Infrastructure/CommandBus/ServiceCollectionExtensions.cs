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

                conf.AddOpenBehavior(typeof(LoggingBehavior<,>));
                conf.AddOpenBehavior(typeof(NotifyRequestSuccessBehavior<,>));
                conf.AddOpenBehavior(typeof(TransactionBehavior<,>));
                conf.AddOpenBehavior(typeof(DisposeBehavior<,>));
            });
    }
}