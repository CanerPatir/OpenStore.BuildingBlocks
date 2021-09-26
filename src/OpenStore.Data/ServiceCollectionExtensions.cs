using Microsoft.Extensions.DependencyInjection;
using OpenStore.Data.OutBox;

namespace OpenStore.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreData(this IServiceCollection services)
        {
            services.AddTransient<IOpenStoreOutBoxMessageNotifier, MediatrOpenStoreOutBoxMessageNotifier>();

            return services;
        }
    }
}