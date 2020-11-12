using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp.MailKit
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMailKitSmtp(this IServiceCollection services, IConfiguration configuration, string configurationSectionName = "Mail")
        {
            services.Configure<SmtpEmailSenderConfiguration>(configuration.GetSection(configurationSectionName));
            services.AddTransient<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>(p => new DefaultMailKitSmtpBuilder(p.GetRequiredService<IOptions<SmtpEmailSenderConfiguration>>().Value));
            services.AddTransient<IAppEmailSender, MailKitEmailSender>(sp => new MailKitEmailSender(sp.GetRequiredService<IMailKitSmtpBuilder>()));

            return services;
        }
    }
}