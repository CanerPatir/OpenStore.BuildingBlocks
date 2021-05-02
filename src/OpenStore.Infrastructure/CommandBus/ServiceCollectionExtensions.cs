using System;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;

namespace OpenStore.Infrastructure.CommandBus
{
    public static class ServiceCollectionExtensions
    {
        internal static IServiceCollection AddOpenStoreCommandBus(this IServiceCollection services, ServiceLifetime lifetime, params Assembly[] assemblies)
        {
            return services
                    .AddTransient<IDomainEventNotifier, MediatrDomainEventNotifier>()
                    .AddMediatR(assemblies, conf =>
                    {
                        switch (lifetime)
                        {
                            case ServiceLifetime.Singleton:
                                conf.AsSingleton();
                                break;
                            case ServiceLifetime.Scoped:
                                conf.AsScoped();
                                break;
                            case ServiceLifetime.Transient:
                                conf.AsTransient();
                                break;
                        }
                    })
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(NotifyRequestSuccessBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>))
                    .AddTransient(typeof(IPipelineBehavior<,>), typeof(DisposeBehavior<,>))
                ;
        }
    }
}