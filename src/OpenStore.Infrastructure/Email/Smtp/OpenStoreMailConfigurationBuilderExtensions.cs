using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenStore.Application.Email;

namespace OpenStore.Infrastructure.Email.Smtp;

public static class OpenStoreMailConfigurationBuilderExtensions
{
    public static IOpenStoreMailConfigurationBuilder UseSmtp(this IOpenStoreMailConfigurationBuilder builder,
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

    public static IOpenStoreMailConfigurationBuilder UseSmtp(this IOpenStoreMailConfigurationBuilder builder,
        SmtpEmailSenderConfiguration smtpEmailSenderConfiguration)
    {
        var services = builder.Services;
            
        services
            .Configure<SmtpEmailSenderConfiguration>(opts =>
            {
                opts.Domain = smtpEmailSenderConfiguration.Domain;
                opts.Host = smtpEmailSenderConfiguration.Host;
                opts.Port = smtpEmailSenderConfiguration.Port;
                opts.EnableSsl = smtpEmailSenderConfiguration.EnableSsl;
                opts.UseDefaultCredentials = smtpEmailSenderConfiguration.UseDefaultCredentials;
                opts.UserName = smtpEmailSenderConfiguration.UserName;
                opts.Password = smtpEmailSenderConfiguration.Password;
                opts.DefaultFromAddress = smtpEmailSenderConfiguration.DefaultFromAddress;
                opts.DefaultFromDisplayName = smtpEmailSenderConfiguration.DefaultFromDisplayName;
            })
            .AddTransient<IMailKitSmtpBuilder, DefaultMailKitSmtpBuilder>()
            .AddTransient<IAppEmailSender, MailKitEmailSender>();
            
        return builder;
    }
}