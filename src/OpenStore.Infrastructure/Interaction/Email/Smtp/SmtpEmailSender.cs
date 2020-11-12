using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp
{
    /// <summary>
    /// Used to send emails over SMTP.
    /// </summary>
    public class SmtpEmailSender : EmailSenderBase, ISmtpEmailSender
    {
        private readonly SmtpEmailSenderConfiguration _configuration;

        /// <summary>
        /// Creates a new <see cref="SmtpEmailSender"/>.
        /// </summary>
        /// <param name="configuration">Configuration</param>
        public SmtpEmailSender(SmtpEmailSenderConfiguration configuration)
            : base(configuration)
        {
            _configuration = configuration;
        }

        public virtual SmtpClient BuildClient()
        {
            var host = _configuration.Host;
            var port = _configuration.Port;

            var smtpClient = new SmtpClient(host, port);
            try
            {
                if (_configuration.EnableSsl)
                {
                    smtpClient.EnableSsl = true;
                }

                if (_configuration.UseDefaultCredentials)
                {
                    smtpClient.UseDefaultCredentials = true;
                }
                else
                {
                    smtpClient.UseDefaultCredentials = false;

                    var userName = _configuration.UserName;
                    if (!string.IsNullOrEmpty(userName))
                    {
                        var password = _configuration.Password;
                        var domain = _configuration.Domain;
                        smtpClient.Credentials = !string.IsNullOrEmpty(domain)
                            ? new NetworkCredential(userName, password, domain)
                            : new NetworkCredential(userName, password);
                    }
                }

                return smtpClient;
            }
            catch
            {
                smtpClient.Dispose();
                throw;
            }
        }

        protected override async Task SendEmailAsync(MailMessage mail, CancellationToken cancellationToken = default)
        {
            using var smtpClient = BuildClient();
            await smtpClient.SendMailAsync(mail);
        }
    }
}