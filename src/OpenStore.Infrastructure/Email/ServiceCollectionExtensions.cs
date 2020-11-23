using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;
using OpenStore.Infrastructure.Email.Smtp;
using OpenStore.Infrastructure.Email.Templating;

namespace OpenStore.Infrastructure.Email
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