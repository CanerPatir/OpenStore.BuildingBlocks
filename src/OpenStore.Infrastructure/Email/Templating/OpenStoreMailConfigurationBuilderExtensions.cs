using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;
using OpenStore.Infrastructure.Email.Smtp;
using OpenStore.Infrastructure.Email.Templating;
using RazorLight;

namespace OpenStore.Infrastructure.Email
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