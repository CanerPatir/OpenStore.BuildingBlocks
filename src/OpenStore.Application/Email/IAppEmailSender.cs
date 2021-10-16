using System.Threading;
using System.Threading.Tasks;

namespace OpenStore.Application.Email;

public interface IAppEmailSender
{
    Task SendEmailAsync(MailBuilder mailBuilder, CancellationToken cancellationToken = default);
}