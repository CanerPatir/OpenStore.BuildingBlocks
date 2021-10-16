using ApplicationException = OpenStore.Application.Exceptions.ApplicationException;

namespace OpenStore.Infrastructure.Email;

public class MailSenderException : ApplicationException
{
    public MailSenderException()
    {
    }

    public MailSenderException(string message) : base(message)
    {
    }

    public MailSenderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}