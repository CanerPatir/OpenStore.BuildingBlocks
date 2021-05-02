using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Crud;
using OpenStore.Infrastructure.CommandBus;

namespace OpenStore.Infrastructure
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreCore(this IServiceCollection services, params  Assembly[] assemblies)
        {
            return services
                .AddOpenStoreCommandBus(ServiceLifetime.Transient, assemblies)
                .Scan(x => x.FromAssemblies(assemblies).AddClasses(c => c.AssignableTo(typeof(ICrudService<,>))).AsImplementedInterfaces());
        }
        
        public static IServiceCollection AddOpenStoreCore(this IServiceCollection services, ServiceLifetime serviceLifetime, params  Assembly[] assemblies)
        {
            return services
                .AddOpenStoreCommandBus(serviceLifetime, assemblies)
                .Scan(x => x.FromAssemblies(assemblies).AddClasses(c => c.AssignableTo(typeof(ICrudService<,>))).AsImplementedInterfaces());
        }
    }
}