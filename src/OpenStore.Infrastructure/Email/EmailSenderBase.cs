using System.Net.Mail;
using System.Text;
using OpenStore.Application.Email;

namespace OpenStore.Infrastructure.Email;

public abstract class EmailSenderBase : IAppEmailSender
{
    public EmailSenderConfiguration Configuration { get; }

    /// <summary>
    /// Constructor.
    /// </summary>
    /// <param name="configuration">Configuration</param>
    protected EmailSenderBase(EmailSenderConfiguration configuration)
    {
        Configuration = configuration;
    }

    public async Task SendEmailAsync(MailBuilder mailBuilder, CancellationToken cancellationToken = default)
    {
        try
        {
            var mailMessage = mailBuilder.Build();
            NormalizeMail(mailMessage);
            await SendEmailAsync(mailMessage, CancellationToken.None);
        }
        catch (MailSenderException)
        {
            throw;
        }
        catch (Exception e)
        {
            throw new MailSenderException("Generic mail sender error. See the inner exception.", e);
        }
    }

    /// <summary>
    /// Should implement this method to send email in derived classes.
    /// </summary>
    /// <param name="mail">Mail to be sent</param>
    /// <param name="cancellationToken"></param>
    protected abstract Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default);

    /// <summary>
    /// Normalizes given email.
    /// Fills <see cref="MailMessage.From"/> if it's not filled before.
    /// Sets encodings to UTF8 if they are not set before.
    /// </summary>
    /// <param name="mail">Mail to be normalized</param>
    protected virtual void NormalizeMail(MailMessage mail)
    {
        if (mail.From == null || string.IsNullOrEmpty(mail.From.Address))
        {
            mail.From = new MailAddress(
                Configuration.DefaultFromAddress,
                Configuration.DefaultFromDisplayName,
                Encoding.UTF8
            );
        }

        mail.HeadersEncoding ??= Encoding.UTF8;
        mail.SubjectEncoding ??= Encoding.UTF8;
        mail.BodyEncoding ??= Encoding.UTF8;
    }
}