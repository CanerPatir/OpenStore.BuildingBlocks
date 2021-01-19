using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;

namespace OpenStore.Infrastructure.Email.Smtp
{
    public static class OpenStoreMailConfigurationBuilderExtensions
    {
        public static IOpenStoreMailConfigurationBuilder WithSmtp(this IOpenStoreMailConfigurationBuilder builder,
            IConfiguration configuration, 
            string configSection = "Mail")
        {
            var services = builder.Services;
            
            services
                .Configure<SmtpEmailSenderConfiguration>(configuration.GetSection(configSection))
                .AddTransient<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>()
                .AddTransient<IAppEmailSender, MailKitEmailSender>();
            
            return builder;
        }
    }
}