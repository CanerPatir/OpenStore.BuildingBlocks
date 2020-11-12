using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Interaction.Email
{
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

        public virtual async Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default)
        {
            await SendEmailAsync(new MailMessage
            {
                To = {to},
                Subject = subject,
                Body = body,
                IsBodyHtml = isBodyHtml
            }, cancellationToken);
        }

        public virtual async Task SendEmailAsync(string from, string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default) => await SendEmailAsync(new MailMessage(@from, to, subject, body) {IsBodyHtml = isBodyHtml}, cancellationToken);

        public virtual async Task SendEmailAsync(MailMessage mail, bool normalize = true, CancellationToken cancellationToken = default)
        {
            if (normalize)
            {
                NormalizeMail(mail);
            }

            await SendEmailAsync(mail, cancellationToken);
        }

        public virtual Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            return SendEmailAsync(new MailBuilder()
                .AddTo(email)
                .Subject(subject)
                .UseHtmlBody()
                .Body(htmlMessage).Build(), CancellationToken.None);
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
}