using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Web.Spa;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpenStoreSpa(this IServiceCollection services, string rootPath)
    {
        // In production, the Angular files will be served from this directory
        services.AddSpaStaticFiles(configuration => { configuration.RootPath = rootPath; });

        return services;
    }
}