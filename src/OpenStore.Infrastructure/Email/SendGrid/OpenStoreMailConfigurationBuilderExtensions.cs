using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenStore.Application.Email;
using SendGrid.Extensions.DependencyInjection;

namespace OpenStore.Infrastructure.Email.SendGrid;

public static class OpenStoreMailConfigurationBuilderExtensions
{
    public static IOpenStoreMailConfigurationBuilder UseSendGrid(this IOpenStoreMailConfigurationBuilder builder,
        IConfiguration configuration,
        string configSection = "Mail")
    {
        var services = builder.Services;
            
        services.AddSendGrid((sp, options) =>
        {
            options.ApiKey = sp.GetRequiredService<IOptions<SendGridSenderConfiguration>>().Value.ApiKey;
        });
            
        services
            .Configure<SendGridSenderConfiguration>(configuration.GetSection(configSection))
            .AddTransient<IAppEmailSender, SendGridEmailSender>();

        return builder;
    }

    public static IOpenStoreMailConfigurationBuilder UseSendGrid(this IOpenStoreMailConfigurationBuilder builder,
        SendGridSenderConfiguration sendGridSenderConfiguration)
    {
        var services = builder.Services;

        services.AddSendGrid((sp, options) =>
        {
            options.ApiKey = sp.GetRequiredService<IOptions<SendGridSenderConfiguration>>().Value.ApiKey;
        });
            
        services
            .Configure<SendGridSenderConfiguration>(opts =>
            {
                opts.ApiKey = sendGridSenderConfiguration.ApiKey;
                opts.DefaultFromAddress = sendGridSenderConfiguration.DefaultFromAddress;
                opts.DefaultFromDisplayName = sendGridSenderConfiguration.DefaultFromDisplayName;
            })
            .AddTransient<IAppEmailSender, SendGridEmailSender>();

        return builder;
    }
}