using System.Net.Mail;

namespace OpenStore.Infrastructure.Interaction.Email.Smtp
{
    /// <summary>
    /// Used to send emails over SMTP.
    /// </summary>
    public interface ISmtpEmailSender : IAppEmailSender
    {
        /// <summary>
        /// Creates and configures new <see cref="SmtpClient"/> object to send emails.
        /// </summary>
        /// <returns>
        /// An <see cref="SmtpClient"/> object that is ready to send emails.
        /// </returns>
        SmtpClient BuildClient();
    }
}