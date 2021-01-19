using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using RazorLight;

namespace OpenStore.Infrastructure.Email.Templating
{
    public static class OpenStoreMailConfigurationBuilderExtensions
    {
        public static IOpenStoreMailConfigurationBuilder UseTemplate(this IOpenStoreMailConfigurationBuilder builder,  
            Assembly embeddedAssembly,
            string embeddedResourceNamespace)
        {
            var services = builder.Services;
            
            services
                .AddSingleton(sp => new RazorLightEngineBuilder()
                    .UseEmbeddedResourcesProject(embeddedAssembly, embeddedResourceNamespace)
                    .UseMemoryCachingProvider()
                    .Build());
            
            services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();

            return builder;
        }

    }
}