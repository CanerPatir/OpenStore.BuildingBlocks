using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Email;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenStoreMailInfrastructure(this IServiceCollection services,
        Action<IOpenStoreMailConfigurationBuilder> configurationBuilder)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configurationBuilder == null) throw new ArgumentNullException(nameof(configurationBuilder));

        configurationBuilder(new OpenStoreMailConfigurationBuilder(services));

        return services;
    }
}