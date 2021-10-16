using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;

namespace OpenStore.Infrastructure.Email.Smtp;

public class DefaultMailKitSmtpBuilder : IMailKitSmtpBuilder
{
    public SmtpEmailSenderConfiguration SmtpEmailSenderConfiguration { get; }

    public DefaultMailKitSmtpBuilder(IOptions<SmtpEmailSenderConfiguration> smtpEmailSenderConfiguration)
    {
        SmtpEmailSenderConfiguration = smtpEmailSenderConfiguration.Value;
    }

    public virtual SmtpClient Build()
    {
        var client = new SmtpClient();

        try
        {
            ConfigureClient(client);
            return client;
        }
        catch
        {
            client.Dispose();
            throw;
        }
    }

    protected virtual void ConfigureClient(SmtpClient client)
    {
        if (string.IsNullOrWhiteSpace(SmtpEmailSenderConfiguration.Host))
        {
            throw new Exception("Smtp host can not be empty.");
        }

        client.Connect(
            SmtpEmailSenderConfiguration.Host,
            SmtpEmailSenderConfiguration.Port,
            SmtpEmailSenderConfiguration.EnableSsl
        );

        if (SmtpEmailSenderConfiguration.UseDefaultCredentials)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(SmtpEmailSenderConfiguration.UserName))
        {
            throw new Exception("Smtp user name can not be empty.");
        }
            
        if (string.IsNullOrWhiteSpace(SmtpEmailSenderConfiguration.Password))
        {
            throw new Exception("Smtp password can not be empty.");
        }

        client.Authenticate(
            SmtpEmailSenderConfiguration.UserName,
            SmtpEmailSenderConfiguration.Password
        );
    }
}

public interface IMailKitSmtpBuilder
{
    SmtpEmailSenderConfiguration SmtpEmailSenderConfiguration { get; }

    SmtpClient Build();
}