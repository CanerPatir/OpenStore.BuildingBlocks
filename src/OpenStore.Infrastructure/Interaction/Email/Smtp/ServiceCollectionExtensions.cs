using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmtp(this IServiceCollection services, IConfigurationRoot configuration, string configurationSectionName = "Mail")
        {
            services.Configure<SmtpEmailSenderConfiguration>(configuration.GetSection(configurationSectionName));
            services.AddTransient<ISmtpEmailSender, SmtpEmailSender>(p => new SmtpEmailSender(p.GetRequiredService<IOptions<SmtpEmailSenderConfiguration>>().Value));
            services.AddTransient<IAppEmailSender>(p => p.GetRequiredService<ISmtpEmailSender>());

            return services;

        }
    }
}