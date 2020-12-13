using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;
using OpenStore.Infrastructure.Email.Smtp;
using OpenStore.Infrastructure.Email.Templating;
using RazorLight;

namespace OpenStore.Infrastructure.Email
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreMailInfrastructure(this IServiceCollection services,
            IConfiguration configuration,
            string configSection = "Mail")
        {
            services
                .Configure<SmtpEmailSenderConfiguration>(configuration.GetSection(configSection))
                .AddTransient<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>()
                .AddTransient<IAppEmailSender, MailKitEmailSender>();

            return services;
        }

        public static IServiceCollection AddOpenStoreMailInfrastructureWithTemplate(this IServiceCollection services,
            IConfiguration configuration,
            Assembly embeddedAssembly,
            string embeddedResourceNamespace,
            string configSection = "Mail")
        {
            services
                .AddSingleton(sp => new RazorLightEngineBuilder()
                .UseEmbeddedResourcesProject(embeddedAssembly, embeddedResourceNamespace)
                .UseMemoryCachingProvider()
                .Build());
            
            services.AddSingleton<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            
            services.AddOpenStoreMailInfrastructure(configuration, configSection);

            return services;
        }
    }
}