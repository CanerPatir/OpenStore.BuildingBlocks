using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp
{
    public class MailKitEmailSender : EmailSenderBase, IAppEmailSender
    {
        private readonly IMailKitSmtpBuilder _smtpBuilder;

        public MailKitEmailSender(IMailKitSmtpBuilder smtpBuilder)
            : base(smtpBuilder.SmtpEmailSenderConfiguration)
        {
            _smtpBuilder = smtpBuilder;
        }

        protected override async Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default)
        {
            using var client = BuildSmtpClient();
            var message = mail.ToMimeMessage();
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        protected virtual SmtpClient BuildSmtpClient() => _smtpBuilder.Build();
    }
}