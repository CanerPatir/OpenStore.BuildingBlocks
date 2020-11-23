using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenStore.Infrastructure.Interaction.Email.Smtp;
using OpenStore.Infrastructure.Interaction.Email.Templating;

namespace OpenStore.Infrastructure.Interaction.Email
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOpenStoreMailInfrastructure(this IServiceCollection services, 
            IConfiguration configuration, 
            bool withTemplateEngine = false,
            string configSection = "Mail")
        {
            if (withTemplateEngine)
            {
                services.AddTransient<IRazorViewToStringRenderer, RazorViewToStringRenderer>();
            }

            services
                .Configure<SmtpEmailSenderConfiguration>(configuration.GetSection(configSection))
                .AddTransient<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>()
                .AddTransient<IAppEmailSender, MailKitEmailSender>();

            return services;
        }
    }
}