using Microsoft.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Email
{
    public interface IOpenStoreMailConfigurationBuilder
    {
        public IServiceCollection Services { get; }
    }

    internal class OpenStoreMailConfigurationBuilder : IOpenStoreMailConfigurationBuilder
    {
        public OpenStoreMailConfigurationBuilder(IServiceCollection services)
        {
            Services = services;
        }

        public IServiceCollection Services { get; }
    }
}