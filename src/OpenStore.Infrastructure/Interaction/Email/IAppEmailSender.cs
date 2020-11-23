using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Infrastructure.Interaction.Email
{
    public interface IAppEmailSender
    {
        Task SendEmailAsync(MailBuilder mailBuilder, CancellationToken cancellationToken = default);
    }
}