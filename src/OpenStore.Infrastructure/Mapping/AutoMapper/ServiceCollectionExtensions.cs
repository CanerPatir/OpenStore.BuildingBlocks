using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application;

namespace OpenStore.Infrastructure.Mapping.AutoMapper;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adding object mapper services to IoC context. It uses AutoMapper library internally 
    /// </summary>
    /// <param name="services">this</param>
    /// <param name="configure">Configuration action</param>
    /// <returns></returns>
    public static IServiceCollection AddOpenStoreObjectMapper(this IServiceCollection services, Action<IMapperConfigurationExpression> configure)
    {
        services
            .AddAutoMapper(configure, assemblies: null, serviceLifetime: ServiceLifetime.Singleton)
            .AddSingleton<IOpenStoreObjectMapper, AutoMapperOpenStoreObjectMapper>();

        return services;
    }
}