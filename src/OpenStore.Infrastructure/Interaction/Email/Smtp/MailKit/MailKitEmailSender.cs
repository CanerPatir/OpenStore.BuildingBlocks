using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp.MailKit
{
    public class MailKitEmailSender : EmailSenderBase, IAppEmailSender
    {
        private readonly IMailKitSmtpBuilder _smtpBuilder;

        public MailKitEmailSender(IMailKitSmtpBuilder smtpBuilder)
            : base(smtpBuilder.SmtpEmailSenderConfiguration)
        {
            _smtpBuilder = smtpBuilder;
        }

        public override async Task SendEmailAsync(string from, string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default)
        {
            using var client = BuildSmtpClient();
            var message = BuildMimeMessage(@from, to, subject, body, isBodyHtml);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        protected override async Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default)
        {
            using var client = BuildSmtpClient();
            var message = mail.ToMimeMessage();
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
        }

        protected virtual SmtpClient BuildSmtpClient() => _smtpBuilder.Build();

        private static MimeMessage BuildMimeMessage(string from, string to, string subject, string body, bool isBodyHtml = true)
        {
            var bodyType = isBodyHtml ? "html" : "plain";
            var message = new MimeMessage
            {
                Subject = subject,
                Body = new TextPart(bodyType)
                {
                    Text = body
                }
            };

            message.From.Add(new MailboxAddress(from, from));
            message.To.Add(new MailboxAddress(from, to));

            return message;
        }
    }
}