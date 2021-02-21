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
                .AddOpenStoreCommandBus(assemblies)
                .Scan(x => x.FromAssemblies(assemblies).AddClasses(c => c.AssignableTo(typeof(ICrudService<,>))).AsImplementedInterfaces());
        }
    }
}