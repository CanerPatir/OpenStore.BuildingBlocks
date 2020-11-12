using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Interaction.Email
{
    public interface IAppEmailSender
    {
        /// <summary>
        /// Sends an email.
        /// </summary>
        Task SendEmailAsync(string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Sends an email.
        /// </summary>
        Task SendEmailAsync(string from, string to, string subject, string body, bool isBodyHtml = true, CancellationToken cancellationToken = default);
  
        /// <summary>
        /// Sends an email.
        /// </summary>
        /// <param name="mail">Mail to be sent</param>
        /// <param name="normalize">
        /// Should normalize email?
        /// If true, it sets sender address/name if it's not set before and makes mail encoding UTF-8. 
        /// </param>
        Task SendEmailAsync(MailMessage mail, bool normalize = true, CancellationToken cancellationToken = default);
    }
}